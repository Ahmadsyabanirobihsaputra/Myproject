using System.Collections.Generic;

using UnityEngine;

public class EnvironmentalPuzzlePress : MonoBehaviour
{
    // =========================================================
    // PLAYER SETTINGS
    // =========================================================

    [Header("Player Settings")]

    [Tooltip("Player GameObject used to check interaction distance.")]
    public GameObject player;

    [Tooltip("Maximum distance from the player to activate a button.")]
    public float interactRange = 3f;


    // =========================================================
    // ENVIRONMENTAL BUTTONS
    // =========================================================

    [Header("Environmental Puzzle Buttons")]

    [Tooltip(
        "Add all EnvironmentPuzzleButton GameObjects here."
    )]
    public GameObject[] environmentButtonObjects;


    // =========================================================
    // INTERNAL BUTTON LIST
    // =========================================================

    private List<EnvironmentPuzzleButton> buttons =
        new List<EnvironmentPuzzleButton>();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();

        FindButtons();
    }


    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindPlayer()
    {
        if (player != null)
            return;


        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");


        if (playerObject != null)
        {
            player = playerObject;


            Debug.Log(
                "[EnvironmentalPuzzlePress] " +
                "Player found: " +
                player.name
            );
        }
        else
        {
            Debug.LogWarning(
                "[EnvironmentalPuzzlePress] " +
                "Player with tag 'Player' not found!"
            );
        }
    }


    // =========================================================
    // FIND BUTTONS
    // =========================================================

    private void FindButtons()
    {
        buttons.Clear();


        if (environmentButtonObjects == null ||
            environmentButtonObjects.Length == 0)
        {
            Debug.LogWarning(
                "[EnvironmentalPuzzlePress] " +
                "No EnvironmentPuzzleButton objects assigned!"
            );

            return;
        }


        foreach (
            GameObject buttonObject
            in environmentButtonObjects)
        {
            if (buttonObject == null)
                continue;


            EnvironmentPuzzleButton button =
                buttonObject.GetComponent<
                    EnvironmentPuzzleButton
                >();


            if (button == null)
            {
                Debug.LogWarning(
                    "[EnvironmentalPuzzlePress] " +
                    "EnvironmentPuzzleButton not found on: " +
                    buttonObject.name
                );

                continue;
            }


            buttons.Add(button);


            Debug.Log(
                "[EnvironmentalPuzzlePress] " +
                "Connected: " +
                buttonObject.name
            );
        }


        Debug.Log(
            "[EnvironmentalPuzzlePress] " +
            "Total buttons: " +
            buttons.Count
        );
    }


    // =========================================================
    // TOUCH
    // =========================================================

    public void VoiceTouch()
    {
        if (player == null)
        {
            Debug.LogWarning(
                "[EnvironmentalPuzzlePress] " +
                "Player is missing!"
            );

            return;
        }


        EnvironmentPuzzleButton closestButton = null;

        float closestDistance =
            Mathf.Infinity;


        // -----------------------------------------------------
        // SEARCH BUTTONS
        // -----------------------------------------------------

        foreach (
            EnvironmentPuzzleButton button
            in buttons)
        {
            if (button == null)
                continue;


            // Ignore buttons that have already been pressed
            // if multiple presses are disabled.

            if (button.buttonPressed &&
                !button.canPressMultipleTimes)
            {
                continue;
            }


            float distance =
                Vector3.Distance(
                    player.transform.position,
                    button.transform.position
                );


            // -------------------------------------------------
            // CHECK RANGE
            // -------------------------------------------------

            if (distance <= interactRange)
            {
                if (distance < closestDistance)
                {
                    closestDistance =
                        distance;

                    closestButton =
                        button;
                }
            }
        }


        // =====================================================
        // PRESS BUTTON
        // =====================================================

        if (closestButton != null)
        {
            Debug.Log(
                "[EnvironmentalPuzzlePress] " +
                "Touch detected."
            );


            Debug.Log(
                "[EnvironmentalPuzzlePress] " +
                "Pressing: " +
                closestButton.buttonName +
                " | Distance: " +
                closestDistance.ToString("F2")
            );


            // Same interaction as pressing E.

            closestButton.Interact();


            return;
        }


        // =====================================================
        // NO BUTTON
        // =====================================================

        Debug.Log(
            "[EnvironmentalPuzzlePress] " +
            "Touch detected, but no environmental " +
            "button is within range."
        );
    }


    // =========================================================
    // VOICE INTERACTION
    // =========================================================

    public void VoiceInteract(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;


        if (command.Trim().Equals(
            "touch",
            System.StringComparison.OrdinalIgnoreCase))
        {
            VoiceTouch();

            return;
        }


        Debug.Log(
            "[EnvironmentalPuzzlePress] " +
            "Unknown voice command: " +
            command
        );
    }


    // =========================================================
    // UI BUTTON
    // =========================================================

    public void OnUIButtonPressed()
    {
        VoiceTouch();
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;


        if (environmentButtonObjects == null)
            return;


        foreach (
            GameObject buttonObject
            in environmentButtonObjects)
        {
            if (buttonObject == null)
                continue;


            Gizmos.DrawWireSphere(
                buttonObject.transform.position,
                interactRange
            );
        }
    }
}