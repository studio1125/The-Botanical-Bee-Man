using UnityEngine;

public class DataManager : MonoBehaviour {

    [Header("Data")]
    private int nectarCollected;

    public void AddNectar(int amount) => nectarCollected += amount;

    public int GetNectarCollected() => nectarCollected;

}
