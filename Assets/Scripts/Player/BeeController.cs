using System;
using UnityEngine;
using UnityEngine.UIElements;
using SFX;

public class BeeController : MonoBehaviour {

    [Header("References")]
    private Rigidbody2D rb;
    private BuzzUIManager ui;
    private AudioPlayer audioPlayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private float currMoveSpeed;
    [SerializeField] private float rotationSpeed; // degrees per second
    private float currRotationSpeed;
    [SerializeField] private float stopThreshold; // minimum distance to mouse position before stopping
    private bool isHoldingMouseButton;

    private enum LookMode {

        Point,
        Flip,
        None

    }

    [Header("Visual")]
    [SerializeField] private GameObject sprite;
    [SerializeField] private LookMode lookMode;

    [Header("Taking Damage")]
    [SerializeField] private float damageKb;
    [SerializeField, Tooltip("% that move speed will be set to after being damaged"), Range(0, 1)] private float damageMoveSpeedPenalty;
    [SerializeField, Tooltip("% that rotation speed will be set to after being damaged"), Range(0, 2)] private float damageRotSpeedPenalty;
    [SerializeField, Tooltip("How long to keep reduced move speed")] private Cooldown damageSpeedPenaltyDuration;

    [Header("Super Bumble ")]
    [SerializeField, Tooltip("Charge gained on pollinate flower, max 100")] private float superChargeOnPollinate;
    [SerializeField, Tooltip("Charge decay per second")] private float superChargeDecay;
    [SerializeField] private Cooldown superDuration;
    private float superCharge;

    [Header("Sounds")]
    [SerializeField] private Sound onHurtSound;
    [SerializeField] private Sound onFinishPollinateSound;
    [SerializeField] private Sound onSuperSound;

    private void Start() {

        rb = GetComponent<Rigidbody2D>();
        ui = FindAnyObjectByType<BuzzUIManager>();
        audioPlayer = GetComponent<AudioPlayer>();

        FindAnyObjectByType<DataManager>().onNectarCollected += amount => OnPollinateFlower();

        currMoveSpeed = moveSpeed;
        currRotationSpeed = rotationSpeed;

    }

    private void Update() {
        isHoldingMouseButton = Input.GetMouseButton(0);

        // constantly decay charge when not full (full charge means super is active)
        if (superCharge != 100)
            superCharge = Mathf.Max(0, superCharge - (superChargeDecay * Time.deltaTime));

        // reset super when the time is up 
        if (CooldownManager.FulfillIfComplete(superDuration))
            superCharge = 0;

        // reflect changes to super charge in the ui 
        ui.UpdateSuperSlider(superCharge);

        UpdateSprite();

    }

    private void FixedUpdate() {

        if (!isHoldingMouseButton) {

            rb.angularVelocity = 0;
            return; // only move towards the mouse position when the left mouse button is held down

        }

        // if the damage penalty cooldown started and its done, finish the cooldown and set speeds to default values
        if (CooldownManager.HasCooldown(damageSpeedPenaltyDuration) && CooldownManager.FulfillIfComplete(damageSpeedPenaltyDuration)) {

            currMoveSpeed = moveSpeed;
            currRotationSpeed = rotationSpeed;

        }

        // the bee will always turn to the mouse position fully when the left mouse button clicked somewhere (like it will always face the direction of the last click), but it will only move towards the mouse position when the left mouse button is held down, and will stop when it is released
        // add force is used so the bee drifts when left mouse button is released and slows down gradually instead of stopping instantly, which feels more natural for a flying character

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toMouse = mousePos - rb.position;
        float distance = toMouse.magnitude;

        if (distance < stopThreshold) return;

        Vector2 direction = toMouse.normalized; // the direction from the bee to the mouse position, normalized to have a magnitude of 1

        // rotate towards the mouse position at the rotation speed
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 degrees to align with the sprite's forward direction
        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, currRotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(angle);

        rb.AddForce(transform.up * currMoveSpeed);

        // clamp the velocity to the move speed
        if (rb.linearVelocity.magnitude > currMoveSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * currMoveSpeed;

    }

    private void UpdateSprite() {

        sprite.transform.position = transform.position;

        switch (lookMode) {

            case LookMode.Point:
                sprite.transform.rotation = transform.rotation;
                break;

            case LookMode.Flip:

                sprite.transform.rotation = Quaternion.Euler(Vector3.zero);

                if (transform.rotation.eulerAngles.z > 0)
                    sprite.transform.localScale = new Vector3(sprite.transform.localScale.x, -sprite.transform.localScale.y);
                else
                    sprite.transform.localScale = new Vector3(sprite.transform.localScale.x, sprite.transform.localScale.y);
                break;

            default:
                break;

        }



    }

    public void OnHurt(Vector2 kbDir) {

        audioPlayer.Play(onHurtSound);
        audioPlayer.Stop(onSuperSound);

        // apply kb to player
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(kbDir.normalized * damageKb, ForceMode2D.Impulse);

        superCharge = 0;

        // apply penalties to speed
        currMoveSpeed = moveSpeed * damageMoveSpeedPenalty;
        currRotationSpeed = rotationSpeed * damageRotSpeedPenalty;
        // restart if already running
        CooldownManager.ForceStart(damageSpeedPenaltyDuration);

    }
    public void OnPollinateFlower() {

        // update super charge
        superCharge = Mathf.Min(100, superCharge + superChargeOnPollinate);

        // play sound for super if super hasnt been initiated and charge is maxed
        if (!CooldownManager.HasCooldown(superDuration) && superCharge == 100)
            audioPlayer.Play(onSuperSound);

        // initiate timer to reset super charge (once superDuration elapses, charge resets in Update())
        if (superCharge == 100)
            CooldownManager.TryStart(superDuration);

        audioPlayer.Play(onFinishPollinateSound);

    }

    public bool HasSuper() => superCharge >= 100;

    private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        if (rb)
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + (0.3f * rb.linearVelocity));

    }
}
