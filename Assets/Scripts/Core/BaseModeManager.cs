using UnityEngine;

public class BaseModeManager : MonoBehaviour {

    [Header("References")]
    protected DataManager dataManager;
    protected FunFactDatabase funFactDatabase;
    protected LoadingManager loadingManager;

    protected void Start() {

        dataManager = FindFirstObjectByType<DataManager>();
        funFactDatabase = FindFirstObjectByType<FunFactDatabase>();
        loadingManager = FindFirstObjectByType<LoadingManager>();
        loadingManager.HideLoadingScreen(); // hide the loading screen at the start of the game

    }
}
