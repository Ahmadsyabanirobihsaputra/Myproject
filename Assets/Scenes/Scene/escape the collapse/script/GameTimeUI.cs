
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


    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        UpdateTimeDisplay();
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