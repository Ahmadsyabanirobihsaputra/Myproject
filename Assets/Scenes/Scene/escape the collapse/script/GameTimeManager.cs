
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimeManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static GameTimeManager Instance { get; private set; }


    // =========================================================
    // SCENE SETTINGS
    // =========================================================

    [Header("Scene Detection")]

    [Tooltip("Scene name that cancels the current run.")]
    public string deathSceneName = "DeathScreen";

    [Tooltip("Scene name that completes the current run.")]
    public string winSceneName = "WinScreen";

    [Tooltip("Scene name of the main menu / lobby. If the player returns " +
             "here while a run is still in progress (e.g. they quit mid-level " +
             "instead of dying or winning), the run is cancelled the same way " +
             "a death would, so the time is not left running or counted.")]
    public string mainMenuSceneName = "MainMenu";


    // =========================================================
    // TIMER DATA
    // =========================================================

    private float currentGameTime = 0f;

    private float bestTime = -1f;

    private bool timerRunning = false;


    // =========================================================
    // PLAYER PREFS
    // =========================================================

    private const string BEST_TIME_KEY = "BestGameTime";


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // Prevent duplicate manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep manager between scenes
        DontDestroyOnLoad(gameObject);

        // Load saved best time
        LoadBestTime();
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void Update()
    {
        if (!timerRunning)
            return;

        currentGameTime += Time.deltaTime;
    }


    // =========================================================
    // SCENE LOADED DETECTION
    // =========================================================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string loadedSceneName = scene.name;

        Debug.Log(
            "[GameTimeManager] Scene Loaded: " +
            loadedSceneName
        );


        // -----------------------------------------------------
        // DEATH SCENE
        // -----------------------------------------------------

        if (!string.IsNullOrEmpty(deathSceneName) &&
            loadedSceneName == deathSceneName)
        {
            CancelRun("Player entered death scene");

            return;
        }


        // -----------------------------------------------------
        // WIN SCENE
        // -----------------------------------------------------

        if (!string.IsNullOrEmpty(winSceneName) &&
            loadedSceneName == winSceneName)
        {
            StopTimer();

            return;
        }


        // -----------------------------------------------------
        // MAIN MENU / LOBBY
        // Player left the level early without dying or winning.
        // Treat it the same as a cancelled run so the time doesn't stay
        // running or get mistaken for a completed/best time.
        // -----------------------------------------------------

        if (!string.IsNullOrEmpty(mainMenuSceneName) &&
            loadedSceneName == mainMenuSceneName)
        {
            if (timerRunning)
            {
                CancelRun("Player returned to main menu / lobby");
            }

            return;
        }
    }


    // =========================================================
    // START NEW RUN
    // =========================================================

    public void StartTimer()
    {
        currentGameTime = 0f;

        timerRunning = true;

        Debug.Log("================================");
        Debug.Log("GAME TIMER STARTED");
        Debug.Log("================================");
    }


    // =========================================================
    // COMPLETE RUN
    // =========================================================

    public void StopTimer()
    {
        if (!timerRunning)
            return;

        timerRunning = false;

        Debug.Log("================================");
        Debug.Log("GAME COMPLETED");
        Debug.Log("GAME TIME: " + GetFormattedGameTime());
        Debug.Log("================================");

        CheckBestTime();
    }


    // =========================================================
    // CANCEL RUN
    // Used both for "player died" and "player quit to main menu".
    // The reason is only used for the log message so it's clear which
    // case triggered it, but the effect (stop + reset, no score saved)
    // is identical either way.
    // =========================================================

    public void CancelRun(string reason = "Player entered death scene")
    {
        if (!timerRunning)
            return;

        timerRunning = false;

        Debug.Log("================================");
        Debug.Log("RUN CANCELLED");
        Debug.Log(reason);
        Debug.Log("TIME WAS NOT SAVED");
        Debug.Log("================================");

        // Reset current run time
        currentGameTime = 0f;
    }


    // =========================================================
    // RESET TIMER
    // =========================================================

    public void ResetTimer()
    {
        currentGameTime = 0f;

        timerRunning = false;

        Debug.Log("Game Timer Reset.");
    }


    // =========================================================
    // CHECK BEST TIME
    // =========================================================

    private void CheckBestTime()
    {
        // First completed run
        if (bestTime < 0f)
        {
            bestTime = currentGameTime;

            SaveBestTime();

            Debug.Log("NEW BEST TIME!");

            return;
        }


        // Faster time = better
        if (currentGameTime < bestTime)
        {
            bestTime = currentGameTime;

            SaveBestTime();

            Debug.Log("NEW BEST TIME!");
        }
        else
        {
            Debug.Log(
                "Best Time remains: " +
                GetFormattedBestTime()
            );
        }
    }


    // =========================================================
    // SAVE BEST TIME
    // =========================================================

    private void SaveBestTime()
    {
        PlayerPrefs.SetFloat(
            BEST_TIME_KEY,
            bestTime
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // LOAD BEST TIME
    // =========================================================

    private void LoadBestTime()
    {
        if (PlayerPrefs.HasKey(BEST_TIME_KEY))
        {
            bestTime =
                PlayerPrefs.GetFloat(BEST_TIME_KEY);

            Debug.Log(
                "Loaded Best Time: " +
                GetFormattedBestTime()
            );
        }
        else
        {
            bestTime = -1f;

            Debug.Log("No Best Time Found.");
        }
    }


    // =========================================================
    // GET CURRENT GAME TIME
    // =========================================================

    public float GetGameTime()
    {
        return currentGameTime;
    }


    // =========================================================
    // GET BEST TIME
    // =========================================================

    public float GetBestTime()
    {
        return bestTime;
    }


    // =========================================================
    // TIMER STATE
    // =========================================================

    public bool IsTimerRunning()
    {
        return timerRunning;
    }


    // =========================================================
    // FORMATTED GAME TIME
    // HH:MM:SS
    // =========================================================

    public string GetFormattedGameTime()
    {
        return FormatTime(currentGameTime);
    }


    // =========================================================
    // FORMATTED BEST TIME
    // HH:MM:SS
    // =========================================================

    public string GetFormattedBestTime()
    {
        if (bestTime < 0f)
            return "--:--:--";

        return FormatTime(bestTime);
    }


    // =========================================================
    // FORMAT TIME
    // =========================================================

    private string FormatTime(float time)
    {
        int totalSeconds =
            Mathf.FloorToInt(time);

        int hours =
            totalSeconds / 3600;

        int minutes =
            (totalSeconds % 3600) / 60;

        int seconds =
            totalSeconds % 60;

        return string.Format(
            "{0:00}:{1:00}:{2:00}",
            hours,
            minutes,
            seconds
        );
    }


    // =========================================================
    // DELETE BEST TIME
    // =========================================================

    public void DeleteBestTime()
    {
        PlayerPrefs.DeleteKey(BEST_TIME_KEY);
        PlayerPrefs.Save();

        bestTime = -1f;

        Debug.Log("Best Time Deleted.");

        // Update all GameTimeUI instances
        GameTimeUI[] allTimeUI = FindObjectsOfType<GameTimeUI>();

        foreach (GameTimeUI ui in allTimeUI)
        {
            ui.UpdateTimeDisplay();
        }
    }
}