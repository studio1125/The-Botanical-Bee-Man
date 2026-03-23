using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowerManager : MonoBehaviour {

    [Header("References")]

    private DataManager dataManager;

    [Serializable]
    private class ObjectSpawnRegion {

        [Header("Spawning")]
        [SerializeField, Tooltip("Equal chance of spawning any listed object")] public List<GameObject> spawns;
        [SerializeField] public bool isFlowerGrid;
        [SerializeField, Range(0, 1)] public float objSpawnChance;
        [SerializeField, Min(0), Tooltip("Adds a bit of randomness to flower pos")] public float objPositionOffset;
        [HideInInspector] public List<GameObject> spawned; // list to hold the instantiated flower objects

        [Header("Region Config")]
        [SerializeField] public Vector2 gridCenter;
        [SerializeField, Min(0)] public float gridWidth;
        [SerializeField, Min(0)] public float gridHeight;
        [SerializeField, Tooltip("try to pick factor of width"), Min(0.1f)] public float gridWidthDelta;
        [SerializeField, Tooltip("try to pick factor of height"), Min(0.1f)] public float gridHeightDelta;

        [Header("Debug")]
        [SerializeField] public Color displayColor = Color.green;


    }

    [Header("Spawn Regions (\"Grids\")")]
    [SerializeField] List<ObjectSpawnRegion> grids;
    [SerializeField, Min(0), Tooltip("For editor")] int displayedGridIdx;

    private void Start() {

        dataManager = FindFirstObjectByType<DataManager>();

        foreach (ObjectSpawnRegion grid in grids)
            grid.spawned = new List<GameObject>();

        SpawnObjects(); // spawn the flowers at the start of the game

    }

    // spawns all flowers based on grid config and spawn chance
    private void SpawnObjects() {

        foreach (ObjectSpawnRegion grid in grids) {

            float dx = grid.gridWidthDelta;
            float xo = (grid.gridCenter.x - grid.gridWidth / 2) + dx / 2;
            float xf = grid.gridCenter.x + grid.gridWidth / 2;

            float dy = grid.gridHeightDelta;
            float yo = (grid.gridCenter.y - grid.gridHeight / 2) + dy / 2;
            float yf = grid.gridCenter.y + grid.gridHeight / 2;

            if (dx == 0 || dy == 0) {

                Debug.LogError("A grid in the FlowerManager has an invalid configuration! Height and width deltas must be nonzero. Skipped spawning for grid.");
                return;

            }

            for (float x = xo; x < xf; x += dx)
                for (float y = yo; y < yf; y += dy)
                    if (UnityEngine.Random.Range(0f, 1f) < grid.objSpawnChance)
                        SpawnObject(new Vector2(x + UnityEngine.Random.Range(-grid.objPositionOffset, grid.objPositionOffset),
                                                y + UnityEngine.Random.Range(-grid.objPositionOffset, grid.objPositionOffset)), grid);



        }
    }

    private void OnDisable() {

        // unsubscribe from the pollination events to prevent memory leaks when the object is disabled
        foreach (ObjectSpawnRegion grid in grids)
            if (grid.isFlowerGrid)
                foreach (GameObject obj in grid.spawned)
                    (obj.GetComponent<Flower>()).onPollinated -= (nectar) => AddNectar(nectar);

    }


    // spawns a flower (and subscribes to its pollination event)
    GameObject SpawnObject(Vector2 loc, ObjectSpawnRegion owner) {

        GameObject obj = Instantiate(owner.spawns[UnityEngine.Random.Range(0, owner.spawns.Count)], loc, Quaternion.identity); // spawn a flower at a specific position
        owner.spawned.Add(obj);

        if (owner.isFlowerGrid)
            obj.GetComponent<Flower>().onPollinated += (nectar) => AddNectar(nectar); // add nectar to the player's total when a flower is pollinated

        return obj;

    }

    private void AddNectar(int nectarAmount) => dataManager.AddNectar(nectarAmount);

    public void MatchGridToMap() {

        if (displayedGridIdx > grids.Count) {

            Debug.LogError("Invalid displayed grid idx!");
            return;

        }

        ObjectSpawnRegion grid = grids[displayedGridIdx];

        BoxCollider2D mapCollider = FindFirstObjectByType<BuzzModeManager>().GetMapBounds();
        Bounds mapBounds = mapCollider.bounds;

        grid.gridWidth = Mathf.Abs(mapBounds.max.x - mapBounds.min.x);
        grid.gridHeight = Mathf.Abs(mapBounds.max.y - mapBounds.min.y);

        grid.gridCenter = new Vector2(mapCollider.transform.position.x + mapBounds.center.x,
                                mapCollider.transform.position.y + mapBounds.center.y);

    }

    public void CycleDisplayedGrid() {

        displayedGridIdx++;

        if (displayedGridIdx >= grids.Count)
            displayedGridIdx = 0;

    }

    private void OnDrawGizmos() {

        if (displayedGridIdx > grids.Count)
            return;

        ObjectSpawnRegion grid = grids[displayedGridIdx];

        Gizmos.color = grid.displayColor;
        Gizmos.DrawWireCube(grid.gridCenter, new Vector2(grid.gridWidth, grid.gridHeight));

    }

    private void OnDrawGizmosSelected() {

        if (displayedGridIdx > grids.Count) {

            Debug.LogError("Invalid displayed grid idx!");
            return;

        }

        ObjectSpawnRegion grid = grids[displayedGridIdx];

        Gizmos.color = grid.displayColor;

        // taken from spawn logic

        float dx = grid.gridWidthDelta;
        float xo = (grid.gridCenter.x - grid.gridWidth / 2) + dx / 2;
        float xf = grid.gridCenter.x + grid.gridWidth / 2;

        float dy = grid.gridHeightDelta;
        float yo = (grid.gridCenter.y - grid.gridHeight / 2) + dy / 2;
        float yf = grid.gridCenter.y + grid.gridHeight / 2;

        if (dx == 0 || dy == 0) return;

        for (float x = xo; x < xf; x += dx)
            for (float y = yo; y < yf; y += dy)
                Gizmos.DrawSphere(new Vector2(x, y), 0.2f);

    }


}


