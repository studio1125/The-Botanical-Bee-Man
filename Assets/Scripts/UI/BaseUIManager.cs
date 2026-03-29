using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseUIManager : MonoBehaviour {

    [Header("References")]
    protected DataManager dataManager;
    private Coroutine textLerpCoroutine;

    [Header("Nectar HUD")]
    [SerializeField] private RectTransform nectarHUD;
    [SerializeField] private TMP_Text nectarText;
    [SerializeField] private float textLerpDuration;

    protected void Start() {

        dataManager = FindFirstObjectByType<DataManager>();

        dataManager.onNectarCollected += UpdateNectarText; // subscribe to the onNectarCollected action to update the nectar text when nectar is collected
        UpdateNectarText(dataManager.GetNectarCollected()); // initialize the nectar text to show the current amount of nectar collected at the start of the buzz mode

    }

    protected void OnDisable() => dataManager.onNectarCollected -= UpdateNectarText; // unsubscribe from the onNectarCollected action to prevent memory leaks when the object is disabled

    private void UpdateNectarText(int nectarCollected) {

        if (textLerpCoroutine != null) StopCoroutine(textLerpCoroutine); // stop the current text lerp coroutine if it's still running
        textLerpCoroutine = StartCoroutine(HandleTextLerp(nectarText, nectarCollected, textLerpDuration)); // start a new text lerp coroutine to animate the change in nectar text

    }

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

    // IMPORTANT: this only works when the root is visible
    protected void RefreshLayout(RectTransform root) {

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(root); // force a rebuild of the root layout at the end

    }
}
