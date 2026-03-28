using UnityEngine;

public class CameraController : MonoBehaviour {

    [Header("References")]
    private BuzzModeManager gameManager;
    private new Camera camera;

    [Header("Follow")]
    [SerializeField] private float followSmoothing;
    private Transform target;
    private float zOffset;

    [Header("Bounds")]
    private float xMin, yMin, xMax, yMax;
    private float camSize;
    private float camRatio;

    private void Start() {

        target = FindFirstObjectByType<BeeController>().transform;
        gameManager = FindFirstObjectByType<BuzzModeManager>();
        camera = GetComponent<Camera>();

        zOffset = transform.position.z - target.position.z;

        Vector2 mapBounds = gameManager.GetMapBounds();
        xMin = mapBounds.x / -2f;
        yMin = mapBounds.y / -2f;
        xMax = mapBounds.x / 2f;
        yMax = mapBounds.y / 2f;

        camSize = camera.orthographicSize;
        camRatio = ((float) Screen.width / Screen.height) * camSize;

    }

    private void LateUpdate() => transform.position = Vector3.Lerp(transform.position, new Vector3(Mathf.Clamp(target.position.x, xMin + camRatio, xMax - camRatio), Mathf.Clamp(target.position.y, yMin + camSize, yMax - camSize), zOffset), followSmoothing * Time.deltaTime); // set the camera's position to a lerp between its current position and the target's position, clamped within the bounds of the map, with smoothing applied

}
