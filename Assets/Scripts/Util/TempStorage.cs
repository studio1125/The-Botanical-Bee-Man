public static class TempStorage {

    private static string currentFunFact = ""; // the current fun fact being displayed on the loading screen, used to keep both scenes' loading screens in sync when transitioning between scenes (with the same fun fact being displayed on both scenes' loading screens)
    private static int nectarCollected = 0;
    private static int currentUpgradeStage = 0; // the current upgrade stage, used to determine which upgrades are available to show in the upgrade menu

    public static void SetCurrentFunFact(string funFact) => currentFunFact = funFact;

    public static string GetCurrentFunFact() => currentFunFact;

    public static void SetNectarCollected(int amount) => nectarCollected = amount;

    public static int GetNectarCollected() => nectarCollected;

    public static void SetCurrentUpgradeStage(int stage) => currentUpgradeStage = stage;

    public static int GetCurrentUpgradeStage() => currentUpgradeStage;

}
