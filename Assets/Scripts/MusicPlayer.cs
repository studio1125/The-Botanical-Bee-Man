using UnityEngine;
using SFX;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(AudioPlayer))]
public class MusicPlayer : MonoBehaviour {

    [SerializeField] private List<Sound> playlist;

    private AudioPlayer audioPlayer;
    private Cooldown currentSongWait;
    int playingIdx = 0;

    private void Start() => audioPlayer = GetComponent<AudioPlayer>();

    private void FixedUpdate() {

        if (playlist.Count == 0)
            return;

        // reset count
        if (playingIdx >= playlist.Count)
            playingIdx = 0;

        // if not playing song, play a song and set it as being played
        if (!CooldownManager.HasCooldown(currentSongWait)) {

            float songDuration = audioPlayer.Play(playlist[playingIdx]);

            CooldownManager.TryMakeStart(ref currentSongWait, songDuration);

        }
        if (CooldownManager.FulfillIfComplete(currentSongWait)) {

            //audioPlayer.Stop(playlist[playingIdx]); //failsafe
            playingIdx++; // next tick will play next song

        }

    }




}
