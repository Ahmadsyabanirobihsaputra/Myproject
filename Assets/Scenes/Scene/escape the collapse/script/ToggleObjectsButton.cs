using UnityEngine;

public class ToggleObjectsButton : MonoBehaviour
{
    [Header("Objects to Toggle")]
    public GameObject objectA; // The first object
    public GameObject objectB; // The second object

    private bool isATurn = true; // Tracks which one is active

    // Call this method from your Button's OnClick event
    public void ToggleObjects()
    {
        if (objectA == null || objectB == null)
        {
            Debug.LogWarning("⚠️ Please assign both GameObjects in the Inspector!");
            return;
        }

        if (isATurn)
        {
            // Deactivate A, Activate B
            objectA.SetActive(false);
            objectB.SetActive(true);
        }
        else
        {
            // Activate A, Deactivate B
            objectA.SetActive(true);
            objectB.SetActive(false);
        }

        // Flip the state for next click
        isATurn = !isATurn;
    }
}
