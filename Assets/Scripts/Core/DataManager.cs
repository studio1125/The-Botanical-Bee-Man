using System;
using UnityEngine;

public class DataManager : MonoBehaviour {

    [Header("Data")]
    private int nectarCollected;

    [Header("Actions")]
    public Action<int> onNectarCollected;

    private void Awake() => nectarCollected = TempStorage.GetNectarCollected(); // get the nectar collected from TempStorage

    public void AddNectar(int amount) {

        nectarCollected += amount;
        onNectarCollected?.Invoke(nectarCollected); // invoke the onNectarCollected action to notify other scripts that nectar has been collected

    }

    public void RemoveNectar(int amount) {

        nectarCollected -= amount;
        onNectarCollected?.Invoke(nectarCollected); // invoke the onNectarCollected action to notify other scripts that nectar has been removed

    }

    public int GetNectarCollected() => nectarCollected;

}
