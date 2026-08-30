
using TMPro;
using UnityEngine;

public class GameTimeUI : MonoBehaviour
{
    // =========================================================
    // UI REFERENCES
    // =========================================================

    [Header("Game Time")]
    public TMP_Text gameTimeText;

    [Header("Best Time")]
    public TMP_Text bestTimeText;

    [Header("Live Update")]
    [Tooltip("If true, gameTimeText keeps refreshing every frame while the " +
             "timer is running (a real-time HUD). If false, it only shows " +
             "the time once at Start (e.g. a static result screen).")]
    public bool liveUpdateGameTime = true;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        UpdateTimeDisplay();
    }

    private void Update()
    {
        // Only keep refreshing the running game time - the best time never
        // changes mid-frame, so no need to touch it every tick.
        if (!liveUpdateGameTime)
            return;

        if (GameTimeManager.Instance == null)
            return;

        if (!GameTimeManager.Instance.IsTimerRunning())
            return;

        if (gameTimeText != null)
        {
            gameTimeText.text =
                GameTimeManager.Instance.GetFormattedGameTime();
        }
    }

    // =========================================================
    // UPDATE DISPLAY
    // =========================================================

    public void UpdateTimeDisplay()
    {
        if (GameTimeManager.Instance == null)
        {
            Debug.LogWarning(
                "[GameTimeUI] GameTimeManager not found!"
            );

            return;
        }

        // Game Time
        if (gameTimeText != null)
        {
            gameTimeText.text =
                GameTimeManager.Instance.GetFormattedGameTime();
        }

        // Best Time
        if (bestTimeText != null)
        {
            bestTimeText.text =
                GameTimeManager.Instance.GetFormattedBestTime();
        }
    }
}