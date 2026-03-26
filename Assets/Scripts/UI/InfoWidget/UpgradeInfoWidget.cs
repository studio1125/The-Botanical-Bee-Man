using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeInfoWidget : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private RectTransform canvasRect;

    [Header("Bounds")]
    private Vector3[] canvasCorners;

    [Header("Settings")]
    [SerializeField] private float fadeDuration;

    public void Initialize(UpgradeData upgradeData) {

        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvasRect = FindFirstObjectByType<BaseUIManager>().GetComponent<RectTransform>(); // find UIManager not Canvas because there are multiple canvases in the scene

        canvasGroup.alpha = 0f; // make the widget invisible

        // get the corners of the canvas
        canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        upgradeIcon.sprite = upgradeData.GetIcon();
        nameText.text = upgradeData.GetSiteName();
        descriptionText.text = upgradeData.GetConciseDescription();

        StartCoroutine(Fade(1f, fadeDuration)); // fade the widget in to full opacity

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform); // force the layout to update to fix any issues with the widget's size or position

    }

    private void Update() {

        transform.position = Input.mousePosition; // set the position of the widget to the mouse position
        Vector3 widgetPos = transform.position; // store the position of the widget

        // get the corners of the widget
        Vector3[] widgetCorners = new Vector3[4];
        rectTransform.GetWorldCorners(widgetCorners);

        // if the widget is outside the canvas bounds on the x axis, move it back in
        if (widgetCorners[0].x < canvasCorners[0].x)
            widgetPos.x += canvasCorners[0].x - widgetCorners[0].x;
        else if (widgetCorners[2].x > canvasCorners[2].x)
            widgetPos.x -= widgetCorners[2].x - canvasCorners[2].x;

        // if the widget is outside the canvas bounds on the y axis, move it back in
        if (widgetCorners[0].y < canvasCorners[0].y)
            widgetPos.y += canvasCorners[0].y - widgetCorners[0].y;
        else if (widgetCorners[1].y > canvasCorners[1].y)
            widgetPos.y -= widgetCorners[1].y - canvasCorners[1].y;

        transform.position = widgetPos; // set the position of the widget

    }

    private IEnumerator Fade(float targetOpacity, float fadeDuration) {

        float currentTime = 0f;
        float initialOpacity = canvasGroup.alpha;

        while (currentTime < fadeDuration) {

            canvasGroup.alpha = Mathf.Lerp(initialOpacity, targetOpacity, currentTime / fadeDuration);
            currentTime += Time.deltaTime;
            yield return null;

        }

        canvasGroup.alpha = targetOpacity;

    }
}
