using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[ExecuteAlways, RequireComponent(typeof(ParticleSystem))]
public class PopupParticles : StrictPopup {

    private ParticleSystem particlePlayer;

    private bool waitingToFinish;

    public override void Initialize() {

        particlePlayer = GetComponent<ParticleSystem>();
        particlePlayer.Stop();

        gameObject.name = "Popup Particles";

    }
    protected override void OnPlay() {

        particlePlayer.Clear();
        particlePlayer.time = 0f;
        particlePlayer.Play();

    }

    protected override void OnFinish() {

        particlePlayer.Stop();

        waitingToFinish = true;

        // base.OnFinish() is called in Update once all particles in the system go away 

    }

    private void Update() {

        if (waitingToFinish && particlePlayer.particleCount == 0) {

            waitingToFinish = false;
            base.OnFinish();

        }

    }

    private void OnValidate() {

        if (!particlePlayer)
            particlePlayer = GetComponent<ParticleSystem>();
        duration = particlePlayer.main.duration;

    }


}