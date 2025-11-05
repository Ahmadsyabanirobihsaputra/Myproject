using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [Tooltip("Optional: GameObject to enable/disable when interacting.")]
    public GameObject targetObject;

    [Tooltip("If true, toggles the target object's active state on each interaction.")]
    public bool toggleObject = true;

    [Tooltip("Optional: Name of the scene to load when interacted.")]
    public string sceneToLoad;

    [Tooltip("If true, destroys this object after interaction.")]
    public bool destroyAfterUse = false;

    public void Interact()
    {
        Debug.Log($"Interacted with {name}");

        // Toggle target object
        if (targetObject != null)
        {
            if (toggleObject)
                targetObject.SetActive(!targetObject.activeSelf);
            else
                targetObject.SetActive(true);
        }

        // Load scene if specified
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning($"Scene '{sceneToLoad}' not found in Build Settings.");
            }
        }

        // Optional: destroy after use
        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }

    public string GetPrompt()
    {
        return "Press E to interact";
    }
}
