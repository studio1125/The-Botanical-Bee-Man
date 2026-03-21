using UnityEngine;

public class BaseModeManager : MonoBehaviour {

    [Header("References")]
    protected LoadingManager loadingManager;

    protected void Start() {

        loadingManager = FindFirstObjectByType<LoadingManager>();
        loadingManager.HideLoadingScreen(); // hide the loading screen at the start of the game

    }
}
