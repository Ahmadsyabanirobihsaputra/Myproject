using UnityEngine;

public class ButtonInteract : MonoBehaviour, IInteractable
{
    public GameObject door; // Example: something to activate when pressed

    public void Interact()
    {
        Debug.Log("Button pressed!");
        if (door != null)
        {
            door.SetActive(!door.activeSelf); // Toggle door on/off
        }
    }

    public string GetPrompt()
    {
        return "Press E to press the button";
    }
}
