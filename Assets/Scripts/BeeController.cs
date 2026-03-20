using UnityEngine;

public class BeeController : MonoBehaviour {

    [Header("References")]
    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed; // degrees per second
    [SerializeField] private float stopThreshold; // minimum distance to mouse position before stopping
    private bool isHoldingMouseButton;

    private void Start() => rb = GetComponent<Rigidbody2D>();

    private void Update() => isHoldingMouseButton = Input.GetMouseButton(0);

    private void FixedUpdate() {

        if (!isHoldingMouseButton) return; // only move towards the mouse position when the left mouse button is held down

        // the bee will always turn to the mouse position fully when the left mouse button clicked somewhere (like it will always face the direction of the last click), but it will only move towards the mouse position when the left mouse button is held down, and will stop when it is released
        // add force is used so the bee drifts when left mouse button is released and slows down gradually instead of stopping instantly, which feels more natural for a flying character

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toMouse = mousePos - rb.position;
        float distance = toMouse.magnitude;

        if (distance < stopThreshold) return;

        Vector2 direction = toMouse.normalized; // the direction from the bee to the mouse position, normalized to have a magnitude of 1

        // rotate towards the mouse position at the rotation speed
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // subtract 90 degrees to align with the sprite's forward direction
        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(angle);

        rb.AddForce(transform.up * moveSpeed);

        // clamp the velocity to the move speed
        if (rb.linearVelocity.magnitude > moveSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;

    }
}
