using SFX;
using UnityEngine;

public class CooldownTest : MonoBehaviour {

    [Header("1")]
    [SerializeField] private Cooldown cd1;
    [SerializeField] private Sound sound1;

    [Header("2")]
    [SerializeField] private Cooldown cd2;
    [SerializeField] private Sound sound2;

    [Header("3")]
    [SerializeField, Min(0)] private float time;
    private Cooldown cd3 = null;
    [SerializeField] private Sound sound3;

    [Header("stuff")]
    [SerializeField] private Popup indicator;
    private AudioPlayer audioPlayer;
    private BeeController player;

    private void Start() {

        audioPlayer = GetComponent<AudioPlayer>();
        player = FindFirstObjectByType<BeeController>();
        // side note: this is completely terrible usage of PopupPlayer because now i have an innacessible Popup 
        //      but its ok cuz this script is full of weird code 
        PopupPlayer.Play(indicator, gameObject, true);

    }

    private void Update() {

        float dist = Vector3.Distance(player.transform.position, transform.position);

        // (if in certain range of object) wait until cd finishes, play a sound, repeat

        if (dist < 4) {


            CooldownManager.TryStart(cd1); // runs a cooldown if it is not already running, otherwise does nothing

            // the above function calls are very safe. if cd1 is not playing or is already unpaused, nothing happens
            //      and the performance overhead is entirely negligible

            if (CooldownManager.FulfillIfComplete(cd1)) // checks if the cooldown is done then automatically Fulfills it
                audioPlayer.Play(sound1);

        }
        else
            CooldownManager.Fulfill(cd1); // when out of range, i fully stop the timer, so when i go back in range it counts from 0

        if (dist < 8) {

            // i want to calculate the duration at runtime, so i have to use TryMakeStart
            //      and pass in a cd by ref to promise i am handling it
            CooldownManager.TryMakeStart(ref cd3, Random.Range(0.3f, 3));
            CooldownManager.Resume(cd3); // i unpause the timer
            // this call (and every other) is very safe, meaning it just does nothing if cd3 is null or already unpaused
            //      (also negligible performance overhead)

            // checking for IsComplete() then running Fulfill() if true is the *same* as just checking FulfillIfComplete
            if (CooldownManager.IsComplete(cd3)) {

                CooldownManager.Fulfill(cd3);
                cd3 = null; // im resetting this because i want to recalculate a new duration when i run it again
                            // this has no negative impact on the manaeger

                audioPlayer.Play(sound3);

            }

        }
        else

            CooldownManager.Pause(cd3); // instead of ending out of range, i just pause the timer, and itll pick back up where i left off

        // pressing f to play a sound, with a cooldown
        CooldownManager.TryStart(cd2); // runs a cooldown if it is not already running, otherwise does nothing
        if (Input.GetKeyDown(KeyCode.F) && CooldownManager.IsComplete(cd2)) {

            // this specific case would actually break if you used FulfillIfComplete()
            //      because FulfillIfComplete() only returns true on the first call where the cooldown was finshed,
            //      so I will wait to fulfill it until after I get the input

            CooldownManager.Fulfill(cd2); // waits until key f is pressed to stop tracking cooldown

            audioPlayer.Play(sound2);

        }
        else if (Input.GetKeyDown(KeyCode.F))
            print(CooldownManager.TimeRemaining(cd2) + " seconds left on the cooldown");

        // YES, PROPER USAGE OF THE COOLDOWN MANAGER MEANS YOU DONT NEED A FLAG FOR EACH COOLDOWN!!
        // instead of handling like 5 variables for each cooldown you want to run, you just serialize ONE Cooldown!!!

    }

}
