using System.Collections;
using UnityEngine;

public abstract class Popup : MonoBehaviour {

    private Vector3? targetPosition;
    private GameObject targetObject;
    [Header("Default Duration")]
    [SerializeField] protected float duration;

    // get/set all components
    // call this after instantiating a new popup
    public abstract void Initialize();

    // Popup plays either at a GameObject, or at a fixed position (set in PopupPlayer).
    // it will either play forever or for a fixed duration, which can be the default
    //      duration or a custom one
    public IEnumerator HandlePlay(float? overrideDuration = null, GameObject target = null, bool persistent = false) {

        OnPlay();

        float remaining = overrideDuration ?? duration;
        gameObject.SetActive(true);

        if (persistent)
            while (gameObject.activeSelf) {
                if (target != null)
                    transform.position = target.transform.position;

                yield return null;
            }
        else {

            while (remaining > 0f) {
                if (target != null)
                    transform.position = target.transform.position;

                yield return null;
                remaining -= Time.deltaTime;
            }

            OnFinish();

        }

    }

    // deactivate object if its active
    public void StopPlaying() {

        // Popups are deactivated when not playing, and you cant stop a stopped Popup
        if (gameObject.activeSelf)
            OnFinish();

    }

    // repool this Popup. override to enable special behavior on finish.
    // WHEN REIMPLEMENTING, MAKE SURE TO CALL base.OnFinish()!!!!!!!!!!
    protected virtual void OnFinish() {

        gameObject.SetActive(false);

        PopupPlayer.Pool(this);

    }

    // called on play ...
    protected virtual void OnPlay() { }

    public abstract bool IsStrict { get; }

}

// depools exact matches, used for copies that are too heavy or complex (like particle systems)
// faster but uses more memory
public abstract class StrictPopup : Popup {

    public override bool IsStrict => true;

    private int parentId;

    public void SetId(Popup p) => parentId = p.GetInstanceID();

    public int Id {

        get => parentId;

    }

}

// depools matching types then copies state, used for lighter copies (like sprite)
// slower but uses less memory 
public abstract class FlexPopup : Popup {

    public override bool IsStrict => false;

    // switches the component configuration of this popup to that of Popup other
    // ex: for a PopupItem, xwap out the sprite and the runtime animation controller
    public abstract void SwapPopup(Popup popup);

    public System.Type Type {

        get => GetType();

    }


}

// in certain applications, there is no performance difference between flex and strict 