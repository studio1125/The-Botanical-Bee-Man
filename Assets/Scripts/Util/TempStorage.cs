public static class TempStorage {

    private static string currentFunFact = ""; // the current fun fact being displayed on the loading screen, used to keep both scenes' loading screens in sync when transitioning between scenes (with the same fun fact being displayed on both scenes' loading screens)
    private static int nectarCollected = 0;

    public static void SetCurrentFunFact(string funFact) => currentFunFact = funFact;

    public static string GetCurrentFunFact() => currentFunFact;

    public static void SetNectarCollected(int amount) => nectarCollected = amount;

    public static int GetNectarCollected() => nectarCollected;

}
