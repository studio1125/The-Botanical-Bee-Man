using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuzzUIManager : BaseUIManager {

    [Header("References")]
    private BuzzModeManager gameManager;
    private DataManager dataManager;
    private Coroutine textLerpCoroutine;

    [Header("Info HUD")]
    [SerializeField] private RectTransform infoHUD;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayText;

    [Header("Nectar HUD")]
    [SerializeField] private RectTransform nectarHUD;
    [SerializeField] private TMP_Text nectarText;

    [Header("Super HUD")]
    [SerializeField] private Slider superChargeSlider;

    [Header("Settings")]
    [SerializeField] private float textLerpDuration;

    private void Start() {

        gameManager = FindFirstObjectByType<BuzzModeManager>();
        dataManager = FindFirstObjectByType<DataManager>();

        dataManager.onNectarCollected += UpdateNectarText; // subscribe to the onNectarCollected action to update the nectar text when nectar is collected
        dayText.text = $"Day {PlayerData.GetCurrentDay()}"; // update the day text to show the current day at the start of the buzz mode

    }

    private void Update() {

        // update the timer text to show the time remaining in the format "0:00.00"
        float timeRemaining = gameManager.GetTimeRemaining();

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        int milliseconds = Mathf.FloorToInt((timeRemaining * 100f) % 100f);

        timerText.text = $"{minutes}:{seconds:00}.{milliseconds:00}";
        RefreshLayout(infoHUD); // refresh the layout of the info HUD to ensure the timer text is properly aligned after updating the text

    }

    private void OnDisable() => dataManager.onNectarCollected -= UpdateNectarText; // unsubscribe from the onNectarCollected action to prevent memory leaks when the object is disabled

    private void UpdateNectarText(int nectarCollected) {

        if (textLerpCoroutine != null) StopCoroutine(textLerpCoroutine); // stop the current text lerp coroutine if it's still running
        textLerpCoroutine = StartCoroutine(HandleTextLerp(nectarText, nectarCollected, textLerpDuration)); // start a new text lerp coroutine to animate the change in nectar text

    }

    public void UpdateSuperSlider(float percent) => superChargeSlider.value = percent;

    private IEnumerator HandleTextLerp(TMP_Text text, float targetValue, float lerpDuration) {

        float currentTime = 0f;
        float startValue = float.Parse(text.text); // parse the current text value as a float

        while (currentTime < lerpDuration) {

            text.text = Mathf.Lerp(startValue, targetValue, currentTime / lerpDuration).ToString("0"); // lerp the text value from the start value to the target value over the duration of the lerp
            currentTime += Time.deltaTime;
            RefreshLayout(nectarHUD); // refresh the layout of the nectar HUD to ensure the text is properly aligned after updating the text
            yield return null;

        }

        text.text = targetValue.ToString("0"); // ensure the text is set to the target value at the end of the lerp
        RefreshLayout(nectarHUD); // refresh the layout of the nectar HUD to ensure the text is properly aligned after updating the text
        textLerpCoroutine = null; // reset the coroutine reference

    }
}
