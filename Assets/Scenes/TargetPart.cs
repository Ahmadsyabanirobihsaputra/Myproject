
using UnityEngine;

public class TargetPart : MonoBehaviour
{
    [Header("Target Settings")]
    public bool isHead = false;          // Check this in Inspector if this collider is the head
    [HideInInspector] public bool isCorrectTarget = false; // Set true only by TargetRandomizer

    [Header("On Hit Feedback")]
    public GameObject hitEnableObject;   // Texture/decal that appears on hit (disabled by default)

    // Called when shot hits this part
    public void OnHit(bool isWinningShot)
    {
        if (hitEnableObject != null)
            hitEnableObject.SetActive(true);

        Debug.Log($"{gameObject.name} was hit. Correct? {isCorrectTarget}, Winning shot? {isWinningShot}");
    }
}
