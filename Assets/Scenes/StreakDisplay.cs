using UnityEngine;
using UnityEngine.UI;


public class StreakDisplay : MonoBehaviour
{
    public Text streakText;

    void Start()
    {
        int streak = PlayerPrefs.GetInt("WinStreak", 0);
        streakText.text = "Win Streak: " + streak;
    }
}
