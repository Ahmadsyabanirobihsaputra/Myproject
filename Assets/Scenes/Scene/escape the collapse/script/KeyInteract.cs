using UnityEngine;

public class KeyInteract : MonoBehaviour, IInteractable
{
    public static bool hasKey = false;

    public string GetPrompt()
    {
        return "Press E to pick up key";
    }

    public void Interact()
    {
        if (!hasKey)
        {
            hasKey = true;
            Debug.Log("Key obtained!");
            gameObject.SetActive(false);
        }
    }
}
