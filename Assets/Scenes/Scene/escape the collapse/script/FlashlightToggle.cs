using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public GameObject flashlightObject;   // The flashlight GameObject to enable/disable
    public KeyCode toggleKey = KeyCode.F; // The key to toggle the flashlight

    private bool isOn = false;

    void Start()
    {
        if (flashlightObject == null)
        {
            Debug.LogWarning("Flashlight object not assigned! Please drag your flashlight GameObject here.");
            return;
        }

        // Make sure flashlight starts off
        flashlightObject.SetActive(isOn);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        isOn = !isOn;
        flashlightObject.SetActive(isOn);
    }
}
