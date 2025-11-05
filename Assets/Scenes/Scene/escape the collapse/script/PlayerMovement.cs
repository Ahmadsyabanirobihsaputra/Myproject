using UnityEngine;

public class PlayerMovementWithPauseUI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("UI Settings")]
    public GameObject pauseMenuCanvas;

    private float xRotation = 0f;
    private bool mouseControlEnabled = false;
    private bool isPaused = false;

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

        // pause game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused)
            return;

        MovePlayer();
        RotatePlayer();
    }

    void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        transform.position += move * moveSpeed * Time.deltaTime;
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

    // 🔹 Fungsi tambahan untuk dikontrol oleh PlayerSceneManager
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
    private float defaultSpeed;

    void Awake()
    {
        defaultSpeed = moveSpeed; // Simpan kecepatan awal
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = defaultSpeed;
    }

}
