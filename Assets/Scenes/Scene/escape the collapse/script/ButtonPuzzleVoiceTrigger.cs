using System.Collections.Generic;

using UnityEngine;

public class ButtonPuzzleVoiceTrigger : MonoBehaviour
{
    // =========================================================
    // PLAYER SETTINGS
    // =========================================================

    [Header("Player Settings")]

    [Tooltip("Player GameObject used to check interaction distance.")]
    public GameObject player;

    [Tooltip("Maximum distance required to activate a puzzle button.")]
    public float interactRange = 3f;


    // =========================================================
    // PUZZLE BUTTONS
    // =========================================================

    [Header("Puzzle Buttons")]

    [Tooltip("Multiple puzzle button GameObjects that can be controlled.")]
    public GameObject[] puzzleButtonObjects;


    // =========================================================
    // BUTTON DATA
    // =========================================================

    [System.Serializable]
    public class PuzzleButtonData
    {
        [Header("Button GameObject")]
        public GameObject buttonObject;

        [Header("Button Component")]
        public ButtonPuzzleButton puzzleButton;

        [Header("Voice Command")]
        public string buttonName;

        [Header("Debug")]
        public bool playerInRange;
        public float currentDistance;
    }


    [Tooltip("Automatically generated button information.")]
    public List<PuzzleButtonData> buttons =
        new List<PuzzleButtonData>();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();
        SetupPuzzleButtons();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CheckAllPlayerDistances();
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
                "[ButtonPuzzleVoiceTrigger] Player found: " +
                player.name
            );
        }
        else
        {
            Debug.LogWarning(
                "[ButtonPuzzleVoiceTrigger] " +
                "Player with tag 'Player' not found!"
            );
        }
    }


    // =========================================================
    // SETUP PUZZLE BUTTONS
    // =========================================================

    private void SetupPuzzleButtons()
    {
        buttons.Clear();

        if (puzzleButtonObjects == null ||
            puzzleButtonObjects.Length == 0)
        {
            Debug.LogWarning(
                "[ButtonPuzzleVoiceTrigger] " +
                "No puzzle button GameObjects assigned!"
            );

            return;
        }


        foreach (GameObject buttonObject in puzzleButtonObjects)
        {
            if (buttonObject == null)
                continue;


            ButtonPuzzleButton buttonComponent =
                buttonObject.GetComponent<ButtonPuzzleButton>();


            if (buttonComponent == null)
            {
                Debug.LogWarning(
                    "[ButtonPuzzleVoiceTrigger] " +
                    "ButtonPuzzleButton component not found on: " +
                    buttonObject.name
                );

                continue;
            }


            PuzzleButtonData data =
                new PuzzleButtonData();

            data.buttonObject = buttonObject;
            data.puzzleButton = buttonComponent;


            // -------------------------------------------------
            // GET BUTTON NAME
            // -------------------------------------------------

            if (!string.IsNullOrEmpty(
                buttonComponent.buttonName))
            {
                data.buttonName =
                    buttonComponent.buttonName;
            }
            else
            {
                data.buttonName =
                    buttonObject.name;
            }


            buttons.Add(data);


            Debug.Log(
                "[ButtonPuzzleVoiceTrigger] Connected: " +
                data.buttonName +
                " -> " +
                buttonObject.name
            );
        }


        Debug.Log(
            "[ButtonPuzzleVoiceTrigger] Total buttons connected: " +
            buttons.Count
        );
    }


    // =========================================================
    // DISTANCE CHECK
    // =========================================================

    private void CheckAllPlayerDistances()
    {
        if (player == null)
            return;


        foreach (PuzzleButtonData data in buttons)
        {
            if (data.puzzleButton == null)
            {
                data.playerInRange = false;
                data.currentDistance = Mathf.Infinity;

                continue;
            }


            data.currentDistance =
                Vector3.Distance(
                    player.transform.position,
                    data.puzzleButton.transform.position
                );


            data.playerInRange =
                data.currentDistance <= interactRange;
        }
    }


    // =========================================================
    // INTERACT WITH BUTTON
    // =========================================================

    private void InteractWithButton(
        PuzzleButtonData data)
    {
        if (data == null)
            return;


        if (data.puzzleButton == null)
            return;


        if (player == null)
        {
            Debug.LogWarning(
                "[ButtonPuzzleVoiceTrigger] " +
                "Player is missing!"
            );

            return;
        }


        // -----------------------------------------------------
        // CHECK DISTANCE
        // -----------------------------------------------------

        float distance =
            Vector3.Distance(
                player.transform.position,
                data.puzzleButton.transform.position
            );


        if (distance > interactRange)
        {
            Debug.Log(
                "[ButtonPuzzleVoiceTrigger] " +
                data.buttonName +
                " is too far away. Distance: " +
                distance.ToString("F2")
            );

            return;
        }


        // -----------------------------------------------------
        // PRESS BUTTON
        // -----------------------------------------------------

        Debug.Log(
            "[ButtonPuzzleVoiceTrigger] " +
            "Touch pressed button: " +
            data.buttonName
        );


        // Same interaction used by E
        data.puzzleButton.Interact();
    }


    // =========================================================
    // TOUCH COMMAND
    // =========================================================
    //
    // Voice command:
    //
    // "touch"
    //
    // The script searches for the closest button
    // that the player is currently within range of.
    // =========================================================

    public void VoiceTouch()
    {
        if (player == null)
        {
            Debug.LogWarning(
                "[ButtonPuzzleVoiceTrigger] " +
                "Player is missing!"
            );

            return;
        }


        PuzzleButtonData closestButton = null;

        float closestDistance =
            Mathf.Infinity;


        // -----------------------------------------------------
        // SEARCH ALL BUTTONS
        // -----------------------------------------------------

        foreach (PuzzleButtonData data in buttons)
        {
            if (data.puzzleButton == null)
                continue;


            float distance =
                Vector3.Distance(
                    player.transform.position,
                    data.puzzleButton.transform.position
                );


            // -------------------------------------------------
            // ONLY ACCEPT BUTTONS IN RANGE
            // -------------------------------------------------

            if (distance <= interactRange)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestButton = data;
                }
            }
        }


        // -----------------------------------------------------
        // BUTTON FOUND
        // -----------------------------------------------------

        if (closestButton != null)
        {
            Debug.Log(
                "[ButtonPuzzleVoiceTrigger] " +
                "Voice command 'touch' detected."
            );


            Debug.Log(
                "[ButtonPuzzleVoiceTrigger] " +
                "Closest button: " +
                closestButton.buttonName +
                " | Distance: " +
                closestDistance.ToString("F2")
            );


            InteractWithButton(closestButton);

            return;
        }


        // -----------------------------------------------------
        // NO BUTTON IN RANGE
        // -----------------------------------------------------

        Debug.Log(
            "[ButtonPuzzleVoiceTrigger] " +
            "Voice command 'touch' detected, " +
            "but no puzzle button is within range."
        );
    }


    // =========================================================
    // VOICE INTERACTION BY NAME
    // =========================================================

    public void VoiceInteract(string voiceButtonName)
    {
        if (string.IsNullOrEmpty(voiceButtonName))
        {
            Debug.LogWarning(
                "[ButtonPuzzleVoiceTrigger] " +
                "Voice button name is empty!"
            );

            return;
        }


        // =====================================================
        // TOUCH COMMAND
        // =====================================================

        if (voiceButtonName.Trim().Equals(
            "touch",
            System.StringComparison.OrdinalIgnoreCase))
        {
            VoiceTouch();
            return;
        }


        // =====================================================
        // SPECIFIC BUTTON COMMAND
        // =====================================================

        foreach (PuzzleButtonData data in buttons)
        {
            if (data.puzzleButton == null)
                continue;


            if (!string.Equals(
                data.buttonName.Trim(),
                voiceButtonName.Trim(),
                System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            InteractWithButton(data);

            return;
        }


        // =====================================================
        // NO BUTTON FOUND
        // =====================================================

        Debug.LogWarning(
            "[ButtonPuzzleVoiceTrigger] " +
            "No button found with voice name: " +
            voiceButtonName
        );
    }


    // =========================================================
    // SPECIFIC VOICE COMMANDS
    // =========================================================

    public void VoiceSquare()
    {
        VoiceInteract("Square");
    }


    public void VoiceCircle()
    {
        VoiceInteract("Circle");
    }


    public void VoiceTriangle()
    {
        VoiceInteract("Triangle");
    }


    public void VoiceStar()
    {
        VoiceInteract("Star");
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


        if (puzzleButtonObjects == null)
            return;


        foreach (GameObject buttonObject in puzzleButtonObjects)
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