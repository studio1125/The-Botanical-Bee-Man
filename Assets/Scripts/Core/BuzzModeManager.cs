using UnityEngine;

public class BuzzModeManager : BaseModeManager {

    [Header("References")]
    [SerializeField] private Vector2 mapBounds;
    [SerializeField] private GameObject waterFountain;

    [Header("Settings")]
    [SerializeField, Tooltip("Amount of seconds in a day")] private float dayDuration;
    [SerializeField, Tooltip("Multiplier for the size of the garden grid (used for the garden expansion upgrade)")] private float gardenSizeMultiplier;
    private float timeRemaining;
    private bool isDayOver;

    [Header("Bounds")]
    [SerializeField] private Transform topBound;
    [SerializeField] private Transform leftBound;
    [SerializeField] private Transform bottomBound;
    [SerializeField] private Transform rightBound;

    private new void Start() {

        base.Start();

        // apply the garden expansion multiplier to the map bounds if the upgrade is purchased
        if (PlayerData.IsUpgradePurchased(UpgradeType.GardenExpansion))
            mapBounds *= gardenSizeMultiplier;

        waterFountain.SetActive(PlayerData.IsUpgradePurchased(UpgradeType.ExtendedSuper)); // make the water fountain visible if the player has the extended super upgrade

        // now that bounds are fully adjusted, position and scale the boundary objects based on the map bounds (the boundary objects are scaled to be as long as the map bounds in their respective directions, and positioned just outside the map bounds so they effectively act as invisible walls around the map)

        topBound.position = new Vector2(0f, (mapBounds.y / 2f) + (topBound.localScale.y / 2f));
        topBound.localScale = new Vector2(mapBounds.x, topBound.localScale.y);

        leftBound.position = new Vector2(-mapBounds.x / 2f - (leftBound.localScale.x / 2f), 0f);
        leftBound.localScale = new Vector2(leftBound.localScale.x, mapBounds.y);

        bottomBound.position = new Vector2(0f, -mapBounds.y / 2f - (bottomBound.localScale.y / 2f));
        bottomBound.localScale = new Vector2(mapBounds.x, bottomBound.localScale.y);

        rightBound.position = new Vector2(mapBounds.x / 2f + (rightBound.localScale.x / 2f), 0f);
        rightBound.localScale = new Vector2(rightBound.localScale.x, mapBounds.y);

        timeRemaining = dayDuration;

    }

    private void Update() {

        if (isDayOver) return; // if the day is already over, skip the update

        // if the time remaining is less than or equal to 0, start a new day
        if (timeRemaining <= 0f) {

            isDayOver = true; // set the isDayOver flag to true to indicate that the day has ended
            timeRemaining = 0f; // reset the time remaining to ensure it doesn't go negative

            if (PlayerData.IsUpgradePurchased(UpgradeType.PesticideResistance))
                loadingManager.LoadScene(Constants.VICTORY_SCENE_NAME, funFactDatabase.GetRandomFunFact(), dataManager.GetNectarCollected()); // load the victory scene if the player has the pesticide resistance upgrade (which is the final upgrade and thus indicates that the player has won the game)
            else
                loadingManager.LoadScene(Constants.RESEARCH_MODE_SCENE_NAME, funFactDatabase.GetRandomFunFact(), dataManager.GetNectarCollected()); // load the research mode scene at the end of the day

            // don't update the day text here; it is only updated when the player returns to buzz mode after the research mode scene is loaded (so in the Start method)

        } else {

            timeRemaining -= Time.deltaTime; // decrease the time remaining by the time that has passed since the last frame

        }
    }

    private void OnDrawGizmos() {

        // draw the map bounds as a wire cube in the scene view for visualization (centered at the origin with the specified map bounds as its size)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector2.zero, new Vector2(mapBounds.x, mapBounds.y));

    }

    public Vector2 GetMapBounds() => mapBounds;

    public float GetTimeRemaining() => timeRemaining;

}
