using System.Collections.Generic;
using UnityEngine;
using SFX;

public class AudioPlayer : MonoBehaviour {

    /// <summary>
    /// This class keeps all audio-related code in one place for organization and standardization
    /// 
    ///     - Anything in the world that wants to play audio will utilize an AudioPlayer, 
    ///     most likely located on the object itself. 
    ///     
    ///     - The Popup system is being used here to have an optimized way to have as many audio
    ///     sources as needed per object at any time dynamically (pooling). 
    ///     
    ///     - AudioPlayer uses SFXLib as a dictionary of enums and Popups. Scripts only need to 
    ///     define which enums they want to use, and AudioPlayer handles getting the Popup by
    ///     using SFXLib 
    /// 
    /// </summary>

    private SFXLib sfx; // TODO: make into singleton

    private HashSet<PopupSound> currentlyPlaying = new HashSet<PopupSound>();

    void Awake() => sfx = FindAnyObjectByType<SFXLib>();

    /// <summary>
    /// Play a sound
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="stopCurrentMatch"></param>
    /// <param name="looping"></param>
    /// <param name="target"></param>
    public void Play(Sound sound, bool stopCurrentMatch = false, bool looping = false, GameObject target = null) {

        // get the popup prefab
        PopupSound toPlay = sfx.GetPopup(sound);

        if (toPlay != null) {

            if (stopCurrentMatch)
                Stop(sound);

            // play the popup and get the reference to the GameObject
            PopupSound playing = PopupPlayer.Play(toPlay, (target ? target : gameObject), looping) as PopupSound;

            // start tracking this sound
            currentlyPlaying.Add(playing);

            // tell the PopupSound which AudioPlayer created it, so the PopupSound can later tell this AudioPlayer to stop tracking it when it finishes playing
            playing.SetOwner(this);
            // let it know what its key is
            // consider removing the key field in PopupSound and rely entirely on SFXLib.GetKey();
            playing.SetKey(sound);

        }
        else
            Debug.LogError($"SFXLib: No PopupSound defined for SFXLib.Sounds.{sound}.");

    }

    /// <summary>
    /// attempt to stop a sound of a given key. fails if no sound of that key is being tracked by this AudioPlayer
    /// </summary>
    /// <param name="key"></param>
    public void Stop(Sound key) {

        // check if a PopupSound this AudioPlayer is tracking has the given key
        foreach (PopupSound playing in currentlyPlaying)
            if (playing.GetKey() == key) {

                // stop the sound from playing. Note that this will call Popup.OnFinish() which calls this.OnSoundComplete()
                playing.StopPlaying();
                break;

            }

    }


    /// <summary>
    /// like Stop, but this is only used when we know for a fact that this specific PopupSound is playing and being tracked
    ///  by this AudioPlayer. For example, it is called when a PopupSound finishes playing
    /// </summary>
    /// <param name="sound"></param>
    public void OnSoundComplete(PopupSound sound) {

        if (sound != null)
            currentlyPlaying.Remove(sound);

    }

    /// <summary>
    /// Get a list of all sounds currently being played by this AudioPlayer 
    /// </summary>
    /// <returns></returns>
    public List<PopupSound> Playing() => new List<PopupSound>(currentlyPlaying);

}
