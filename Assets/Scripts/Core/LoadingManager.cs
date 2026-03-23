using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject loadingScreen;
    private AsyncOperation currSceneOperation;
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private float fadeDuration;
    [SerializeField, Tooltip("Minimum time to show the loading screen (in seconds)")] private float minLoadingTime;

    private void Awake() {

        canvasGroup = GetComponent<CanvasGroup>();
        loadingScreen.SetActive(true); // show the loading screen

    }

    public void LoadScene(string sceneName) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName); // start loading the scene asynchronously
        operation.allowSceneActivation = false; // prevent the scene from activating immediately when it finishes loading
        ShowLoadingScreen(operation); // show the loading screen and allow the scene to load after the loading screen is shown

    }

    // shows the loading screen and allows the scene to load after the loading screen is shown
    private void ShowLoadingScreen(AsyncOperation operation) {

        loadingScreen.SetActive(true); // show the loading screen

        // if there is an existing scene operation, allow it to activate since the current coroutine will be stopped and we don't want the player to get stuck on a loading screen
        if (currSceneOperation != null)
            currSceneOperation.allowSceneActivation = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(Fade(1f, operation));

    }

    public void HideLoadingScreen() {

        loadingScreen.SetActive(true); // hide the loading screen

        // if there is an existing scene operation, allowing it to activate since the current coroutine will be stopped and we don't want the player to get stuck on a loading screen
        if (currSceneOperation != null)
            currSceneOperation.allowSceneActivation = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(Fade(0f));

    }

    // fades the loading screen in or out and optionally allows the scene to load after the fade
    private IEnumerator Fade(float targetOpacity, AsyncOperation operation = null) {

        this.currSceneOperation = operation; // store the current scene operation

        float currentTime = 0f;
        float initialOpacity = canvasGroup.alpha;

        while (currentTime < fadeDuration) {

            canvasGroup.alpha = Mathf.Lerp(initialOpacity, targetOpacity, currentTime / fadeDuration);
            currentTime += Time.unscaledDeltaTime; // use unscaled time to avoid issues with time scale changes (pausing)
            yield return null;

        }

        canvasGroup.alpha = targetOpacity;

        // if operation was passed, allow the scene to activate
        if (operation != null) {

            // wait until the minimum loading time has passed before allowing the scene to activate (if operation was passed) to ensure the loading screen is shown for at least the minimum time
            if (minLoadingTime > fadeDuration)
                yield return new WaitForSeconds(minLoadingTime - fadeDuration);

            operation.allowSceneActivation = true;

        }

        // hide the loading screen if faded out
        if (targetOpacity == 0f)
            loadingScreen.SetActive(false);

        fadeCoroutine = null;

    }
}
