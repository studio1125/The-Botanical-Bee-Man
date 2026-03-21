using System.Collections.Generic;
using UnityEngine;

public class FlowerManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Flower[] flowerPrefabs;
    private List<Flower> flowers; // list to hold the instantiated flower objects
    private DataManager dataManager;

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

    private void SpawnFlowers() {

        // TODO: implement (below is an example of how to spawn a flower and subscribe to its pollination event)
        Flower newFlower = Instantiate(flowerPrefabs[0], new Vector3(-2f, 0f, 0f), Quaternion.identity); // spawn a flower at a specific position
        flowers.Add(newFlower);
        newFlower.onPollinated += (nectar) => AddNectar(nectar); // add nectar to the player's total when a flower is pollinated

    }

    private void OnDisable() {

        // unsubscribe from the pollination events to prevent memory leaks when the object is disabled
        foreach (Flower flower in flowers)
            flower.onPollinated -= (nectar) => AddNectar(nectar);

    }

    private void AddNectar(int nectarAmount) => dataManager.AddNectar(nectarAmount);

}
