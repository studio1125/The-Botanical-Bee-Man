using System;
using UnityEngine;
using UnityEngine.UI;

public class Flower : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private Slider pollinationSlider;

    [Header("Settings")]
    [SerializeField, Tooltip("The time it takes for the flower to be fully pollinated")] private float pollinationTime;
    [SerializeField, Tooltip("The amount of nectar the flower provides when fully pollinated")] private int nectarAmount;
    [SerializeField, Tooltip("The range for the time it takes for the flower to regenerate nectar after being fully pollinated")] private Vector2 nectarRegenerationRange;
    private float pollinationTimer;
    private bool hasNectar;

    [Header("Actions")]
    public Action<int> onPollinated; // action to be invoked when the flower is fully pollinated (passes the amount of nectar provided by the flower as an argument)

    private void Start() {

        hasNectar = true; // the flower starts with nectar available
        pollinationSlider.gameObject.SetActive(false); // hide the pollination slider at the start

    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Bee") && hasNectar) // check if the colliding object is a bee and if the flower has nectar available
            pollinationSlider.value = 0f; // reset the slider value when the bee enters the flower

    }

    private void OnTriggerStay2D(Collider2D collision) {

        if (collision.CompareTag("Bee") && hasNectar) { // check if the colliding object is a bee and if the flower has nectar available

            // activate slider if it's not already active (in case the bee entered the flower and then stayed without triggering OnTriggerEnter2D again)
            if (!pollinationSlider.gameObject.activeSelf)
                pollinationSlider.gameObject.SetActive(true);

            pollinationTimer += Time.deltaTime;
            pollinationSlider.value = pollinationTimer / pollinationTime; // update the slider to show the pollination progress

            if (pollinationTimer >= pollinationTime) { // check if the bee has been pollinating the flower for long enough to fully pollinate it

                pollinationTimer = 0f; // reset the timer for the next pollination
                hasNectar = false;
                pollinationSlider.gameObject.SetActive(false); // hide the pollination slider when the flower is fully pollinated
                onPollinated?.Invoke(nectarAmount); // invoke the pollination action to notify other parts of the game that the flower has been fully pollinated
                Invoke(nameof(ResetNectar), UnityEngine.Random.Range(nectarRegenerationRange.x, nectarRegenerationRange.y)); // schedule the nectar to be reset after a random time within the specified range

            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {

        if (collision.CompareTag("Bee") && hasNectar) { // check if the colliding object is a bee and if the flower has nectar available

            pollinationTimer = 0f; // reset the timer when the bee leaves the flower
            pollinationSlider.gameObject.SetActive(false); // hide the pollination slider when the bee leaves the flower

        }
    }

    private void ResetNectar() => hasNectar = true; // method to reset the nectar availability

}
