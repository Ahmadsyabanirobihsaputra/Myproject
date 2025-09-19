using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;



public class SceneRouletteHandler : MonoBehaviour
{
    [System.Serializable]
    public class SceneChance
    {
        [Tooltip("Name of the scene (must be in Build Settings).")]
        public string sceneName;

        [Min(0f)]
        [Tooltip("Chance weight (relative, not exact %). Higher = more likely.")]
        public float chanceWeight = 1f;
    }

    [Header("Scene Roulette Settings")]
    [Tooltip("List of possible scenes with their chances.")]
    public List<SceneChance> possibleScenes = new List<SceneChance>();

    public void LoadRandomScene()
    {
        if (possibleScenes.Count == 0)
        {
            Debug.LogWarning("No scenes assigned in SceneRouletteHandler!");
            return;
        }

        float totalWeight = 0f;
        foreach (var sc in possibleScenes)
        {
            totalWeight += Mathf.Max(0f, sc.chanceWeight);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("All scene weights are 0! Please assign positive values.");
            return;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var sc in possibleScenes)
        {
            cumulative += Mathf.Max(0f, sc.chanceWeight);
            if (roll <= cumulative)
            {
                if (!string.IsNullOrEmpty(sc.sceneName) && Application.CanStreamedLevelBeLoaded(sc.sceneName))
                {
                    Debug.Log($"[SceneRoulette] Loading scene: {sc.sceneName}");
                    SceneManager.LoadScene(sc.sceneName);
                }
                else
                {
                    Debug.LogWarning($"Scene '{sc.sceneName}' is not valid or not in Build Settings.");
                }
                return;
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
