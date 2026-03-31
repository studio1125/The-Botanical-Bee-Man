using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// doesn't inherit from BaseUIManager because the main menu is a separate kind of UI
public class MainMenuManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private RectTransform mainMenu;
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    private FunFactDatabase funFactDatabase;
    protected LoadingManager loadingManager;
    private Coroutine menuFadeCoroutine;

    [Header("Tutorial Section")]
    [SerializeField] private CanvasGroup tutorialSection;
    [SerializeField] private float tutorialFadeDuration;
    [SerializeField] private Button tutorialCloseButton;
    [SerializeField] private Button buzzButton;
    private bool isTutorialVisible;

    [Header("Credits Section")]
    [SerializeField] private CanvasGroup creditsSection;
    [SerializeField] private float creditsFadeDuration;
    [SerializeField] private Button creditsCloseButton;
    private bool isCreditsVisible;

    private void Start() {

        funFactDatabase = FindFirstObjectByType<FunFactDatabase>();
        loadingManager = FindFirstObjectByType<LoadingManager>();
        loadingManager.HideLoadingScreen(); // hide the loading screen at the start of the game

        playButton.onClick.AddListener(OpenTutorial);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(Quit);

        tutorialCloseButton.onClick.AddListener(CloseTutorial);
        creditsCloseButton.onClick.AddListener(CloseCredits);

        buzzButton.onClick.AddListener(LoadBuzzMode);

        // make sure the tutorial section is hidden by default
        tutorialSection.gameObject.SetActive(false);
        tutorialSection.alpha = 0f;

        // make sure the credits section is hidden by default
        creditsSection.gameObject.SetActive(false);
        creditsSection.alpha = 0f;

        RefreshLayout(mainMenu); // refresh the layout to ensure everything is positioned correctly at the start

    }

    private void OpenTutorial() {

        if (isTutorialVisible) return; // if the tutorial is already visible, do nothing

        if (menuFadeCoroutine != null) StopCoroutine(menuFadeCoroutine); // stop any existing fade coroutine to prevent conflicts
        menuFadeCoroutine = StartCoroutine(FadeMenu(tutorialSection, 1f, tutorialFadeDuration));

        RefreshLayout(tutorialSection.GetComponent<RectTransform>()); // refresh the layout to ensure everything is positioned correctly

        isTutorialVisible = true;

    }

    private void CloseTutorial() {

        if (!isTutorialVisible) return; // if the tutorial is already hidden, do nothing

        if (menuFadeCoroutine != null) StopCoroutine(menuFadeCoroutine); // stop any existing fade coroutine to prevent conflicts
        menuFadeCoroutine = StartCoroutine(FadeMenu(tutorialSection, 0f, tutorialFadeDuration));

        isTutorialVisible = false;

    }

    private void OpenCredits() {

        if (isCreditsVisible) return; // if the credits are already visible, do nothing

        if (menuFadeCoroutine != null) StopCoroutine(menuFadeCoroutine); // stop any existing fade coroutine to prevent conflicts
        menuFadeCoroutine = StartCoroutine(FadeMenu(creditsSection, 1f, creditsFadeDuration));

        RefreshLayout(creditsSection.GetComponent<RectTransform>()); // refresh the layout to ensure everything is positioned correctly

        isCreditsVisible = true;

    }

    private void CloseCredits() {

        if (!isCreditsVisible) return; // if the credits are already hidden, do nothing

        if (menuFadeCoroutine != null) StopCoroutine(menuFadeCoroutine); // stop any existing fade coroutine to prevent conflicts
        menuFadeCoroutine = StartCoroutine(FadeMenu(creditsSection, 0f, creditsFadeDuration));

        isCreditsVisible = false;

    }

    // TODO: edit the 0 nectar collected value to reflect the player's actual progress if they return to the main menu after playing
    private void LoadBuzzMode() {

        loadingManager.LoadScene(Constants.BUZZ_MODE_SCENE_NAME, funFactDatabase.GetRandomFunFact(), 0); // load buzz mode with a random fun fact and 0 nectar collected since the player hasn't played yet
        EventSystem.current.SetSelectedGameObject(null); // clear the selected game object to prevent any UI navigation issues when the new scene loads

    }

    private void Quit() => Application.Quit();

    protected IEnumerator FadeMenu(CanvasGroup canvasGroup, float targetOpacity, float fadeDuration) {

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

        menuFadeCoroutine = null; // reset the coroutine reference when done

    }

    // IMPORTANT: this only works when the root is visible
    protected void RefreshLayout(RectTransform root) {

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(root); // force a rebuild of the root layout at the end

    }
}
