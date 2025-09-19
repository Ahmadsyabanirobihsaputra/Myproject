using UnityEngine;
using UnityEngine.UI;


public class ShootingRangeSniper : MonoBehaviour
{
    [Header("Crosshair")]
    public Transform crosshairObject;
    public Transform focusOverlay;
    public GameObject ironSight;

    [Header("Mobile Controls")]
    public DynamicJoystick aimJoystick;
    public Button shootButton;
    public float aimSpeed = 5f;

    [Header("Target Settings")]
    public LayerMask targetLayer;
    [HideInInspector] public TargetPart requiredTargetHead;

    [Header("Effects")]
    public GameObject hitEffect;

    [Header("UI & Score")]
    public Text scoreText;
    public Text highScoreText;
    public Text timerText;
    public GameObject gameOverScreen;
    public float resultDelay = 1f;

    [Header("Timer Settings")]
    public float startTime = 30f;
    public float bonusTimeOnHit = 3f;
    private float currentTime;
    private bool timerRunning = true;

    [Header("Crosshair Depth")]
    public bool use2DMode = true;    // if true, crosshair stays on z = 0 world plane
    public float crosshairDepth = 10f; // distance from camera when not in 2D mode

    [Header("Target Randomizer")]
    public ShootingRangeRandomizer randomizer;

    private int score = 0;
    private int highScore = 0;

    private void Start()
    {
        if (shootButton != null)
            shootButton.onClick.AddListener(Shoot);

        currentTime = startTime;
        UpdateTimerUI();

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateScoreUI();
    }

    private void Update()
    {
        UpdateCrosshairWithJoystick();
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            timerRunning = false;
            EndGame();
        }
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    private void UpdateCrosshairWithJoystick()
    {
        if (aimJoystick == null || crosshairObject == null) return;
        if (Camera.main == null) return;

        // Move crosshair based on joystick
        Vector3 move = new Vector3(aimJoystick.Horizontal, aimJoystick.Vertical, 0f) * aimSpeed * Time.deltaTime;
        crosshairObject.position += move;

        // Convert to viewport, clamp, then convert back preserving desired depth
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(crosshairObject.position);
        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);

        // z for ViewportToWorldPoint is camera-to-world distance.
        if (use2DMode)
        {
            // Keep crosshair on world z = 0 plane (works for typical 2D camera at z = -camZ)
            viewportPos.z = Mathf.Abs(Camera.main.transform.position.z);
        }
        else
        {
            // Use explicit depth from camera (editable)
            viewportPos.z = crosshairDepth;
        }

        crosshairObject.position = Camera.main.ViewportToWorldPoint(viewportPos);

        // Sync overlays
        SetCrosshair(crosshairObject.position);
    }

    private void SetCrosshair(Vector3 pos)
    {
        if (crosshairObject != null) crosshairObject.position = pos;
        if (focusOverlay != null) focusOverlay.position = pos;
        if (ironSight != null) ironSight.transform.position = pos;
    }

    private void Shoot()
    {
        if (!timerRunning) return;

        Vector2 crosshairWorldPos = crosshairObject.position;
        Collider2D hit = Physics2D.OverlapPoint(crosshairWorldPos, targetLayer);

        if (hit != null)
        {
            TargetPart part = hit.GetComponent<TargetPart>();
            if (part != null)
            {
                bool isClueTarget = (requiredTargetHead != null && part == requiredTargetHead);
                int points = isClueTarget ? 20 : 10;

                score += points;
                UpdateScoreUI();

                currentTime += bonusTimeOnHit;

                if (hitEffect != null)
                    Instantiate(hitEffect, crosshairWorldPos, Quaternion.identity);

                part.OnHit(isClueTarget);

                //  Register hit in FiringRangeManager
                if (FiringRangeManager.Instance != null)
                    FiringRangeManager.Instance.RegisterShot(true);

                // Notify randomizer so it can swap targets & pick a new clue
                if (randomizer != null)
                    randomizer.OnTargetHit(part);
            }
            else
            {
                //  Register as a miss (hit something with no TargetPart)
                if (FiringRangeManager.Instance != null)
                    FiringRangeManager.Instance.RegisterShot(false);
            }
        }
        else
        {
            //  Register a miss (nothing hit at all)
            if (FiringRangeManager.Instance != null)
                FiringRangeManager.Instance.RegisterShot(false);
        }
    }


    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (highScoreText != null) highScoreText.text = $"High Score: {highScore}";
    }

    private void EndGame()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        UpdateScoreUI();
    }
}
