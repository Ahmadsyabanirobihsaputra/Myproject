using UnityEngine;
using UnityEngine.UI;

public class WorldButton : MonoBehaviour, IInteractable
{
    public Button button; // Reference to the actual UI button

    void Start()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Interact()
    {
        if (button != null)
        {
            button.onClick.Invoke(); // Simulate button press
            Debug.Log("World Button pressed via Interact()");
        }
    }

    public string GetPrompt()
    {
        return "Press E to press the button";
    }
}
