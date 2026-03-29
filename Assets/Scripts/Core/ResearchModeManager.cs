public class ResearchModeManager : BaseModeManager {

    public void ReturnToBuzzMode() {

        PlayerData.AdvanceDay();
        loadingManager.LoadScene(Constants.BUZZ_MODE_SCENE_NAME, funFactDatabase.GetRandomFunFact(), dataManager.GetNectarCollected());

    }
}
