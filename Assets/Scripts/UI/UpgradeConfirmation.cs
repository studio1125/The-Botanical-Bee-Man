using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeConfirmation : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text nectarCostText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button cancelButton;
    private UpgradeManager upgradeManager;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private UpgradeData selectedUpgrade;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private float upgradeConfirmationFadeDuration;

    public void Initialize(UpgradeManager upgradeManager) {

        this.upgradeManager = upgradeManager;

        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        purchaseButton.onClick.AddListener(() => {

            if (upgradeManager.PurchaseUpgrade(selectedUpgrade)) // attempt to purchase the selected upgrade; if successful, hide the upgrade confirmation screen and generate new upgrade choices (in case the purchase changed which upgrades are available)
                HideUpgradeConfirmation();

        });
        cancelButton.onClick.AddListener(HideUpgradeConfirmation); // hide the upgrade confirmation screen without purchasing the upgrade

        canvasGroup.alpha = 0f; // make the upgrade confirmation invisible

    }

    public void ShowUpgradeConfirmation(UpgradeData upgradeData) {

        this.selectedUpgrade = upgradeData; // store the selected upgrade data for use when the purchase button is clicked

        purchaseButton.interactable = upgradeManager.CanAffordUpgrade(upgradeData); // deactivate the purchase button if the player cannot afford the upgrade to provide immediate feedback on whether the upgrade can be purchased

        gameObject.SetActive(true); // show the upgrade confirmation screen

        upgradeIcon.sprite = upgradeData.GetIcon();
        nameText.text = upgradeData.GetSiteName();
        descriptionText.text = upgradeData.GetConciseDescription();
        nectarCostText.text = upgradeData.GetNectarCost().ToString();

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any existing fade coroutine to prevent multiple coroutines from running simultaneously and causing unintended behavior
        fadeCoroutine = StartCoroutine(FadeMenu(1f, upgradeConfirmationFadeDuration)); // start the fade coroutine to fade in the upgrade confirmation screen

        RefreshLayout(rectTransform); // force the layout to update to fix any issues with the upgrade confirmation's size or position

    }

    public void HideUpgradeConfirmation() {

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any existing fade coroutine to prevent multiple coroutines from running simultaneously and causing unintended behavior
        fadeCoroutine = StartCoroutine(FadeMenu(0f, upgradeConfirmationFadeDuration)); // start the fade coroutine to fade out the upgrade confirmation screen

    }

    private IEnumerator FadeMenu(float targetOpacity, float fadeDuration) {

        float currentTime = 0f;
        float startOpacity = canvasGroup.alpha;

        canvasGroup.gameObject.SetActive(true); // ensure canvas group is active before fading

        while (currentTime < fadeDuration) {

            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, currentTime / fadeDuration);
            yield return null;

        }

        canvasGroup.alpha = targetOpacity; // ensure final opacity is set

        // if the target opacity is 0, disable the canvas group
        if (targetOpacity == 0f)
            canvasGroup.gameObject.SetActive(false);

        fadeCoroutine = null; // reset the coroutine reference when done

    }

    // IMPORTANT: this only works when the root is visible
    protected void RefreshLayout(RectTransform root) {

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(root); // force a rebuild of the root layout at the end

    }
}
