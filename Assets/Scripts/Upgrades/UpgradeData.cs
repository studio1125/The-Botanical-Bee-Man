using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade")]
public class UpgradeData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private UpgradeType type;
    [SerializeField] private Sprite icon;
    [SerializeField] private string siteName; // e.g. "Speedtest by Ookla"
    [SerializeField] private string url; // e.g. "https://www.speedtest.net/"
    [SerializeField] private string siteTitle; // e.g. "Speedtest by Ookla - The Global Broadband Speed Test"
    [SerializeField] private string description; // e.g. "Speedtest is better with the app. Download the Speedtest app for more metrics, video testing, mobile coverage maps, and more..."
    [SerializeField] private string conciseDescription; // this is what will be shown on the upgrade info widget; it should be a shorter version of the description that gives the player information on EXACTLY what the upgrade does in the context of the game
    [SerializeField, Min(0)] private int stage; // the stage at which this upgrade becomes available; this is used to keep a balanced order of upgrades (stage zero is the first stage)

    public UpgradeType GetUpgradeType() => type;

    public Sprite GetIcon() => icon;

    public string GetSiteName() => siteName;

    public string GetUrl() => url;

    public string GetSiteTitle() => siteTitle;

    public string GetDescription() => description;

    public string GetConciseDescription() => conciseDescription;

    public int GetStage() => stage;

}

public enum UpgradeType {

    PesticideResistance,
    TrashRemoval

}
