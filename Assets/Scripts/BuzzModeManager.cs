using System;
using UnityEngine;

public class BuzzModeManager : BaseModeManager {

    [Header("References")]
    [SerializeField] private BoxCollider2D mapBounds;

    [Header("Settings")]
    [SerializeField, Tooltip("Amount of seconds in a day")] private float dayDuration;
    private int currentDay;
    private float timeRemaining;
    private bool isDayOver;

    private new void Start() {

        base.Start();

        timeRemaining = dayDuration;
        currentDay = 1;

    }

    private void Update() {

        if (isDayOver) return; // if the day is already over, skip the update

        // if the time remaining is less than or equal to 0, start a new day
        if (timeRemaining <= 0f) {

            isDayOver = true; // set the isDayOver flag to true to indicate that the day has ended
            currentDay++;
            timeRemaining = 0f; // reset the time remaining to ensure it doesn't go negative
            loadingManager.LoadScene(Constants.RESEARCH_MODE_SCENE_NAME); // load the research mode scene at the end of the day

            // don't update the day text here; it is only updated when the player returns to buzz mode after the research mode scene is loaded (so in the Start method)

        } else {

            timeRemaining -= Time.deltaTime; // decrease the time remaining by the time that has passed since the last frame

        }
    }

    public BoxCollider2D GetMapBounds() => mapBounds;

    public float GetDayDuration() => dayDuration;

    public int GetCurrentDay() => currentDay;

    public float GetTimeRemaining() => timeRemaining;

}
