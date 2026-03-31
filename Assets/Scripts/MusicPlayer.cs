using UnityEngine;
using SFX;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(AudioPlayer))]
public class MusicPlayer : MonoBehaviour {

    [SerializeField] private Sound song;

    private AudioPlayer audioPlayer;

    private void Start() {

        audioPlayer = GetComponent<AudioPlayer>();

        audioPlayer.Play(song);

        Invoke(nameof(RepeatTrack), audioPlayer.Playing()[0].Duration);

    }


    private void RepeatTrack() {

        audioPlayer.Play(song);

        Invoke(nameof(RepeatTrack), audioPlayer.Playing()[0].Duration);

    }

}
