public class ResearchModeManager : BaseModeManager {

    public void ReturnToBuzzMode() {

        PlayerData.AdvanceDay();
        // TODO: change the nectar value to account for the purchased upgrades in research mode
        loadingManager.LoadScene(Constants.BUZZ_MODE_SCENE_NAME, funFactDatabase.GetRandomFunFact(), TempStorage.GetNectarCollected());

    }
}
