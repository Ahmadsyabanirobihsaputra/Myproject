
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
            CancelRun();

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
    // PLAYER ENTERED DEATH SCENE
    // =========================================================

    public void CancelRun()
    {
        if (!timerRunning)
            return;

        timerRunning = false;

        Debug.Log("================================");
        Debug.Log("RUN CANCELLED");
        Debug.Log("PLAYER ENTERED DEATH SCENE");
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
        PlayerPrefs.DeleteKey(
            BEST_TIME_KEY
        );

        PlayerPrefs.Save();

        bestTime = -1f;

        Debug.Log("Best Time Deleted.");
    }
}