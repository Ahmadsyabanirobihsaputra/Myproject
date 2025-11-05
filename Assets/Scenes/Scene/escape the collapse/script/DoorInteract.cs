using UnityEngine;

public class DoorSwitchTrigger : MonoBehaviour
{
    [Header("Door Objects")]
    [Tooltip("GameObject pintu tertutup (aktif di awal)")]
    public GameObject closedDoor;

    [Tooltip("GameObject pintu terbuka (nonaktif di awal)")]
    public GameObject openDoor;

    [Header("Trigger Settings")]
    public float interactDistance = 3f; // jarak interaksi
    private Transform player;
    private bool isSwitched = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Pastikan kondisi awal
        if (closedDoor != null) closedDoor.SetActive(true);
        if (openDoor != null) openDoor.SetActive(false);
    }

    private void Update()
    {
        if (isSwitched || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            if (KeyManager.Instance != null && KeyManager.Instance.PlayerHasKey)
            {
                SwitchDoors();
            }
            else
            {
                Debug.Log("Door is locked. You need a key!");
            }
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
