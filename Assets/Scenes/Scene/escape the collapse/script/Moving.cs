

using UnityEngine;

public class Moving : MonoBehaviour
{
    // =========================================================
    // VIRTUAL W BUTTON
    // =========================================================

    // True while the UI W button is being held
    public static bool virtualWPressed = false;

    // True when continuous forward movement is active
    public static bool continuousForward = false;


    // =========================================================
    // UI BUTTON FUNCTIONS
    // =========================================================

    // Normal hold-to-move button
    public static void WButtonDown()
    {
        virtualWPressed = true;

        Debug.Log("[Moving] Virtual W: ON");
    }

    public static void WButtonUp()
    {
        virtualWPressed = false;

        Debug.Log("[Moving] Virtual W: OFF");
    }


    // =========================================================
    // TOGGLE CONTINUOUS FORWARD
    // =========================================================

    // Call this from your voice command.
    //
    // First trigger:
    // OFF -> ON
    //
    // Second trigger:
    // ON -> OFF
    //
    // Third trigger:
    // OFF -> ON
    //
    // etc.
    public static void ToggleForward()
    {
        continuousForward = !continuousForward;

        Debug.Log(
            "[Moving] Continuous Forward: " +
            (continuousForward ? "ON" : "OFF")
        );
    }


    // =========================================================
    // INPUT CHECKS
    // =========================================================

    // Keyboard W OR UI W button
    public static bool IsWPressed()
    {
        return Input.GetKey(KeyCode.W) || virtualWPressed;
    }


    // Anything that should make the player move forward
    public static bool IsForwardPressed()
    {
        return IsWPressed() || continuousForward;
    }
}

