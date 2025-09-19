
using UnityEngine;

public class FiringRangeManager : MonoBehaviour
{
    public static FiringRangeManager Instance;

    [Header("Firing Range Stats")]
    public int totalShots = 0;
    public int hits = 0;
    public int misses = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterShot(bool wasHit)
    {
        totalShots++;

        if (wasHit)
        {
            hits++;
            Debug.Log("Hit registered! Total Hits: " + hits);
        }
        else
        {
            misses++;
            Debug.Log("Miss registered! Total Misses: " + misses);
        }
    }

    public float GetAccuracy()
    {
        if (totalShots == 0) return 0f;
        return (float)hits / totalShots * 100f;
    }

    public void ResetStats()
    {
        totalShots = 0;
        hits = 0;
        misses = 0;
        Debug.Log("Firing Range Stats Reset");
    }
}
