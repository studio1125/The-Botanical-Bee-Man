using System;
using UnityEngine;

public class DataManager : MonoBehaviour {

    [Header("Data")]
    private int nectarCollected;

    [Header("Actions")]
    public Action<int> onNectarCollected;

    public void AddNectar(int amount) {

        nectarCollected += amount;
        onNectarCollected?.Invoke(nectarCollected); // invoke the onNectarCollected action to notify other scripts that nectar has been collected

    }

    public int GetNectarCollected() => nectarCollected;

}
