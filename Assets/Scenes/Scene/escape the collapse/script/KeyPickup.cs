using UnityEngine;
using UnityEngine.UI;

public class KeyPickup : MonoBehaviour, IInteractable
{
    [Header("UI Pesan")]
    public Text messageText;
    public float messageDuration = 2f;

    private bool isCollected = false;

    public void Interact()
    {
        if (isCollected) return;

        isCollected = true;
        KeyManager.Instance.PlayerHasKey = true;

        // Hilangkan kunci dari scene
        gameObject.SetActive(false);

        // Tampilkan pesan
        if (messageText != null)
        {
            messageText.text = "Key Obtained!";
            messageText.enabled = true;
            Invoke(nameof(HideMessage), messageDuration);
        }

        Debug.Log("Key obtained!");
    }

    public string GetPrompt()
    {
        if (!isCollected)
            return "Press E to pick up key";
        return "";
    }

    private void HideMessage()
    {
        if (messageText != null)
            messageText.enabled = false;
    }
}
