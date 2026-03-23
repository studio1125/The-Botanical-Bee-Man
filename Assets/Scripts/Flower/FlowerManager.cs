using System.Collections.Generic;
using UnityEngine;

public class FlowerManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Flower[] flowerPrefabs;
    private List<Flower> flowers; // list to hold the instantiated flower objects
    private DataManager dataManager;

    [Header("Spawn Region (\"Grid\")")]
    [SerializeField] private Vector2 gridCenter;
    [SerializeField, Min(0)] private float gridWidth;
    [SerializeField, Min(0)] private float gridHeight;
    [SerializeField, Tooltip("try to pick factor of width"), Min(0.1f)] private float gridWidthDelta;
    [SerializeField, Tooltip("try to pick factor of height"), Min(0.1f)] private float gridHeightDelta;
    [SerializeField, Range(0, 1)] private float flowerSpawnChance;
    [SerializeField, Min(0), Tooltip("Adds a bit of randomness to flower pos")] private float flowerPositionOffset;
    [SerializeField] private bool drawGridGizmo;
    // TODO: Add "Fit map bounds" button

    private void Start() {

        dataManager = FindFirstObjectByType<DataManager>();
        flowers = new List<Flower>();

        // can be removed later, but this will also register all existing flowers in case they were spawned in the editor
        foreach (Flower flower in FindObjectsByType<Flower>(FindObjectsSortMode.None)) {

            flowers.Add(flower);
            flower.onPollinated += (nectar) => AddNectar(nectar); // subscribe to the pollination event for each flower

        }

        SpawnFlowers(); // spawn the flowers at the start of the game

    }

    // spawns all flowers based on grid config and spawn chance
    private void SpawnFlowers() {

        // TODO: implement 
        float dx = gridWidthDelta;
        float xo = (gridCenter.x - gridWidth / 2) + dx / 2;
        float xf = gridCenter.x + gridWidth / 2;

        float dy = gridHeightDelta;
        float yo = (gridCenter.y - gridHeight / 2) + dy / 2;
        float yf = gridCenter.y + gridHeight / 2;

        int maxFlowers = (int)((gridWidth / gridWidthDelta) * (gridHeight / gridHeightDelta));

        for (float x = xo; x < xf; x += dx)
            for (float y = yo; y < yf; y += dy)
                if (Random.Range(0f, 1f) < flowerSpawnChance)
                    SpawnFlower(new Vector2(x + Random.Range(-flowerPositionOffset, flowerPositionOffset),
                                            y + Random.Range(-flowerPositionOffset, flowerPositionOffset)));

    }

    private void OnDisable() {

        // unsubscribe from the pollination events to prevent memory leaks when the object is disabled
        foreach (Flower flower in flowers)
            flower.onPollinated -= (nectar) => AddNectar(nectar);

    }


    // spawns a flower (and subscribes to its pollination event)
    Flower SpawnFlower(Vector2 loc) {

        Flower newFlower = Instantiate(flowerPrefabs[0], loc, Quaternion.identity); // spawn a flower at a specific position
        flowers.Add(newFlower);
        newFlower.onPollinated += (nectar) => AddNectar(nectar); // add nectar to the player's total when a flower is pollinated

        return newFlower;

    }

    private void AddNectar(int nectarAmount) => dataManager.AddNectar(nectarAmount);

    public void MatchGridToMap() {

        BoxCollider2D mapCollider = FindFirstObjectByType<BuzzModeManager>().GetMapBounds();
        Bounds mapBounds = mapCollider.bounds;

        gridWidth = Mathf.Abs(mapBounds.max.x - mapBounds.min.x);
        gridHeight = Mathf.Abs(mapBounds.max.y - mapBounds.min.y);

        gridCenter = new Vector2(mapCollider.transform.position.x + mapBounds.center.x,
                                mapCollider.transform.position.y + mapBounds.center.y);

    }

    private void OnDrawGizmos() {

        Gizmos.color = Color.mediumSpringGreen;
        Gizmos.DrawWireCube(gridCenter, new Vector2(gridWidth, gridHeight));

    }

    private void OnDrawGizmosSelected() {

        if (drawGridGizmo) {

            // taken from spawn logic

            float dx = gridWidthDelta;
            float xo = (gridCenter.x - gridWidth / 2) + dx / 2;
            float xf = gridCenter.x + gridWidth / 2;

            float dy = gridHeightDelta;
            float yo = (gridCenter.y - gridHeight / 2) + dy / 2;
            float yf = gridCenter.y + gridHeight / 2;

            for (float x = xo; x < xf; x += dx)
                for (float y = yo; y < yf; y += dy) {

                    /*if (Random.Range(0f, 1f) < flowerSpawnChance) {

                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(new Vector2(x, y), new Vector2(dx, dy));

                    }*/

                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(new Vector2(x, y), 0.2f);

                }

        }

    }


}


