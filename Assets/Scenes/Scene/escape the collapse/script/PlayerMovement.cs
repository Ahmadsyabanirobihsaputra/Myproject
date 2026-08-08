using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementWithPauseUI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Ground Detection Settings")]
    public LayerMask groundLayer;          // Which layer counts as "ground"
    public float groundCheckDistance = 1.1f; // Adjustable raycast distance

    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("UI Settings")]
    public GameObject pauseMenuCanvas;

    private float xRotation = 0f;
    private bool mouseControlEnabled = false;
    private bool isPaused = false;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float defaultSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // prevent unwanted rotation from physics
        defaultSpeed = moveSpeed; // save original speed
    }

    void Start()
    {
        if (playerCamera == null)
            Debug.LogWarning("Player camera belum di-assign!");

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
    }

    void Update()
    {
        if (!mouseControlEnabled)
            return;

        // Pause game
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (isPaused)
            return;

        RotatePlayer();
        CheckGround();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        if (!isPaused && mouseControlEnabled)
            MovePlayer();
    }

 
void MovePlayer()
    {
        // A/D movement
        float horizontal = Input.GetAxis("Horizontal");

        // Normal keyboard vertical input
        float vertical = Input.GetAxis("Vertical");

        // ---------------------------------------------------------
        // FORWARD INPUT
        // ---------------------------------------------------------
        //
        // Moving.IsForwardPressed() returns TRUE when:
        //
        // 1. Keyboard W is pressed
        // 2. UI W button is pressed
        // 3. Voice command activated continuous forward
        //
        // If any of those are active, force forward movement.
        // ---------------------------------------------------------

        if (Moving.IsForwardPressed())
        {
            vertical = 1f;
        }

        // ---------------------------------------------------------
        // CREATE MOVEMENT
        // ---------------------------------------------------------

        Vector3 move =
            (transform.right * horizontal +
             transform.forward * vertical).normalized;

        // ---------------------------------------------------------
        // MOVE PLAYER
        // ---------------------------------------------------------

        Vector3 targetPosition =
            rb.position +
            move * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(targetPosition);
    }



    void RotatePlayer()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void CheckGround()
    {
        // Cast a ray down from the bottom of the player
        Vector3 rayOrigin = transform.position;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            LockCursor(false);
            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            LockCursor(true);
            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
        }
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void SetFPSControl(bool enabled)
    {
        mouseControlEnabled = enabled;

        if (enabled)
        {
            LockCursor(true);
            Time.timeScale = 1f;
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.SetActive(false);
        }
        else
        {
            LockCursor(false);
            Time.timeScale = 1f;
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.SetActive(false);
        }
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = defaultSpeed;
    }

    // Debugging helper: visualize ray in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
