
using UnityEngine;

public class StreakManager : MonoBehaviour
{
    public static StreakManager Instance;

    public int currentStreak = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keep across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWin()
    {
        currentStreak++;
        Debug.Log("Win Streak: " + currentStreak);
    }

    public void ResetStreak()
    {
        currentStreak = 0;
        Debug.Log("Streak Reset");
    }
}
