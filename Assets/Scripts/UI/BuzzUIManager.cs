using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuzzUIManager : BaseUIManager {

    [Header("References")]
    private BuzzModeManager gameManager;

    [Header("Info HUD")]
    [SerializeField] private RectTransform infoHUD;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayText;

    [Header("Super HUD")]
    [SerializeField] private Slider superChargeSlider;
    [SerializeField] private Image superChargeSliderFill;
    [SerializeField] private Sprite superChargeSliderInProgressFill;
    [SerializeField] private Sprite superChargeSliderCompleteFill;

    private new void Start() {

        base.Start();

        gameManager = FindFirstObjectByType<BuzzModeManager>();
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

    public void UpdateSuperSlider(float percent) {

        superChargeSlider.value = percent;

        if (superChargeSlider.value == superChargeSlider.maxValue)
            superChargeSliderFill.sprite = superChargeSliderCompleteFill;
        else
            superChargeSliderFill.sprite = superChargeSliderInProgressFill;
    }
}
