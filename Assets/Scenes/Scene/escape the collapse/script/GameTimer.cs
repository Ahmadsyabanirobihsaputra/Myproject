using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Pilih waktu yang akan dipilih secara acak setiap kali scene dimulai")]
    public float[] possibleTimes = { 30f, 45f, 60f }; // daftar waktu yang mungkin
    public string nextSceneName = "NextScene"; // nama scene tujuan

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    [Header("Audio Settings")]
    public AudioClip warningSound;        // Suara peringatan
    public float warningTime = 10f;       // Mulai mainkan suara saat sisa waktu ini
    public bool loopWarningSound = false; // Apakah suara berulang?

    [Header("Color Settings")]
    [Tooltip("Warna saat waktu normal (di atas threshold pertama)")]
    public Color normalColor = Color.white;

    [Tooltip("Warna saat waktu mulai menurun (antara threshold1 dan threshold2)")]
    public Color mediumColor = Color.yellow;

    [Tooltip("Warna saat waktu hampir habis (di bawah threshold2)")]
    public Color lowColor = Color.red;

    [Range(0f, 1f)]
    [Tooltip("Persentase sisa waktu untuk mulai warna medium (misal 0.5 = 50%)")]
    public float mediumThreshold = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Persentase sisa waktu untuk mulai warna low (misal 0.25 = 25%)")]
    public float lowThreshold = 0.25f;

    private AudioSource audioSource;
    private float currentTime;
    private float startTime;
    private bool timerRunning = true;
    private bool warningPlayed = false;

    void Start()
    {
        // 🔹 Pilih waktu secara acak dari daftar yang diset di Inspector
        if (possibleTimes != null && possibleTimes.Length > 0)
        {
            int randomIndex = Random.Range(0, possibleTimes.Length);
            currentTime = possibleTimes[randomIndex];
        }
        else
        {
            currentTime = 60f;
            Debug.LogWarning("[GameTimer] No possible times set! Using default 60 seconds.");
        }

        startTime = currentTime;
        audioSource = GetComponent<AudioSource>();
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= warningTime && !warningPlayed && warningSound != null)
        {
            PlayWarningSound();
        }

        if (currentTime <= 0)
        {
            currentTime = 0;
            timerRunning = false;
            OnTimerEnd();
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        UpdateTimerColor();
    }

    void UpdateTimerColor()
    {
        float percent = currentTime / startTime;

        if (percent <= lowThreshold)
            timerText.color = lowColor;
        else if (percent <= mediumThreshold)
            timerText.color = mediumColor;
        else
            timerText.color = normalColor;
    }

    void PlayWarningSound()
    {
        warningPlayed = true;
        audioSource.clip = warningSound;
        audioSource.loop = loopWarningSound;
        audioSource.Play();
    }

    void OnTimerEnd()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        SceneManager.LoadScene(nextSceneName);
    }
}
