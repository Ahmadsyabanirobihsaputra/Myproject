using System.Collections;

using UnityEngine;

public class MoveCanvasObject : MonoBehaviour
{
    // =========================================================
    // MOVEMENT DIRECTION
    // =========================================================

    public enum MovementDirection
    {
        Vertical,
        Horizontal
    }


    // =========================================================
    // TARGET OBJECT
    // =========================================================

    [System.Serializable]
    public class TargetObject
    {
        [Header("Target UI Object")]
        public RectTransform targetObject;

        [Header("Movement Direction")]
        public MovementDirection direction = MovementDirection.Vertical;

        [Header("Movement Settings")]
        [Tooltip("How far the UI object moves from its original position.")]
        public float moveDistance = 300f;

        [Tooltip("How long the movement takes in seconds.")]
        public float moveDuration = 0.5f;

        [Header("Direction")]
        [Tooltip(
            "Vertical: -1 = Down, 1 = Up\n" +
            "Horizontal: -1 = Left, 1 = Right"
        )]
        [Range(-1, 1)]
        public int directionSign = -1;

        // Original position
        [HideInInspector]
        public Vector2 originalPosition;

        // Position after moving
        [HideInInspector]
        public Vector2 movedPosition;
    }


    // =========================================================
    // TARGETS
    // =========================================================

    [Header("UI Objects")]
    [Tooltip("Add all UI objects that you want to move.")]
    public TargetObject[] targets;


    // =========================================================
    // STARTING STATE
    // =========================================================

    [Header("Starting State")]
    [Tooltip("If enabled, UI objects start in the moved position.")]
    public bool startMoved = false;


    // =========================================================
    // KEYBOARD TEST
    // =========================================================

    [Header("Keyboard Test")]
    [Tooltip("Press this key to toggle the UI.")]
    public KeyCode testKey = KeyCode.T;


    // =========================================================
    // STATE
    // =========================================================

    private bool isMoved = false;

    private Coroutine movementCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning(
                "[MoveCanvasObject] No targets assigned!"
            );

            return;
        }


        foreach (TargetObject target in targets)
        {
            if (target.targetObject == null)
            {
                Debug.LogWarning(
                    "[MoveCanvasObject] Target UI object is missing!"
                );

                continue;
            }


            // -------------------------------------------------
            // SAVE ORIGINAL POSITION
            // -------------------------------------------------

            target.originalPosition =
                target.targetObject.anchoredPosition;


            // -------------------------------------------------
            // DETERMINE MOVEMENT DIRECTION
            // -------------------------------------------------

            int sign =
                target.directionSign >= 0
                    ? 1
                    : -1;


            Vector2 movement = Vector2.zero;


            if (target.direction == MovementDirection.Vertical)
            {
                movement =
                    Vector2.up *
                    target.moveDistance *
                    sign;
            }
            else
            {
                movement =
                    Vector2.right *
                    target.moveDistance *
                    sign;
            }


            // -------------------------------------------------
            // CALCULATE MOVED POSITION
            // -------------------------------------------------

            target.movedPosition =
                target.originalPosition + movement;


            // -------------------------------------------------
            // SET STARTING STATE
            // -------------------------------------------------

            if (startMoved)
            {
                target.targetObject.anchoredPosition =
                    target.movedPosition;
            }
            else
            {
                target.targetObject.anchoredPosition =
                    target.originalPosition;
            }
        }


        isMoved = startMoved;


        Debug.Log(
            "[MoveCanvasObject] READY. State = " +
            (isMoved ? "MOVED" : "ORIGINAL")
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log(
                "[MoveCanvasObject] Keyboard pressed: " +
                testKey
            );

            ToggleMovement();
        }
    }


    // =========================================================
    // TOGGLE MOVEMENT
    // =========================================================

    public void ToggleMovement()
    {
        isMoved = !isMoved;

        StartMovement();

        Debug.Log(
            "[MoveCanvasObject] ToggleMovement -> " +
            (isMoved
                ? "UI MOVING OUT"
                : "UI RETURNING TO ORIGINAL POSITION")
        );
    }


    // =========================================================
    // START MOVEMENT
    // =========================================================

    private void StartMovement()
    {
        // Stop previous movement if the player
        // activates the command again while moving.
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine =
            StartCoroutine(AnimateUI());
    }


    // =========================================================
    // UI ANIMATION
    // =========================================================

    private IEnumerator AnimateUI()
    {
        if (targets == null)
            yield break;


        // Find the longest duration among targets.
        float maxDuration = 0f;

        foreach (TargetObject target in targets)
        {
            if (target.targetObject == null)
                continue;

            if (target.moveDuration > maxDuration)
                maxDuration = target.moveDuration;
        }


        if (maxDuration <= 0f)
        {
            // Safety fallback.
            foreach (TargetObject target in targets)
            {
                if (target.targetObject == null)
                    continue;

                target.targetObject.anchoredPosition =
                    isMoved
                        ? target.movedPosition
                        : target.originalPosition;
            }

            yield break;
        }


        // Save the position at the moment animation starts.
        Vector2[] startPositions =
            new Vector2[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i].targetObject != null)
            {
                startPositions[i] =
                    targets[i].targetObject.anchoredPosition;
            }
        }


        float elapsed = 0f;


        while (elapsed < maxDuration)
        {
            elapsed += Time.unscaledDeltaTime;


            for (int i = 0; i < targets.Length; i++)
            {
                TargetObject target = targets[i];

                if (target.targetObject == null)
                    continue;


                // Each object can have its own duration.
                float duration =
                    Mathf.Max(target.moveDuration, 0.01f);


                float t =
                    Mathf.Clamp01(elapsed / duration);


                // Smooth movement.
                t = Mathf.SmoothStep(0f, 1f, t);


                Vector2 destination =
                    isMoved
                        ? target.movedPosition
                        : target.originalPosition;


                target.targetObject.anchoredPosition =
                    Vector2.Lerp(
                        startPositions[i],
                        destination,
                        t
                    );
            }


            yield return null;
        }


        // -----------------------------------------------------
        // IMPORTANT:
        // Force the exact final position.
        // -----------------------------------------------------

        foreach (TargetObject target in targets)
        {
            if (target.targetObject == null)
                continue;


            target.targetObject.anchoredPosition =
                isMoved
                    ? target.movedPosition
                    : target.originalPosition;
        }


        movementCoroutine = null;
    }


    // =========================================================
    // MOVE OUT
    // =========================================================

    public void MoveOut()
    {
        isMoved = true;

        StartMovement();

        Debug.Log(
            "[MoveCanvasObject] UI MOVING OUT."
        );
    }


    // =========================================================
    // MOVE BACK
    // =========================================================

    public void MoveBack()
    {
        isMoved = false;

        StartMovement();

        Debug.Log(
            "[MoveCanvasObject] UI RETURNING TO ORIGINAL POSITION."
        );
    }


    // =========================================================
    // FORCE RESET
    // =========================================================

    public void ResetToOriginal()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }


        isMoved = false;


        if (targets == null)
            return;


        foreach (TargetObject target in targets)
        {
            if (target.targetObject == null)
                continue;


            target.targetObject.anchoredPosition =
                target.originalPosition;
        }


        Debug.Log(
            "[MoveCanvasObject] UI RESET TO ORIGINAL POSITION."
        );
    }


    // =========================================================
    // DEBUG
    // =========================================================

    public bool IsMoved()
    {
        return isMoved;
    }
}

