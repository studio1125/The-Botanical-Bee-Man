using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SFX;

public class UpgradeManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TMP_Text searchText;
    [SerializeField] private TMP_Text resultCountText;
    [SerializeField] private UpgradeButton upgradeButtonPrefab;
    [SerializeField] private UpgradeData[] upgrades;
    [SerializeField] private RectTransform upgradeContent; // the parent transform for the upgrade buttons (e.g. a vertical layout group)
    [SerializeField] private Button nextButton;
    private ResearchModeManager researchModeManager;
    private DataManager dataManager;
    private AudioPlayer audioPlayer;

    [Header("Settings")]
    [SerializeField] private string[] searchBarTexts;
    [SerializeField] private int maxResultsToShow; // the maximum number of upgrade results to show at once (shown upgrades will be less than this if there are not enough unbought upgrades to reach the max)
    private int currentStage; // the current stage of the game, used to determine which upgrades are available to show

    [Header("Data")]
    private List<UpgradeData>[] availableUpgradesByStage;

    [Header("Sounds")]
    [SerializeField] private Sound purchaseSound;
    [SerializeField] private Sound purchaseFailSound;

    private void Start() {

        researchModeManager = FindFirstObjectByType<ResearchModeManager>();
        dataManager = FindFirstObjectByType<DataManager>();
        audioPlayer = GetComponent<AudioPlayer>();

        nextButton.onClick.AddListener(OnNextButtonClicked);

        // find the highest stage among the upgrades to determine how many stages we need to organize upgrades into
        int maxStage = 0;

        foreach (UpgradeData upgrade in upgrades) {

            int stage = upgrade.GetStage();

            if (stage > maxStage)
                maxStage = stage;

        }

        searchText.text = searchBarTexts[Random.Range(0, searchBarTexts.Length)]; // set the search bar text to a random string from the list of search bar texts

        availableUpgradesByStage = new List<UpgradeData>[maxStage + 1];

        // initialize the lists for each stage
        for (int i = 0; i <= maxStage; i++)
            availableUpgradesByStage[i] = new List<UpgradeData>();

        // organize the upgrades into the lists based on their stage
        foreach (UpgradeData upgrade in upgrades)
            availableUpgradesByStage[upgrade.GetStage()].Add(upgrade);

        // output an error if there is a stage with no upgrades, as this will cause issues when trying to generate choices for that stage
        for (int i = 0; i <= maxStage; i++)
            if (availableUpgradesByStage[i].Count == 0)
                Debug.LogError($"No upgrades found for stage {i}. This will cause issues when trying to generate upgrade choices for this stage.");

        GenerateUpgradeChoices();

    }

    private void GenerateUpgradeChoices() {

        // we are guaranteed to have at least one upgrade for the current stage because of the error checking in Start()
        // all bought upgrades are removed from the available upgrades list, so we can just take the first maxResultsToShow upgrades from the list for the current stage to show as choices
        List<UpgradeData> availableUpgrades = availableUpgradesByStage[currentStage];
        int resultsToShow = Mathf.Min(maxResultsToShow, availableUpgrades.Count);

        // clear out any existing upgrade buttons before generating new ones
        foreach (Transform child in upgradeContent)
            Destroy(child.gameObject);

        // shuffle the available upgrades for the current stage to add some randomness to the choices shown
        for (int i = 0; i < availableUpgrades.Count; i++) {

            UpgradeData temp = availableUpgrades[i];
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;

        }

        // instantiate buttons for the first resultsToShow upgrades in the shuffled list of available upgrades for the current stage
        for (int i = 0; i < resultsToShow; i++) {

            UpgradeData upgrade = availableUpgrades[i];
            UpgradeButton button = Instantiate(upgradeButtonPrefab, upgradeContent);
            button.Initialize(this, upgrade, transform.parent);

        }

        resultCountText.text = $"Showing {Mathf.Min(maxResultsToShow, availableUpgrades.Count)} of {availableUpgrades.Count} available upgrades";
        RefreshLayout(upgradeContent); // refresh the layout to ensure the new buttons are properly arranged

    }

    public bool PurchaseUpgrade(UpgradeData upgrade) {

        // check if the upgrade has already been purchased; if so, return false to indicate the purchase was unsuccessful
        if (PlayerData.IsUpgradePurchased(upgrade.GetUpgradeType())) return false;

        // check if the player has enough nectar to purchase the upgrade; if not, return false to indicate the purchase was unsuccessful
        if (dataManager.GetNectarCollected() < upgrade.GetNectarCost()) {

            audioPlayer.Play(purchaseFailSound, target: Camera.main.gameObject);
            return false;

        }

        availableUpgradesByStage[upgrade.GetStage()].Remove(upgrade); // remove the upgrade from the available upgrades list for its stage
        dataManager.RemoveNectar(upgrade.GetNectarCost()); // remove the nectar cost of the upgrade from the player's total nectar
        PlayerData.AddUpgrade(upgrade);
        audioPlayer.Play(purchaseSound, target: Camera.main.gameObject);
        return true;

    }

    private void OnNextButtonClicked() {

        // this does not necessarily move to the next stage; it just goes back to the buzz mode and updates the current stage if there are no more available upgrades for the current stage (if there are still available upgrades for the current stage, we stay in the same stage and just generate new choices)
        if (availableUpgradesByStage[currentStage].Count == 0) // if there are no more available upgrades for the current stage, move to the next stage
            currentStage++;

        researchModeManager.ReturnToBuzzMode();

    }

    // IMPORTANT: this only works when the root is visible
    private void RefreshLayout(RectTransform root) {

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(root); // force a rebuild of the root layout at the end

    }
}
