
using UnityEngine;

public class DoorSwitchTrigger : MonoBehaviour, IInteractable
{
    [Header("Door Objects")]
    [Tooltip("GameObject pintu tertutup (aktif di awal)")]
    public GameObject closedDoor;

    [Tooltip("GameObject pintu terbuka (nonaktif di awal)")]
    public GameObject openDoor;

    [Header("Interaction Settings")]
    public string interactionPrompt = "Press E to open door";

    private bool isSwitched = false;

    private void Start()
    {
        if (closedDoor != null)
            closedDoor.SetActive(true);

        if (openDoor != null)
            openDoor.SetActive(false);
    }

    public string GetPrompt()
    {
        if (isSwitched)
            return "";

        if (KeyManager.Instance != null &&
            KeyManager.Instance.PlayerHasKey)
        {
            return interactionPrompt;
        }

        return "Door is locked - need a key";
    }

    public void Interact()
    {
        if (isSwitched)
            return;

        if (KeyManager.Instance != null &&
            KeyManager.Instance.PlayerHasKey)
        {
            SwitchDoors();
        }
        else
        {
            Debug.Log("Door is locked. You need a key!");
        }
    }

    private void SwitchDoors()
    {
        isSwitched = true;

        if (closedDoor != null)
            closedDoor.SetActive(false);

        if (openDoor != null)
            openDoor.SetActive(true);

        Debug.Log("Door switched: closed -> open");
    }
}