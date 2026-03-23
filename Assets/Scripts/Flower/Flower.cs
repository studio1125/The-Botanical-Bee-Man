using System;
using UnityEngine;
using UnityEngine.UI;

public class Flower : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Slider pollinationSlider;
    private BeeController player;

    [Header("Particle References (temp)")]
    [SerializeField] private ParticleSystem nectarParticles;
    [SerializeField] private ParticleSystem pesticideCloseParticles;
    [SerializeField] private ParticleSystem pesticideFarParticles;

    [Header("Settings")]
    [SerializeField, Tooltip("The time it takes for the flower to be fully pollinated")] private float pollinationTime;
    [SerializeField, Tooltip("The time the bee must be in contact before taking pesticide damage")] private float damageTime;
    [SerializeField, Tooltip("Time nectar stays on flower before going on cooldown again")] private Cooldown nectarLifetime; // TODO. add cooldownmanager ? 
    [SerializeField, Tooltip("The amount of nectar the flower provides when fully pollinated")] private int nectarAmount;
    [SerializeField, Tooltip("The range for the time it takes for the flower to regenerate nectar after its lifetime ends")] private Vector2 nectarRegenerationRange;
    [SerializeField, Tooltip("The range for the time it takes for the flower to regenerate nectar after player capture"), Min(0)] private Vector2 nectarRegenerationOnCaptureRange;
    [SerializeField, Tooltip("Radius player must be within of the flower to see pesticideCloseParticles"), Min(0)] private float pesticideViewRadius;
    [SerializeField, Tooltip("chance to spawn with nectar"), Range(0, 1)] private float hasNectarOnSpawnChance;
    [SerializeField, Tooltip("chance to spawn with nectar"), Range(0, 1)] private float hasPesticideChance;
    [SerializeField, Tooltip("chance to spawn nectar after cooldown"), Range(0, 1)] private float spawnNectarChance;
    private float pollinationTimer;
    private float damageTimer;
    private bool hasNectar;
    private bool hasPesticide;

    [Header("Actions")]
    public Action<int> onPollinated; // action to be invoked when the flower is fully pollinated (passes the amount of nectar provided by the flower as an argument)

    private void Start() {

        // TODO: make particles not sync (using simulate(), maybe random seed)
        // TODO: recode ParticlePopups 

        player = FindAnyObjectByType<BeeController>();

        nectarParticles.Stop();
        pesticideCloseParticles.Stop();

        if (UnityEngine.Random.Range(0f, 1f) < hasNectarOnSpawnChance)
            ResetNectar(); // the flower starts with nectar available
        else
            ScheduleResetNectar();

        if (UnityEngine.Random.Range(0f, 1f) < hasPesticideChance) {

            pesticideFarParticles.Play();
            hasPesticide = true;

        }

        else {

            pesticideFarParticles.Stop();
            hasPesticide = false;

        }

        pollinationSlider.gameObject.SetActive(false); // hide the pollination slider at the start

    }

    private void FixedUpdate() {

        // show pesticide particles
        if (hasPesticide) {

            bool playerInRange = Vector2.Distance(player.transform.position, transform.position) < pesticideViewRadius;

            if (!pesticideCloseParticles.isPlaying && playerInRange)
                pesticideCloseParticles.Play();
            else if (!playerInRange)
                pesticideCloseParticles.Stop();

        }

        // nectar expires after fixed time
        if (hasNectar) {

            // wait for cooldown to end

            CooldownManager.TryStart(nectarLifetime);

            if (CooldownManager.FulfillIfComplete(nectarLifetime)) {

                // reset
                ClearNectar();
                ScheduleResetNectar();

            }

        }

    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Bee") && hasNectar) // check if the colliding object is a bee and if the flower has nectar available
            pollinationSlider.value = 0f; // reset the slider value when the bee enters the flower

    }

    private void OnTriggerStay2D(Collider2D collision) {

        if (collision.CompareTag("Bee")) { // check if the colliding object is a bee and if the flower has nectar available

            if (hasNectar) {
                CooldownManager.Pause(nectarLifetime);

                // activate slider if it's not already active (in case the bee entered the flower and then stayed without triggering OnTriggerEnter2D again)
                if (!pollinationSlider.gameObject.activeSelf)
                    pollinationSlider.gameObject.SetActive(true);

                pollinationTimer += Time.deltaTime;
                pollinationSlider.value = pollinationTimer / pollinationTime; // update the slider to show the pollination progress

                if (player.HasSuper() || pollinationTimer >= pollinationTime) { // check if the bee has been pollinating the flower for long enough to fully pollinate it (or player has super)

                    ClearNectar();
                    onPollinated?.Invoke(nectarAmount); // invoke the pollination action to notify other parts of the game that the flower has been fully pollinated
                    ScheduleResetNectar(nectarRegenerationOnCaptureRange);

                }

            }
            if (hasPesticide) {

                damageTimer += Time.deltaTime;

                if (damageTimer >= damageTime) {

                    player.OnHurt(Vector2.Normalize(player.transform.position - transform.position));
                    pollinationTimer = 0;
                    damageTimer = 0;

                }

            }

        }


    }

    private void OnTriggerExit2D(Collider2D collision) {

        if (collision.CompareTag("Bee") && hasNectar) { // check if the colliding object is a bee and if the flower has nectar available

            CooldownManager.Resume(nectarLifetime);

            damageTimer = 0f;
            pollinationTimer = 0f; // reset the timer when the bee leaves the flower
            pollinationSlider.gameObject.SetActive(false); // hide the pollination slider when the bee leaves the flower

        }
    }

    private void ResetNectar() {

        nectarParticles.Play();
        hasNectar = true; // method to reset the nectar availability

    }

    // removes nectar and adjusts state to match
    private void ClearNectar() {

        CooldownManager.Fulfill(nectarLifetime); // perhaps redundant, but end old nectar lifetime cooldown
        nectarParticles.Stop(); // disable pollen particles
        hasNectar = false; // mark as not having pollen
        pollinationTimer = 0f; // reset timers
        damageTimer = 0f;
        pollinationSlider.value = 0; // reset slider
        pollinationSlider.gameObject.SetActive(false); // disable slider


    }

    // schedule the nectar to be reset after a random time within the specified range
    private void ScheduleResetNectar() => Invoke(nameof(ResetNectar), UnityEngine.Random.Range(nectarRegenerationRange.x, nectarRegenerationRange.y));
    private void ScheduleResetNectar(Vector2 range) => Invoke(nameof(ResetNectar), UnityEngine.Random.Range(range.x, range.y));


    private void OnDrawGizmosSelected() {

        // pesticide particles range 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pesticideViewRadius);

    }

}
