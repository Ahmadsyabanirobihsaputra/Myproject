using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Animator doorAnimator;
    private bool isOpen = false;

    public void TryOpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            doorAnimator.SetTrigger("OpenDoor");
        }
    }
}
