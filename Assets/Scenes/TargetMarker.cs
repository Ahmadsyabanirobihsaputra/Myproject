using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    [Header("Target Parts")]
    public TargetPart head; // Assign the head collider/part in the Inspector

    [Header("Clue Settings")]
    [TextArea]
    public string clue; // Unique clue for THIS stickman
}
