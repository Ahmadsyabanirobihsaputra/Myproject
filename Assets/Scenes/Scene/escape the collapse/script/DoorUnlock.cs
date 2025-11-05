using UnityEngine;

public class DoorUnlock : MonoBehaviour
{
    public Animator doorAnimator;
    public bool hasKey = false;
    private bool doorOpened = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (hasKey && !doorOpened)
            {
                doorAnimator.SetBool("isOpen", true);
                doorOpened = true;
            }
            else if (!hasKey)
            {
                Debug.Log("Door is locked. You need a key!");
            }
        }
    }
}
