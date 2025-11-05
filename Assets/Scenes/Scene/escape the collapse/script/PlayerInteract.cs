using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask interactableLayers = ~0; // default: everything

    [Header("UI Settings")]
    [Tooltip("UI Text untuk menampilkan prompt seperti 'Press E to pick up key'")]
    public Text promptText;
    [Tooltip("UI Text untuk menampilkan pesan seperti 'Key obtained!'")]
    public Text messageText;

    [Header("Debug")]
    public bool showDebugRay = true;
    public bool logDetails = true;

    private IInteractable currentInteractable;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null && logDetails)
                Debug.LogWarning("[PlayerInteract] playerCamera not assigned and Camera.main is null.");
        }

        if (promptText != null)
            promptText.text = "";
        if (messageText != null)
            messageText.text = "";
    }

    void Update()
    {
        // Debug visual ray
        if (showDebugRay && playerCamera != null)
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.yellow);

        // Update objek interaktif di depan pemain
        HandleInteractionRay();

        // Tekan tombol interaksi
        if (Input.GetKeyDown(interactKey))
            TryInteract();
    }

    void HandleInteractionRay()
    {
        if (playerCamera == null) return;

        Vector3 origin = playerCamera.transform.position;
        Vector3 dir = playerCamera.transform.forward;
        RaycastHit hit;

        bool didHit = Physics.Raycast(origin, dir, out hit, interactDistance, interactableLayers, QueryTriggerInteraction.Collide);

        if (didHit)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>()
                ?? hit.collider.GetComponentInParent<IInteractable>()
                ?? hit.collider.GetComponentInChildren<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                if (promptText != null)
                    promptText.text = interactable.GetPrompt();
                return;
            }
        }

        // Tidak ada objek interaktif yang dilihat
        currentInteractable = null;
        if (promptText != null)
            promptText.text = "";
    }

    void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[PlayerInteract] No playerCamera assigned.");
            return;
        }

        if (currentInteractable != null)
        {
            if (logDetails)
                Debug.Log($"[PlayerInteract] Interacting with {(currentInteractable as MonoBehaviour)?.name}");

            currentInteractable.Interact();

            // Pesan jika pemain baru saja mendapat kunci
            if (KeyInteract.hasKey && messageText != null)
            {
                messageText.text = "Key obtained!";
                CancelInvoke(nameof(ClearMessage));
                Invoke(nameof(ClearMessage), 2f);
            }
        }
        else if (logDetails)
        {
            Debug.Log("[PlayerInteract] Tried to interact but no interactable detected.");
        }
    }

    public void InteractFromButton()
    {
        // Fungsi ini bisa dipanggil dari UI Button
        TryInteract();
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugRay || playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCamera.transform.position, playerCamera.transform.position + playerCamera.transform.forward * interactDistance);
        Gizmos.DrawWireSphere(playerCamera.transform.position + playerCamera.transform.forward * interactDistance, 0.05f);
    }
}
