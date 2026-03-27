using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

/// <summary>
/// 
/// A universal and efficient system that handles cooldowns. Static usage. 
/// 
/// </summary>
/// 
/// Usage:
///     This is a singleton object
///     
///     Primary (reccomended):
///         - serialize a cooldown in the inspector
///         - call CooldownManager.TryStart(YourCooldownObject) to start the cooldown
///         
///     Secondary (less reccomended, but more flexible): 
///         - if u want to do a cooldown but can't/don't predefine its duration ... 
///         - call CooldownManager.TryMakeStart(SomeValidDuration) to start a cooldown of any duration
///     
///     Tertiary (untested):
///         - however tf u want
///         
///     in any case, keep track of your Cooldown references (they act as keys). 
///     use HasCompleted(cooldown) to check if a cooldown is done ...
///     use Fulfill(cooldown) once you are "done" with a cooldown ...
///     use Pause(cooldown) to make a cooldown stop counting down ...
///     use TimeRemaining(cooldown) to see time left to finish cd ...
///     etc ...
///     (there are many other useful methods)
///         
/// Scalability:
///     - Script allows for infinite expansion of Cooldown.UpdateType
///         - Steps:
///             - Add enum to Cooldown.UpdateType
///             - Write coroutine for the ticker
///             - Write a function or accessor to return the ticker's delta time 
///                 ( for example, Update() has Time.deltaTime ) 
///             - Update ActiveCooldown.dtime with {NewUpdateType, () => NewDtimeCall()}
///             - Every coroutine tick, call DoUpdate() by passing in the new enum 
///             
/// Performance:
///     - Script avoids the overhead of coroutines
///     - Avoids extra memory allocs using pooling
/// 

// TODO: You CANNOT tick faster than the fps in unity. So what if u want a cooldown thats lower than ur frame time...?
//       ex: super fast firing gun
//       ex: moderately firing gun but its a multiplayer server and so everyone needs the same stuff
//       should the "wasted" time be returned for classes to do what they want with it?
//       ex: thing happens every 0.01 seconds. enough frames are missed to lose 0.03 seconds. return 0.03
//              script can do 0.03/0.01 to determine 3 actions were missed then perform a recovery

public class CooldownManager : MonoBehaviour {

    #region Data

    // singleton
    private static CooldownManager Instance;

    // pool size for set up on awake
    private const int INITIAL_POOL_SIZE = 1000;

    // associates Cooldown instances with corresponding ActiveCooldown instance, sorted by UpdateType
    // update type : (Cooldown instance : associated ActiveCooldown instance)
    private Dictionary<Cooldown.UpdateType, Dictionary<Cooldown, ActiveCooldown>> cooldowns;

    // cooldowns are moved to this dict when they are finished, to check that they are done
    // update type : cooldown instance
    private Dictionary<Cooldown.UpdateType, HashSet<Cooldown>> completedCooldowns;

    // associates *finished* Cooldown instances with corresponding ActiveCooldown instance, sorted by UpdateType
    // this is used to queue finished instances for pooling
    // update type : completed cooldown instances
    private Dictionary<Cooldown.UpdateType, Stack<Cooldown>> cooldownTrash;

    // associates generic cooldown definitions (reusable, non-instance) with matching previously run ActiveCooldown instances
    // generic definition : queue of available ActiveCooldown objects
    private Stack<ActiveCooldown> cdPool;
    // TODO: auto pool cleanup for instances that have not been used in a while

    private class ActiveCooldown {

        #region Data

        // generic cd def
        private Cooldown cdDef;
        // active cd state
        private float timeRemaining;
        private bool paused = false;

        // links each UpdateType to a delta time call for ticking
        static private readonly Dictionary<Cooldown.UpdateType, Func<float>> dtime = new() {

            {Cooldown.UpdateType.Frame, () => Time.deltaTime},
            {Cooldown.UpdateType.Physics, () => Time.fixedDeltaTime}

        };

        #endregion

        #region Construction

        // init is called when this is unpooled
        // default values are safe but useless
        public ActiveCooldown() { }

        public ActiveCooldown(Cooldown genericDef) => Init(genericDef);

        public void Init(Cooldown cdDef) {

            this.cdDef = cdDef;
            this.timeRemaining = cdDef.Duration;
            paused = false;

        }

        #endregion

        #region Behavior

        // ticks the cooldown down based on the update frequency
        // returns true if the cooldown must continue to tick
        // (when the function returns false, the cooldown has finished)
        public bool Tick() {

            if (!paused)
                timeRemaining -= dtime[cdDef.UpdateFrequency]();

            return timeRemaining > 0;

        }

        public void Reset() {

            timeRemaining = cdDef.Duration;

            paused = false;

        }

        public void ForceEnd() => timeRemaining = -1;

        #endregion

        #region Accessors

        public bool Paused {

            get { return paused; }
            set { paused = value; }

        }

        public bool Done {

            get { return timeRemaining <= 0; }

        }

        public float Duration {

            get { return cdDef.Duration; }

        }

        public float TimeLeft {

            get { return Mathf.Max(timeRemaining, 0); }

        }

        #endregion

    }

    #endregion

    #region Initialization

    private void Awake() {

        // singleton stuff
        // delete every CooldownManager except the first one that compiles(?)
        if (Instance != null && Instance != this) { // if a dif instance has already been defined

            Destroy(this.gameObject);

            return;

        }

        Instance = this;

        // initialize data
        cooldowns = new();
        completedCooldowns = new();
        cooldownTrash = new();
        cdPool = new();

        // initialize substructures
        foreach (Cooldown.UpdateType type in System.Enum.GetValues(typeof(Cooldown.UpdateType))) {

            cooldowns[type] = new();
            cooldownTrash[type] = new();
            completedCooldowns[type] = new();

        }

        // initialize pool
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            PoolInstance(new ActiveCooldown());

    }

    #endregion

    #region Cooldown Actions

    // after a cooldown is STARTED, it automatically begins being TRACKED.
    // TRACKED cooldowns can be MANAGED.
    // a cooldown STOPS being TRACKED when you call Fulfill(cooldown)
    //      (the next TRACKING tick pools the cooldown, where it waits to be started again) 

    #region Starting Cooldowns

    /// <summary>
    /// Run a predefined Cooldown by passing in a Cooldown instance, and check if elapsed
    /// <br/> Does not restart exsiting cooldown if found
    /// </summary>
    /// <param name="cooldown"></param>
    public static void TryStart(Cooldown cooldown) => StartCooldownIfAbsent(cooldown);

    /// <summary>
    /// Run a Cooldown without a definition, passing in the desired duration (and UpdateType) as params.
    /// <br/> Not reccomended for use over TryStart() but offers flexibility <i>(ex: when cooldown time is calculated/determined at runtime)</i>
    /// <br/> <br/><b>You must pass in a Cooldown by reference.</b> If it is null, it will be replaced with a new Cooldown object using the other params.
    /// <br/> <b>Losing the reference will lead to memory leaks and unreachable data.</b> Hold onto the Cooldown instance to manage the cooldown.
    /// </summary>
    /// <param name="cd"></param>
    /// <param name="duration"></param>
    /// <param name="updFreq"></param>
    public static void TryMakeStart(ref Cooldown cd, float duration, Cooldown.UpdateType updFreq = Cooldown.UpdateType.Frame) {

        if (duration < 0)
            return;

        cd ??= new Cooldown(duration, updFreq);

        StartCooldownIfAbsent(cd);

    }

    /// <summary>
    /// Play a predefined Cooldown by passing in a Cooldown instance. If the instance already has a
    ///     running cooldown, that cooldown will be restarted and unpaused. 
    /// <br/> If run on a cooldown that found and is done but unfulfilled, the cooldown will no longer be marked as complete.
    /// </summary>
    /// <param name="cooldown"></param>
    public static void ForceStart(Cooldown cooldown) {

        if (cooldown == null)
            return;

        // if cooldown is currently being played, reset it
        if (Instance.cooldowns[cooldown.UpdateFrequency].ContainsKey(cooldown)) {

            Instance.cooldowns[cooldown.UpdateFrequency][cooldown].Reset();
            Instance.completedCooldowns[cooldown.UpdateFrequency].Remove(cooldown); // failsafe, unsure if necessary

        }
        // if cooldown is not currently played, access an instance of ActiveCooldown and add it to the list of
        //      currently running cooldowns
        else
            Instance.cooldowns[cooldown.UpdateFrequency][cooldown] = GetActiveCooldownInstance(cooldown);

    }

    // start a cooldown if it is not already started and running 
    private static void StartCooldownIfAbsent(Cooldown cooldown) {

        if (cooldown == null)
            return;

        // check if cooldown exists alredy, if so then access its associated ActiveCooldown 
        Instance.cooldowns[cooldown.UpdateFrequency].TryGetValue(cooldown, out ActiveCooldown instance);

        // only create new if existing one is null 
        instance ??= Instance.cooldowns[cooldown.UpdateFrequency][cooldown] = GetActiveCooldownInstance(cooldown);


    }

    /*/// <summary>
    /// Run a "custom" cooldown for when you do not already have a stored Cooldown instance. 
    /// <br/>Generally not reccomended over HandleCooldown(), but offers flexibility. Common use case is cooldowns
    ///     with variable durations (determined/calculated during runtime)
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="updateFrequency"></param>
    /// <returns>Cooldown instance; script should hold onto this for proper usage 
    ///     (passing same instance into other functions) and to prevent memory leaks and stale cooldowns. 
    ///     null if invalid duration</returns>
    public static Cooldown MakeStartCooldown(float duration, Cooldown.UpdateType updateFrequency = Cooldown.UpdateType.Frame) {

        if (duration <= 0)
            return null;

        Cooldown ret = new Cooldown(duration, updateFrequency);

        TryStartCooldown(ret);

        return ret;

    }*/

    #endregion

    #region Tracking Cooldowns

    // Update() and FixedUpdate() handle cooldowns based on the cooldown's defined UpdateType

    private void Update() => DoUpdate(Cooldown.UpdateType.Frame);

    private void FixedUpdate() => DoUpdate(Cooldown.UpdateType.Physics);

    private static void DoUpdate(Cooldown.UpdateType updType) {

        // get all cooldowns of update type
        var cooldowns = Instance.cooldowns[updType];
        // get all to-be-pooled (finished and checked) cooldowns of update type
        var cooldownTrash = Instance.cooldownTrash[updType];

        //print(cooldowns.Count);

        // remove and pool completed cooldowns
        // cooldowns are moved to trash when they are checked for completion
        while (cooldownTrash.Count != 0) {

            Cooldown toPool = cooldownTrash.Pop(); // remove from trash

            if (cooldowns.ContainsKey(toPool)) { // failsafe

                PoolInstance(cooldowns[toPool]); // add instance to pool
                cooldowns.Remove(toPool);

            }

        }

        // tick all cooldowns of type and categorize as completed if needed
        // "pair" is {Cooldown : ActiveCooldown}
        foreach (var pair in cooldowns) // iterate through every cooldown 
            if (!pair.Value.Tick()) // tick the cooldown instance and check if done 
                Instance.completedCooldowns[updType].Add(pair.Key); // mark as done

    }

    #endregion

    #region Managing Cooldowns

    /// <summary>
    /// Check if a cooldown is done. 
    /// <br/> After determining that a cooldown has finished, and you are done with the cooldown, additionally call Fulfill() 
    /// <br/> <i>(ex: for an ability on a cooldown, call Fulfill() when the input is pressed and this function returns true)</i>
    /// <br/><br/> Omitting a call to Fulfill() means the cooldown will stay alive and marked as complete
    /// </summary>
    /// <param name="cooldown"></param>
    /// <returns>true if cooldown is done, false if incomplete or unrecognized</returns>
    public static bool IsComplete(Cooldown cooldown) {

        if (cooldown == null || !HasCooldown(cooldown))
            return false;

        return Instance.completedCooldowns[cooldown.UpdateFrequency].Contains(cooldown);


    }

    /// <summary>
    /// Shortcut that combines IsComplete() and Fulfill(). Cooldown is automatically fulfilled if it is done running.
    /// <br/> Use when an action is run the instant the cooldown expires, every time.
    /// <br/> <i>(ex: for an action that runs over and over on a fixed clock, call this in place of IsOnCooldown(), and skip calling Fulfill())</i>
    /// </summary>
    /// <param name="cooldown"></param>
    /// <returns>true if cooldown is done running and was fulfilled, false if cooldown is unfinished, unrecognized, or null</returns>
    public static bool FulfillIfComplete(Cooldown cooldown) {

        if (IsComplete(cooldown)) {

            Fulfill(cooldown);

            return true;

        }

        return false;
    }

    /// <summary>
    /// Mark a cooldown as done, removing it from the manager. Also can be used to end a cooldown early.
    /// <br/> Tell a cooldown that it has "fulfilled" its role (is done running) 
    /// </summary>
    /// <param name="cooldown"></param>
    /// <param name="paused"></param>
    public static void Fulfill(Cooldown cooldown) {

        if (cooldown == null || !HasCooldown(cooldown))
            return;

        Cooldown.UpdateType updType = cooldown.UpdateFrequency;

        Instance.completedCooldowns[updType].Remove(cooldown);

        Instance.cooldownTrash[updType].Push(cooldown);

        // failsafe
        if (HasCooldown(cooldown))
            Instance.cooldowns[updType][cooldown].ForceEnd();

    }

    /// <summary>
    /// Check if a Cooldown instance is recognized by the manager. The cooldown may or may not be complete.
    /// </summary>
    /// <param name="cooldown"></param>
    /// <returns>true if the cooldown has been started by the manager and has not yet been checked for completion (removed),
    ///     false if null param or cooldown is not active in the manager</returns>
    public static bool HasCooldown(Cooldown cooldown) {

        if (cooldown == null)
            return false;

        foreach (var key in Instance.cooldowns.Keys)
            if (Instance.cooldowns[key].ContainsKey(cooldown))
                return true;

        return false;

    }

    /// <summary>
    /// Set a cooldown to be frozen to make it stop counting down
    /// </summary>
    /// <param name="cooldown"></param>
    /// <param name="paused"></param>
    public static void SetCooldownPaused(Cooldown cooldown, bool paused) {

        if (HasCooldown(cooldown))
            Instance.cooldowns[cooldown.UpdateFrequency][cooldown].Paused = paused;

    }

    /// <summary>
    /// Make a cooldown stop counting down
    /// </summary>
    /// <param name="cooldown"></param>
    /// <param name="paused"></param>
    public static void Pause(Cooldown cooldown) {

        if (HasCooldown(cooldown)) {

            ActiveCooldown activeCd = Instance.cooldowns[cooldown.UpdateFrequency][cooldown];

            if (!activeCd.Paused)
                activeCd.Paused = true;

        }

    }

    /// <summary>
    /// Make a cooldown resume counting down (if paused)
    /// </summary>
    /// <param name="cooldown"></param>
    /// <param name="paused"></param>
    public static void Resume(Cooldown cooldown) {

        if (HasCooldown(cooldown)) {

            ActiveCooldown activeCd = Instance.cooldowns[cooldown.UpdateFrequency][cooldown];

            if (activeCd.Paused)
                activeCd.Paused = false;

        }

    }

    /// <summary>
    /// Check how much time is left on a cooldown
    /// </summary>
    /// <param name="cooldown"></param>
    /// <param name="paused"></param>
    /// <returns>time in seconds the cooldown has left to run. 0 if complete (or null) </returns>
    public static float TimeRemaining(Cooldown cooldown) {

        if (HasCooldown(cooldown))
            return Instance.cooldowns[cooldown.UpdateFrequency][cooldown].TimeLeft;

        return 0f;

    }

    #endregion

    #endregion

    #region ActiveCooldown Instance Pooling

    // returns a pooled cooldown if one exists, otherwise returns a new cooldown
    private static ActiveCooldown GetActiveCooldownInstance(Cooldown cooldown) {


        if (Instance.cdPool.Count != 0) {

            Instance.cdPool.Peek().Init(cooldown);
            ActiveCooldown ret = Instance.cdPool.Pop();

            return ret;

        }

        return new ActiveCooldown(cooldown);

    }

    // instance should never be null ... anyways only this class can create ActiveCooldown objects
    private static void PoolInstance(ActiveCooldown cdInstance) => Instance.cdPool.Push(cdInstance);

    #endregion

}

