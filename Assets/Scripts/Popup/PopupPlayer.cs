using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPlayer : MonoBehaviour {

    /// <summary>
    /// 
    /// used to play popups
    /// 
    /// </summary>
    /// 
    /// ideally, this script should handle everything, so other classes only ever need one simple call to PlayPopup
    /// also handles pooling popups
    /// 
    /// use this to create a popup object that appears at a location for a time
    /// once the time runs out, the popup object is deactivated and added to the pool
    /// later calls to PlayPopup will prefer reactivating a pooled popup over making a new one
    /// new popup objects are only created when nothing in the pool is available for use
    /// 
    /// PopupPlayer also creates a bunch of Popups on Start, saving memory.
    /// 

    [Header("Singleton")]
    private static PopupPlayer Instance;

    [Serializable]
    public class PopupPoolConfigurator {

        [SerializeField] private Popup popup;
        [SerializeField] private int numToSpawn;

        public int NumToSpawn {

            get { return numToSpawn; }

        }

        public Popup Popup {

            get { return popup; }

        }

    }

    [Header("Pool")]
    [SerializeField][Tooltip("All popups are children of this GameObject's transform")] private GameObject poolParent;
    [SerializeField][Tooltip("Reccomended to leave this on unless using really expensive popups or are in a very memory-tight scenario")] private bool infinitePool;
    [SerializeField][Tooltip("Will start deleting Popups after the pool reaches this size")][Range(0, 1000)] private int maxPoolSize;
    [SerializeField][Tooltip("Use to spawn pooled popups on Start. Each entry should contain a different type of Popup Prefab, probably the ones in /[Prefabs]/Popups/Init")] private PopupPoolConfigurator[] poolInit;

    [Header("Debug")]
    [SerializeField] private bool logMetrics;
    private int trackedMaxSize;
    private float timeSinceLastMax;
    private Stack<String> logs;
    private Coroutine logWaitCoroutine;
    private const float logWaitDuration = 0.5f;

    private class PopupPool {

        // associates FlexPopup subclasses with queues of Popups of only that FlexPopup type
        private Dictionary<Type, Queue<FlexPopup>> flexPool;
        // associates prefab instance id to 
        private Dictionary<int, Queue<StrictPopup>> strictPool;

        private int size;

        public PopupPool() {

            flexPool = new();
            strictPool = new();

            size = 0;

        }

        public bool Contains(Popup popup) {

            Type type = popup.GetType();
            int id = popup.GetInstanceID();

            return (flexPool.ContainsKey(type) && flexPool[type].Count != 0)
                || (strictPool.ContainsKey(id) && strictPool[id].Count != 0);

        }

        // try to get a pooled item of a specific Popup type 
        // for type, pass in a child class of Popup (ex: PopupIcon)
        // popups are dequeued on play, requeued when told to by the popup
        // check that a Queue of that type exists in the pool dict, and that it has stuff in it. if so, give me something from it!!
        public Popup Depool(Popup popup) {

            Type type = popup.GetType();
            int id = popup.GetInstanceID();

            Popup ret = null;

            if (popup is FlexPopup)
                ret = flexPool.TryGetValue(type, out var typeQueue) && typeQueue.Count > 0
                    ? typeQueue.Dequeue() : null;

            else if (popup is StrictPopup strict)
                ret = strictPool.TryGetValue(id, out var idQueue) && idQueue.Count > 0
                    ? idQueue.Dequeue() : null;

            if (ret)
                size--;

            return ret;

        }

        public void Push(Popup popup) {

            if (popup is FlexPopup flex) {

                // if this type hasnt been seen yet
                if (!flexPool.TryGetValue(flex.Type, out var typeQueue))
                    flexPool[flex.Type] = typeQueue = new();

                typeQueue.Enqueue(popup as FlexPopup);

            }

            else if (popup is StrictPopup strict) {


                // if this type hasnt been seen yet
                if (!strictPool.TryGetValue(strict.Id, out var idQueue))
                    strictPool[strict.Id] = idQueue = new();

                idQueue.Enqueue(popup as StrictPopup);

            }

            size++;

        }

        public int Count {

            get => size;

        }

    }
    private static PopupPool pool;

    #region Pooling 

    private void Awake() {

        // singleton stuff
        // delete every CooldownManager except the first one that compiles(?)
        if (Instance != null && Instance != this) { // if a dif instance has already been defined

            Destroy(this.gameObject);

            return;

        }

        Instance = this;

        pool = new();
        logs = new Stack<String>();
        trackedMaxSize = 0;
        timeSinceLastMax = 0;

        if (!infinitePool) {

            int initPoolSize = 0;

            foreach (PopupPoolConfigurator config in poolInit)
                initPoolSize += config.NumToSpawn;

            if (maxPoolSize < initPoolSize) {
                Debug.LogWarning("The max PopupPlayer pool size is lower than the configured initial pool size: the max pool size has been automatically adjusted.");
                maxPoolSize = initPoolSize;
            }

        }

        // spawn initial pool
        foreach (PopupPoolConfigurator config in poolInit) {

            trackedMaxSize += config.NumToSpawn;

            for (int i = 0; i < config.NumToSpawn; i++) {

                Popup spawned = SpawnNewPopup(config.Popup);
                pool.Push(spawned);
                spawned.gameObject.SetActive(false);

            }

        }

    }

    private void Update() {

        // destroy pooled objects when max pool size is reached, and log metrics

        // don't waste time on any of Update if the pool is infinite and you don't want metrics
        // the null check on pool is mainly just because i don't want this to run with [AlwaysExecute], i just want OnValidate
        if (pool != null && (logMetrics || !infinitePool)) {

            if (logMetrics && pool.Count > trackedMaxSize) {

                trackedMaxSize = pool.Count;

                logs.Push("<b>New max Popup pool size reached:</b> " + trackedMaxSize + ". Previous peak occured " + ("" + timeSinceLastMax).Substring(4) + "s ago.");

                if (logWaitCoroutine == null)
                    logWaitCoroutine = StartCoroutine(WaitToLogMetrics());

                timeSinceLastMax = 0;

            }

            timeSinceLastMax += Time.deltaTime;
        }

    }

    private static IEnumerator WaitToLogMetrics() {

        int prevLogCount = Instance.logs.Count;
        float totalWait = 0f;

        // wait until no new logs are being added
        while (true) {

            yield return new WaitForSeconds(logWaitDuration);
            totalWait += logWaitDuration;

            if (prevLogCount == Instance.logs.Count)
                break;

            prevLogCount = Instance.logs.Count;

        }

        Debug.Log(Instance.logs.Pop() + " This peak occured " + totalWait + "s ago.");
        Instance.logs.Clear();

        Instance.logWaitCoroutine = null;

    }

    // spawn and initialize a new Popup, then pool it. returns the spawned Popup. it is inactive by default
    private static Popup SpawnNewPopup(Popup popup) {

        Popup spawned = Instantiate(popup, Instance.poolParent.transform);

        // set the passed in prefab to be the identifier for this strict popup instance
        if (spawned is StrictPopup strict)
            strict.SetId(popup);

        spawned.Initialize();

        return spawned;

    }
    public static void Pool(Popup popup) {

        // if this popup cannot be accomodated, get rid of it
        // TODO: figure out better, faster system 
        if (!Instance.infinitePool && pool.Count >= Instance.maxPoolSize)
            Destroy(popup);
        else
            pool.Push(popup);

    }

    #endregion

    #region Playing

    // All methods return the Popup being played
    // What you pass in and how it changes behavior:
    //      Vector3 position -> fixed position to play Popup at
    //      GameObject target -> make the Popup always stick to a certain object so the position can change
    //      float overrideDuration -> duration to play the popup for, overriding the default duration
    //      bool persistent -> set to true for an infinitely playing popup (to be pooled manually), false to play for default duration

    // Play a popup at a FIXED POSITION for a FIXED DURATION
    public static Popup Play(Popup popup, Vector3 position, float duration) => Play(popup, fixedPosition: position, overrideDuration: duration);

    // play a popup at a CHANGING LOCATION (sticking to a GameObject) for a FIXED DURATION
    public static Popup Play(Popup popup, GameObject targetObj, float duration) => Play(popup, target: targetObj, overrideDuration: duration);

    // play a popup at a FIXED POSITION
    // if persistent = true it will play forever unless manually stopped
    // if persistent = false it will play for the default duration
    public static Popup Play(Popup popup, Vector3 position, bool persistent) => Play(popup, fixedPosition: position, persistent: persistent);

    // play a popup at a FIXED POSITION
    // if persistent = true it will play forever unless manually stopped
    // if persistent = false it will play for the default duration
    public static Popup Play(Popup popup, GameObject targetObj, bool persistent) => Play(popup, target: targetObj, persistent: persistent);


    // "Master method" for playing popups
    //  popup is the only required field 
    private static Popup Play(Popup popup, Vector3? fixedPosition = null, GameObject target = null, float? overrideDuration = null, bool persistent = false) {

        // tries to access popup from pool, returns null if none found 
        // if flex popup, returns any popup of matching type
        // if strict popup, returns any identical popup 
        Popup toPlay = pool.Depool(popup);

        // got popup from pool
        if (toPlay != null && toPlay is FlexPopup toModify)
            toModify.SwapPopup(popup as FlexPopup);
        else if (toPlay == null)
            toPlay = SpawnNewPopup(popup);

        if (target != null)
            toPlay.transform.position = target.transform.position;
        else if (fixedPosition.HasValue)
            toPlay.transform.position = fixedPosition.Value;

        Instance.StartCoroutine(toPlay.HandlePlay(overrideDuration, target, persistent));

        return toPlay;

    }

    public static bool Stop(Popup popup) {

        if (popup && popup.isActiveAndEnabled) {

            popup.StopPlaying();
            return true;

        }

        return false;

    }

    public static void Stop(ref Popup instance) {

        Stop(instance);
        instance = null;

    }

    #endregion

    #region Custom Editor

#if UNITY_EDITOR

    // hide the maxPoolSize field if infinitePool = true
    // using UnityEditor prefix to avoid needing to hide the import in the final build -vv
    [UnityEditor.CustomEditor(typeof(PopupPlayer), true)]
    public class PopupPlayerEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {

            serializedObject.Update();

            // make sure its in the right order
            UnityEditor.SerializedProperty poolParentProp = serializedObject.FindProperty("poolParent");
            UnityEditor.SerializedProperty infinitePoolProp = serializedObject.FindProperty("infinitePool");
            UnityEditor.SerializedProperty maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
            UnityEditor.SerializedProperty poolInitProp = serializedObject.FindProperty("poolInit");
            UnityEditor.SerializedProperty logMetricsProp = serializedObject.FindProperty("logMetrics");

            UnityEditor.EditorGUILayout.PropertyField(poolParentProp);

            UnityEditor.EditorGUILayout.PropertyField(infinitePoolProp);

            if (!infinitePoolProp.boolValue)
                UnityEditor.EditorGUILayout.PropertyField(maxPoolSizeProp);

            UnityEditor.EditorGUILayout.PropertyField(poolInitProp, true);
            UnityEditor.EditorGUILayout.PropertyField(logMetricsProp);

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif

    #endregion

}
