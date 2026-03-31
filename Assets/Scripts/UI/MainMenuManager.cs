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
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousBuzzButton; // the previous button on the last page that goes back to the second to last page (specifically for when the buzz button is being shown)
    [SerializeField] private Button buzzButton;
    private int currentPageIndex;
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

        previousButton.onClick.AddListener(OpenPreviousTutorialPage);
        nextButton.onClick.AddListener(OpenNextTutorialPage);
        previousBuzzButton.onClick.AddListener(OpenPreviousTutorialPage); // the previous buzz button does the same thing as the regular previous button, it just shows up on the last page when the buzz button is shown
        buzzButton.onClick.AddListener(LoadBuzzMode);

        // make sure the tutorial section is hidden by default
        tutorialSection.gameObject.SetActive(false);
        tutorialSection.alpha = 0f;

        // make sure the credits section is hidden by default
        creditsSection.gameObject.SetActive(false);
        creditsSection.alpha = 0f;

        ResetTutorial(); // reset the tutorial to ensure it starts on the first page with the correct buttons shown

        RefreshLayout(mainMenu); // refresh the layout to ensure everything is positioned correctly at the start

    }

    private void OpenTutorial() {

        if (isTutorialVisible) return; // if the tutorial is already visible, do nothing

        ResetTutorial(); // reset the tutorial to ensure it starts on the first page with the correct buttons shown

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

    private void OpenPreviousTutorialPage() {

        if (currentPageIndex > 0) {

            pages[currentPageIndex].SetActive(false); // hide the current page
            currentPageIndex--; // move to the previous page index
            pages[currentPageIndex].SetActive(true); // show the previous page
            RefreshLayout(tutorialSection.GetComponent<RectTransform>()); // refresh the layout to ensure everything is positioned correctly

            // we cannot be on the last page if we're moving back
            // essentially, the buzz button is only shown on the last page, and the previous buzz button is also only shown on the last page, but only if there is a previous page to go back to (e.g. if there's only one page, the buzz button is shown but the previous buzz button is not shown since there's no previous page to go back to); both the next button and previous button are hidden
            // if we're not on the last page, the next button is shown and the previous button is shown if we're past the first page; both the buzz button and the previous buzz button are hidden
            if (currentPageIndex == pages.Length - 1) {

                // we are on the last page (this case is technically not possible, but it is kept here for clarity and simplicity of the logic)

                previousButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(false);
                previousBuzzButton.gameObject.SetActive(true);
                buzzButton.gameObject.SetActive(true);

            } else {

                // we are not on the last page

                previousButton.gameObject.SetActive(currentPageIndex > 0); // only shown if we're past the first page
                nextButton.gameObject.SetActive(true);
                previousBuzzButton.gameObject.SetActive(false);
                buzzButton.gameObject.SetActive(false);

            }
        }
    }

    private void OpenNextTutorialPage() {

        if (currentPageIndex < pages.Length - 1) {

            pages[currentPageIndex].SetActive(false); // hide the current page
            currentPageIndex++; // move to the next page index
            pages[currentPageIndex].SetActive(true); // show the next page
            RefreshLayout(tutorialSection.GetComponent<RectTransform>()); // refresh the layout to ensure everything is positioned correctly

            // we cannot be on the first page if we're moving forward
            // essentially, the buzz button is only shown on the last page, and the previous buzz button is also only shown on the last page, but only if there is a previous page to go back to (e.g. if there's only one page, the buzz button is shown but the previous buzz button is not shown since there's no previous page to go back to); both the next button and previous button are hidden
            // if we're not on the last page, the next button is shown and the previous button is shown if we're past the first page; both the buzz button and the previous buzz button are hidden
            if (currentPageIndex == pages.Length - 1) {

                // we are on the last page

                previousButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(false);
                previousBuzzButton.gameObject.SetActive(true);
                buzzButton.gameObject.SetActive(true);

            } else {

                // we are not on the last page, but we can't be on the first page either

                previousButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                previousBuzzButton.gameObject.SetActive(false);
                buzzButton.gameObject.SetActive(false);

            }
        }
    }

    private void ResetTutorial() {

        currentPageIndex = 0; // start on the first page of the tutorial

        if (pages.Length > 1) {

            // if there are multiple pages, show the next button and hide the buzz button and the previous buzz button since we're on the first page
            nextButton.gameObject.SetActive(true);
            previousBuzzButton.gameObject.SetActive(false);
            buzzButton.gameObject.SetActive(false);

        } else {

            // if there's only one page, hide the next button and the previous buzz button and show the buzz button since we're on the only page which is also the last page
            nextButton.gameObject.SetActive(false);
            previousBuzzButton.gameObject.SetActive(false);
            buzzButton.gameObject.SetActive(true);

        }

        previousButton.gameObject.SetActive(false); // the previous button starts off inactive either way since we're on the first page

        // disable all the tutorial pages except the first one
        for (int i = 1; i < pages.Length; i++)
            pages[i].SetActive(false);

        pages[0].SetActive(true); // make sure the first page is active

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
