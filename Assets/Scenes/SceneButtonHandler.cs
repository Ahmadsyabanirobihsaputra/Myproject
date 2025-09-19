

using UnityEngine;
using UnityEngine.SceneManagement;



public class SceneButtonHandler : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Name of the scene to load (must be added to Build Settings).")]
    public string sceneToLoad;

    /// <summary>
    /// Loads the specified scene by name.
    /// </summary>
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning($"Scene '{sceneToLoad}' is not in Build Settings or misspelled.");
            }
        }
        else
        {
            Debug.LogWarning("Scene name is empty. Please assign a valid scene name in the inspector.");
        }
    }

    /// <summary>
    /// Quits the application. Works only in builds.
    /// </summary>
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