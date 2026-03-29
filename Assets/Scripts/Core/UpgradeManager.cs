using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TMP_Text searchText;
    [SerializeField] private TMP_Text resultCountText;
    [SerializeField] private UpgradeButton upgradeButtonPrefab;
    [SerializeField] private UpgradeData[] upgrades;
    [SerializeField] private RectTransform upgradeContent; // the parent transform for the upgrade buttons (e.g. a vertical layout group)
    [SerializeField] private Button nextButton;
    private ResearchModeManager researchModeManager;

    [Header("Settings")]
    [SerializeField] private int maxResultsToShow; // the maximum number of upgrade results to show at once (shown upgrades will be less than this if there are not enough unbought upgrades to reach the max)
    private int currentStage; // the current stage of the game, used to determine which upgrades are available to show

    [Header("Data")]
    private List<UpgradeData>[] availableUpgradesByStage;

    private void Start() {

        researchModeManager = FindFirstObjectByType<ResearchModeManager>();

        nextButton.onClick.AddListener(OnNextButtonClicked);

        // find the highest stage among the upgrades to determine how many stages we need to organize upgrades into
        int maxStage = 0;

        foreach (UpgradeData upgrade in upgrades) {

            int stage = upgrade.GetStage();

            if (stage > maxStage)
                maxStage = stage;

        }

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

        // TODO: return false if player cannot afford the upgrade (not enough nectar)

        availableUpgradesByStage[upgrade.GetStage()].Remove(upgrade); // remove the upgrade from the available upgrades list for its stage
        PlayerData.AddUpgrade(upgrade);
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
