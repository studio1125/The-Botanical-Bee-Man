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
    private BoxCollider2D mapBounds;
    private float xMin, yMin, xMax, yMax;
    private float camSize;
    private float camRatio;

    private void Start() {

        target = FindFirstObjectByType<BeeController>().transform;
        gameManager = FindFirstObjectByType<BuzzModeManager>();
        camera = GetComponent<Camera>();

        mapBounds = gameManager.GetMapBounds();

        zOffset = transform.position.z - target.position.z;

        xMin = mapBounds.bounds.min.x;
        yMin = mapBounds.bounds.min.y;
        xMax = mapBounds.bounds.max.x;
        yMax = mapBounds.bounds.max.y;

        camSize = camera.orthographicSize;
        camRatio = ((float) Screen.width / Screen.height) * camSize;

    }

    private void LateUpdate() => transform.position = Vector3.Lerp(transform.position, new Vector3(Mathf.Clamp(target.position.x, xMin + camRatio, xMax - camRatio), Mathf.Clamp(target.position.y, yMin + camSize, yMax - camSize), zOffset), followSmoothing * Time.deltaTime); // set the camera's position to a lerp between its current position and the target's position, clamped within the bounds of the map, with smoothing applied

}
