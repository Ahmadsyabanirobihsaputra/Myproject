using UnityEngine;
using UnityEngine.UI;


public class StreakUI : MonoBehaviour
{
    public Text streakText;

    void Update()
    {
        if (StreakManager.Instance != null)
            streakText.text = "Win Streak: " + StreakManager.Instance.currentStreak;
    }
}
