
using UnityEngine;

public class MouseTargetDebugger : MonoBehaviour
{
    [Header("Target Settings")]
    public LayerMask targetLayer;           // Layer for valid targets
    public GameObject requiredTargetHead;   // Specific target to check against

    void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, targetLayer);

        if (hit != null)
        {
            bool isCorrectTarget = hit.gameObject == requiredTargetHead;
            Debug.Log("Mouse over target: " + hit.name + " | Is correct head: " + isCorrectTarget);
        }
        else
        {
            Debug.Log("Mouse not over any target.");
        }
    }
}