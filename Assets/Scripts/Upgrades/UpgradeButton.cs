using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [Header("References")]
    private UpgradeManager upgradeManager;
    private UpgradeData upgradeData;
    private Button button;
    private Coroutine fadeCoroutine;

    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text siteNameText; // e.g. "Speedtest by Ookla"
    [SerializeField] private TMP_Text urlText; // e.g. "https://www.speedtest.net/"
    [SerializeField] private TMP_Text siteTitleText; // e.g. "Speedtest by Ookla - The Global Broadband Speed Test"
    [SerializeField] private TMP_Text descriptionText; // e.g. "Speedtest is better with the app. Download the Speedtest app for more metrics, video testing, mobile coverage maps, and more..."

    [Header("Settings")]
    [SerializeField] private Color purchasedColor; // the color to fade the site title text to when the upgrade is purchased
    [SerializeField] private float purchasedFadeDuration; // the duration of the color fade when the upgrade is purchased

    [Header("Info Widget")]
    [SerializeField] private UpgradeInfoWidget infoWidgetPrefab;
    private UpgradeInfoWidget currInfoWidget;
    private Transform rootTransform; // this is the transform of the root object that the item info widget will be parented to; this is used to ensure that the level info widget is always in the same space as the root object and not in world space

    public void Initialize(UpgradeManager upgradeManager, UpgradeData upgradeData, Transform rootTransform) {

        this.upgradeManager = upgradeManager;
        this.upgradeData = upgradeData;
        this.rootTransform = rootTransform;

        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);

        icon.sprite = upgradeData.GetIcon();
        siteNameText.text = upgradeData.GetSiteName();
        urlText.text = upgradeData.GetUrl();
        siteTitleText.text = upgradeData.GetSiteTitle();
        descriptionText.text = upgradeData.GetDescription();

    }

    public void OnPointerEnter(PointerEventData eventData) {

        // destroy the info widget if it exists
        if (currInfoWidget != null) Destroy(currInfoWidget.gameObject);

        currInfoWidget = Instantiate(infoWidgetPrefab, Input.mousePosition, Quaternion.identity, rootTransform); // instantiate the info widget at the mouse position and set its parent to the root transform
        currInfoWidget.transform.SetAsLastSibling(); // set the widget to the front
        currInfoWidget.Initialize(upgradeData); // initialize the widget with the upgrade data and the color of the difficulty from the difficulty database

    }

    public void OnPointerExit(PointerEventData eventData) {

        // destroy the info widget if it exists
        if (currInfoWidget != null)
            Destroy(currInfoWidget.gameObject);

    }

    private void OnClicked() {

        if (upgradeManager.PurchaseUpgrade(upgradeData)) {

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any existing fade coroutine to prevent conflicts
            fadeCoroutine = StartCoroutine(FadeTextColor(siteTitleText, purchasedColor, purchasedFadeDuration)); // fade the site title text to the purchased color to indicate that the upgrade has been purchased

        }
    }

    private IEnumerator FadeTextColor(TMP_Text text, Color targetColor, float fadeDuration) {

        text.gameObject.SetActive(true); // ensure the text is active for fading

        float currentTime = 0f;
        Color initialColor = siteTitleText.color;

        while (currentTime < fadeDuration) {

            text.color = Color.Lerp(initialColor, targetColor, currentTime / fadeDuration);
            currentTime += Time.deltaTime;
            yield return null;

        }

        text.color = targetColor;
        fadeCoroutine = null;

    }
}
