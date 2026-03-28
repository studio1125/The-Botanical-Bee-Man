using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowerManager : MonoBehaviour {

    [Header("References")]
    private BuzzModeManager buzzModeManager;
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
        [SerializeField, Tooltip("Try to pick factor of width"), Min(0.1f)] public float gridWidthDelta;
        [SerializeField, Tooltip("Try to pick factor of height"), Min(0.1f)] public float gridHeightDelta;

        [Header("Visual")]
        [SerializeField] public bool randomizeFacingDirection;

    }

    [Header("Spawn Regions (\"Grids\")")]
    [SerializeField] List<ObjectSpawnRegion> grids;

    private void Start() {

        buzzModeManager = FindFirstObjectByType<BuzzModeManager>();
        dataManager = FindFirstObjectByType<DataManager>();

        foreach (ObjectSpawnRegion grid in grids)
            grid.spawned = new List<GameObject>();

        SpawnObjects(); // spawn the flowers at the start of the game

    }

    // spawns all flowers based on grid config and spawn chance
    private void SpawnObjects() {

        Vector2 mapBounds = buzzModeManager.GetMapBounds();

        foreach (ObjectSpawnRegion grid in grids) {

            if (PlayerData.IsUpgradePurchased(UpgradeType.TrashRemoval) && !grid.isFlowerGrid)
                continue; // skip non-flower grids if the player has the trash removal upgrade

            float dx = grid.gridWidthDelta;
            float xo = -mapBounds.x / 2f;
            float xf = mapBounds.x / 2f;

            float dy = grid.gridHeightDelta;
            float yo = -mapBounds.y / 2f;
            float yf = mapBounds.y / 2f;

            if (dx == 0 || dy == 0) {

                Debug.LogError("A grid in the FlowerManager has an invalid configuration! Height and width deltas must be nonzero. Skipped spawning for grid.");
                return;

            }

            for (float x = xo; x < xf; x += dx)
                for (float y = yo; y < yf; y += dy)
                    if (UnityEngine.Random.Range(0f, 1f) < grid.objSpawnChance)
                        SpawnObject(new Vector2(x + UnityEngine.Random.Range(-grid.objPositionOffset, grid.objPositionOffset),
                                                y + UnityEngine.Random.Range(-grid.objPositionOffset, grid.objPositionOffset)),
                                                grid);

        }
    }

    private void OnDisable() {

        // unsubscribe from the pollination events to prevent memory leaks when the object is disabled
        foreach (ObjectSpawnRegion grid in grids)
            if (grid.isFlowerGrid)
                foreach (GameObject obj in grid.spawned)
                    if (obj)
                        obj.GetComponent<Flower>().onPollinated -= (nectar) => AddNectar(nectar);

    }


    // spawns a flower (and subscribes to its pollination event)
    GameObject SpawnObject(Vector2 loc, ObjectSpawnRegion owner) {

        GameObject obj = Instantiate(owner.spawns[UnityEngine.Random.Range(0, owner.spawns.Count)], loc, Quaternion.identity); // spawn a flower at a specific position
        owner.spawned.Add(obj);

        if (owner.randomizeFacingDirection && UnityEngine.Random.Range(0f, 1f) < 0.5f)
            obj.transform.localScale = new Vector3(-obj.transform.localScale.x, obj.transform.localScale.y);

        if (owner.isFlowerGrid)
            obj.GetComponent<Flower>().onPollinated += (nectar) => AddNectar(nectar); // add nectar to the player's total when a flower is pollinated

        return obj;

    }

    private void AddNectar(int nectarAmount) => dataManager.AddNectar(nectarAmount);

}
