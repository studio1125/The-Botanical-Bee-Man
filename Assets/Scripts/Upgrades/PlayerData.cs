using System.Collections.Generic;

public class PlayerData {

    private static readonly List<UpgradeData> purchasedUpgrades = new List<UpgradeData>();
    private static int currentDay = 1;

    public static void AddUpgrade(UpgradeData upgrade) => purchasedUpgrades.Add(upgrade);

    public static bool IsUpgradePurchased(UpgradeType upgradeType) => purchasedUpgrades.Exists(upgrade => upgrade.GetUpgradeType() == upgradeType);

    public static void AdvanceDay() => currentDay++;

    public static int GetCurrentDay() => currentDay;

}
