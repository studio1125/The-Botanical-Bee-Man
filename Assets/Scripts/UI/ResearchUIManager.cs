using System.Collections;
using UnityEngine;

public class ResearchUIManager : BaseUIManager {

    [Header("UI References")]
    [SerializeField] private RectTransform laptopMenu;
    [SerializeField] private CanvasGroup clockBubble;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private float clockBubbleFadeDelay;
    [SerializeField] private float clockBubbleFadeDuration;

    private new void Start() {

        base.Start();
        RefreshLayout(laptopMenu); // refresh the layout of the laptop menu at the start to ensure all UI elements are properly aligned

        clockBubble.gameObject.SetActive(false); // initially hide the clock bubble
        clockBubble.alpha = 0f; // set the clock bubble's opacity to 0 to make it invisible

        Invoke(nameof(ShowClockBubble), clockBubbleFadeDelay); // schedule the clock bubble to be shown after the specified delay

    }

    private void ShowClockBubble() {

        clockBubble.gameObject.SetActive(true); // show the clock bubble

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any existing fade coroutine to prevent multiple coroutines from running simultaneously and causing unintended behavior
        fadeCoroutine = StartCoroutine(Fade(clockBubble, 1f, clockBubbleFadeDuration)); // start the fade coroutine to fade in the clock bubble

    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float targetOpacity, float fadeDuration) {

        float currentTime = 0f;
        float initialOpacity = canvasGroup.alpha;

        while (currentTime < fadeDuration) {

            canvasGroup.alpha = Mathf.Lerp(initialOpacity, targetOpacity, currentTime / fadeDuration);
            currentTime += Time.deltaTime;
            yield return null;

        }

        canvasGroup.alpha = targetOpacity; // ensure the final opacity is set to the target value at the end of the fade
        fadeCoroutine = null;

    }
}
