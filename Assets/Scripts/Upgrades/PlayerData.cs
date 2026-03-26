using System.Collections.Generic;

public class PlayerData {

    public static List<UpgradeData> purchasedUpgrades = new List<UpgradeData>();

    public static void AddUpgrade(UpgradeData upgrade) => purchasedUpgrades.Add(upgrade);

    public static bool IsUpgradePurchased(UpgradeType upgradeType) => purchasedUpgrades.Exists(upgrade => upgrade.GetUpgradeType() == upgradeType);

}
