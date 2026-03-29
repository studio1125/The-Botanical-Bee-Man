using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour {

    // how it works:
    // 1. when LoadScene is called, the black screen fades in to hide the current scene, and the new scene starts loading in the background
    // 2. once the fade is complete, the loading screen is activated (but is still covered by the black screen)
    // 3. the black screen fades out to reveal the loading screen, and the new scene continues loading in the background
    // 4. once the new scene is fully loaded, the loading screen is faded out to reveal the new scene (in the Awake method)

    [Header("References")]
    [SerializeField] private RectTransform content;
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingPercentText;
    [SerializeField] private TMP_Text funFactText;
    private AsyncOperation currSceneOperation;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private float loadingScreenFadeDuration;
    [SerializeField] private float blackScreenFadeDuration;
    [SerializeField] private float blackScreenVisibleDuration; // how long the black screen should stay fully visible before fading out to reveal the loading screen (in seconds)
    [SerializeField, Tooltip("Minimum time to show the loading screen (in seconds)")] private float minLoadingTime;

    private void Awake() {

        blackScreen.gameObject.SetActive(false); // hide the black screen
        blackScreen.alpha = 0f; // make the black screen invisible

        loadingSlider.value = 100f; // set the loading slider to 100% at the start of the new scene to indicate that the new scene is fully loaded
        loadingPercentText.text = "100%"; // set the loading percent text to 100% at the start of the new scene to indicate that the new scene is fully loaded

        funFactText.text = TempStorage.GetCurrentFunFact(); // set the fun fact text to the current fun fact stored in TempStorage (which was set when the loading screen was shown in the previous scene)
        RefreshLayout(content); // refresh the layout of the loading screen content to ensure the fun fact text is properly aligned after updating the text

    }

    public void LoadScene(string sceneName, string funFact, int nectarCollected) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName); // start loading the scene asynchronously
        operation.allowSceneActivation = false; // prevent the scene from activating immediately when it finishes loading
        ShowLoadingScreen(operation, funFact, nectarCollected); // show the loading screen and allow the scene to load after the loading screen is shown

    }

    // shows the loading screen and allows the scene to load after the loading screen is shown
    private void ShowLoadingScreen(AsyncOperation operation, string funFact, int nectarCollected) {

        // if there is an existing scene operation, allow it to activate since the current coroutine will be stopped and we don't want the player to get stuck on a loading screen
        if (currSceneOperation != null)
            currSceneOperation.allowSceneActivation = true;

        blackScreen.gameObject.SetActive(true); // show the black screen

        TempStorage.SetCurrentFunFact(funFact); // store the current fun fact in TempStorage so it can be accessed in the new scene to display on the loading screen there
        funFactText.text = funFact; // set a random fun fact on the loading screen

        TempStorage.SetNectarCollected(nectarCollected); // store the amount of nectar collected in TempStorage so it can be accessed in the new scene

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(HandleFadeSequence(operation));

    }

    public void HideLoadingScreen() {

        // if there is an existing scene operation, allowing it to activate since the current coroutine will be stopped and we don't want the player to get stuck on a loading screen
        if (currSceneOperation != null)
            currSceneOperation.allowSceneActivation = true;

        loadingScreen.gameObject.SetActive(true); // hide the loading screen
        TempStorage.SetCurrentFunFact(null); // clear the current fun fact from TempStorage since it's no longer needed

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(Fade(loadingScreen, 0f, loadingScreenFadeDuration));

    }

    private IEnumerator HandleFadeSequence(AsyncOperation operation) {

        this.currSceneOperation = operation; // store the current scene operation

        loadingSlider.value = 0f; // reset the loading slider to 0 at the start of the loading sequence
        loadingPercentText.text = "0%"; // reset the loading percent text to 0% at the start of the loading sequence

        yield return Fade(blackScreen, 1f, blackScreenFadeDuration); // fade in the black screen to hide the current scene

        loadingScreen.gameObject.SetActive(true); // show the loading screen while the new scene continues loading in the background
        loadingScreen.alpha = 1f; // make the loading screen fully visible immediately after activating it

        RefreshLayout(content); // refresh the layout of the loading screen content to ensure the fun fact text is properly aligned after updating the text

        yield return new WaitForSeconds(blackScreenVisibleDuration); // wait for the black screen to be fully visible for a moment before fading out to reveal the loading screen

        yield return Fade(blackScreen, 0f, blackScreenFadeDuration); // fade out the black screen to reveal the loading screen

        // if the minimum loading time is greater than the time it takes to fade out the loading screen, we need to wait for the remaining time after fading out the black screen before allowing the scene to activate, and we can also animate the loading slider during this time to give the player feedback that the game is still loading
        if (minLoadingTime > loadingScreenFadeDuration) {

            float currentTime = 0f;

            while (currentTime < minLoadingTime - loadingScreenFadeDuration) {

                loadingSlider.value = Mathf.Lerp(0f, 0.9f, currentTime / (minLoadingTime - loadingScreenFadeDuration)); // lerp the slider value from 0 to 0.9 over the duration of the minimum loading time (minus the time it takes to fade out the loading screen)
                loadingPercentText.text = Mathf.RoundToInt(loadingSlider.value * 100f) + "%"; // update the loading percent text to show the current slider value as a percentage
                currentTime += Time.deltaTime;
                yield return null;

            }

            loadingSlider.value = 0.9f; // set the slider value to 0.9 at the end of the minimum loading time to indicate that the loading is almost complete
            loadingPercentText.text = "90%"; // update the loading percent text to show 90% at the end of the minimum loading time

        }

        currSceneOperation.allowSceneActivation = true; // allow the new scene to activate once it's finished loading

        while (!currSceneOperation.isDone) { // wait until the new scene is fully loaded

            loadingSlider.value = Mathf.Lerp(0.9f, 1f, currSceneOperation.progress / 0.9f); // lerp the slider value from 0.9 to 1 based on the progress of the scene loading (currSceneOperation.progress goes from 0 to 0.9 as the scene loads, and then jumps to 1 when the scene is fully loaded)
            loadingPercentText.text = Mathf.RoundToInt(loadingSlider.value * 100f) + "%"; // update the loading percent text to show the current slider value as a percentage
            yield return null;

        }
    }

    private IEnumerator Fade(CanvasGroup screen, float targetOpacity, float fadeDuration) {

        float currentTime = 0f;
        float initialOpacity = screen.alpha;

        while (currentTime < fadeDuration) {

            screen.alpha = Mathf.Lerp(initialOpacity, targetOpacity, currentTime / fadeDuration);
            currentTime += Time.unscaledDeltaTime; // use unscaled time to avoid issues with time scale changes (pausing)
            yield return null;

        }

        screen.alpha = targetOpacity;

        // hide the screen if faded out
        if (targetOpacity == 0f)
            screen.gameObject.SetActive(false);

        fadeCoroutine = null;

    }

    // IMPORTANT: this only works when the root is visible
    private void RefreshLayout(RectTransform root) {

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(root); // force a rebuild of the root layout at the end

    }
}
