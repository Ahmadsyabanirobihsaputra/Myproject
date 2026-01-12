using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The player object that can trigger the scene change.")]
    public GameObject player;

    [Tooltip("The name of the scene to load when the player collides.")]
    public string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the assigned player
        if (other.gameObject == player)
        {
            // Load the next scene
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
