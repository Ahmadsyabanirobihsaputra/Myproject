
using UnityEngine;
using UnityEngine.UI;


public class WorldCrosshairSniper : MonoBehaviour
{
    [Header("Crosshair")]
    public Transform crosshairObject;
    public Transform focusOverlay;
    public GameObject ironSight;

    [Header("Mobile Controls")]
    public DynamicJoystick aimJoystick;
    public Button shootButton;   // UI button to shoot
    public float aimSpeed = 5f;

    [Header("Target Settings")]
    public LayerMask targetLayer;
    [HideInInspector] public TargetPart requiredTargetHead; // Assigned by TargetRandomizer

    [Header("Effects")]
    public GameObject headExplosionEffect;

    [Header("UI Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public float resultDelay = 1.5f;

    [Header("Crosshair Settings")]
    public float crosshairDepth = 10f; // For 3D (ignored in 2D mode)
    public bool use2DMode = true;      // Keep z=0 for 2D

    [Header("Timer Settings")]
    public float roundTime = 30f;          // total time for the round
    public Text timerText;                 // UI text to display countdown
    private float currentTime;
    private bool timerRunning = true;

    private bool shotTaken = false;

    void Start()
    {
        // Hook up the button for shooting
        if (shootButton != null)
            shootButton.onClick.AddListener(MobileShoot);

        if (crosshairObject == null)
            Debug.LogError("CrosshairObject is not assigned!");

        if (aimJoystick == null)
            Debug.LogWarning("Joystick not assigned. Crosshair won't move.");

        // init timer
        currentTime = roundTime;
        UpdateTimerUI();
    }

    void Update()
    {
        UpdateCrosshairWithJoystick();
        UpdateTimer();
    }

    void UpdateTimer()
    {
        if (!timerRunning || shotTaken) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            timerRunning = false;
            TimeOut();
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    void TimeOut()
    {
        Debug.Log("Time's up! Game Over");
        StartCoroutine(DelayedResult(false)); // lose
        shotTaken = true; // prevent further shooting
        DisableVisuals();
    }

    void UpdateCrosshairWithJoystick()
    {
        if (aimJoystick == null || crosshairObject == null) return;

        // Move crosshair based on joystick
        Vector3 move = new Vector3(aimJoystick.Horizontal, aimJoystick.Vertical, 0f)
                       * aimSpeed * Time.deltaTime;
        crosshairObject.position += move;

        // Clamp position inside camera view
        Vector3 clampedPos = Camera.main.WorldToViewportPoint(crosshairObject.position);
        clampedPos.x = Mathf.Clamp01(clampedPos.x);
        clampedPos.y = Mathf.Clamp01(clampedPos.y);

        if (use2DMode)
            clampedPos.z = Mathf.Abs(Camera.main.transform.position.z); // keep 2D plane
        else
            clampedPos.z = crosshairDepth; // adjustable for 3D

        crosshairObject.position = Camera.main.ViewportToWorldPoint(clampedPos);

        // Sync overlays
        SetCrosshair(crosshairObject.position);
    }

    void SetCrosshair(Vector3 pos)
    {
        if (crosshairObject != null)
            crosshairObject.position = pos;

        if (focusOverlay != null)
            focusOverlay.position = pos;

        if (ironSight != null)
            ironSight.transform.position = pos;
    }

    public void MobileShoot()
    {
        if (!shotTaken)
        {
            shotTaken = true;
            HandleShot();
            DisableVisuals();
        }
    }

    void HandleShot()
    {
        if (crosshairObject == null)
        {
            Debug.LogError("CrosshairObject not set!");
            return;
        }

        if (requiredTargetHead == null)
        {
            Debug.LogError("Required target head not assigned! Did TargetRandomizer run?");
            return;
        }

        Vector2 crosshairWorldPos = crosshairObject.position;
        Collider2D hit = Physics2D.OverlapPoint(crosshairWorldPos, targetLayer);

        if (hit != null)
        {
            TargetPart part = hit.GetComponent<TargetPart>();
            if (part != null)
            {
                bool isWin = (part == requiredTargetHead);
                part.OnHit(isWin);

                if (isWin)
                {
                    if (StreakManager.Instance != null)
                        StreakManager.Instance.AddWin();

                    TriggerExplosion(crosshairWorldPos);
                    StartCoroutine(DelayedResult(true));
                    return;
                }
            }
        }

        // if miss or wrong reset streak
        if (StreakManager.Instance != null)
            StreakManager.Instance.ResetStreak();

        StartCoroutine(DelayedResult(false));
    }

    void TriggerExplosion(Vector2 position)
    {
        if (headExplosionEffect != null)
            Instantiate(headExplosionEffect, position, Quaternion.identity);
    }

    System.Collections.IEnumerator DelayedResult(bool isWin)
    {
        yield return new WaitForSeconds(resultDelay);

        if (isWin && winScreen != null)
            winScreen.SetActive(true);
        else if (!isWin && loseScreen != null)
            loseScreen.SetActive(true);
    }

    void DisableVisuals()
    {
        if (crosshairObject != null)
            crosshairObject.gameObject.SetActive(false);

        if (focusOverlay != null)
            focusOverlay.gameObject.SetActive(false);

        if (ironSight != null)
            ironSight.SetActive(false);
    }
}
