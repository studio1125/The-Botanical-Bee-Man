using System;
using System.Collections.Generic;
using UnityEngine;
using SFX;

[Serializable]
public class SFXEntry {

    [SerializeField, Tooltip("Categorization label for this SFXEntry used to keep the library organized")] private SoundType label;
    [SerializeField, Tooltip("Unique identifier used to play this sound effect. Entries cannot share a key.")] private Sound key;
    [SerializeField, Tooltip("PopupSounds associated with this sound. If there are multiple, a random one is selected on play")] private List<PopupSound> sounds;

    public SFXEntry(Sound key) {
        this.key = key;
        sounds = new List<PopupSound>();
    }

    #region Acessors

    public Sound Key {

        get { return key; }

    }

    public List<PopupSound> Sounds {

        get { return sounds; }

    }

    public SoundType Label {

        get { return label; }

    }

    public PopupSound GetSound() => sounds[UnityEngine.Random.Range(0, sounds.Count)];

    #endregion

}
