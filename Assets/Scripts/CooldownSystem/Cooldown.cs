using System;
using UnityEngine;

[System.Serializable]
public class Cooldown {

    // cant be a struct cuz i need reference type

    #region Data 

    [SerializeField][Min(0)] private float duration;
    [SerializeField] private UpdateType updateFrequency;

    public enum UpdateType {

        Frame, // quicker and more reliable, use for input stuff
        Physics // use when duration consistency matters

    }

    #endregion

    #region Constructors

    public Cooldown(float duration, UpdateType updateFrequency) {

        this.duration = duration;
        this.updateFrequency = updateFrequency;

    }

    public Cooldown(float duration) : this(duration, UpdateType.Frame) { }

    #endregion

    #region Accessors

    public float Duration {

        get { return duration; }

    }

    public UpdateType UpdateFrequency {

        get { return updateFrequency; }

    }

    #endregion


}
