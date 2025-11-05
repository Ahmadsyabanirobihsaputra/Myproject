using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class RisingWater : MonoBehaviour
{
    [Header("Water Movement Settings")]
    [Tooltip("Ketinggian ombak maksimum (amplitudo)")]
    public float waveAmplitude = 0.5f;
    [Tooltip("Kecepatan gerakan ombak (frekuensi gelombang)")]
    public float waveSpeed = 1f;
    [Tooltip("Tinggi dasar posisi air")]
    public float baseHeight = 0f;

    [Header("Player Settings")]
    public GameObject playerObject; // Drag Player ke sini
    public float slowSpeed = 2f;    // Kecepatan pemain di air

    [Header("Audio Settings")]
    public AudioClip waterSound;
    [Range(0.1f, 3f)]
    public float fadeSpeed = 1f;    // Kecepatan fade suara

    private float timeCounter;
    private bool playerInside = false;
    private AudioSource audioSource;
    private PlayerMovementWithPauseUI playerScript;
    private Transform cachedTransform; // cache transform utk efisiensi

    void Awake()
    {
        cachedTransform = transform;

        // Cache komponen agar tidak dipanggil berulang
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = waterSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        if (playerObject != null)
            playerScript = playerObject.GetComponent<PlayerMovementWithPauseUI>();

        if (baseHeight == 0f)
            baseHeight = cachedTransform.position.y;
    }

    void Update()
    {
        // 💧 Gerak ombak (pakai counter, bukan Time.time untuk stabilitas FPS rendah)
        timeCounter += waveSpeed * Time.deltaTime;
        float newY = baseHeight + Mathf.Sin(timeCounter) * waveAmplitude;
        cachedTransform.position = new Vector3(cachedTransform.position.x, newY, cachedTransform.position.z);

        // 🎧 Perbarui audio hanya jika diperlukan
        if (audioSource != null)
            HandleAudioFade();
    }

    private void HandleAudioFade()
    {
        float targetVolume = playerInside ? 1f : 0f;
        float newVolume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        // Hanya ubah jika beda (hemat CPU)
        if (Mathf.Abs(newVolume - audioSource.volume) > 0.001f)
            audioSource.volume = newVolume;

        // Start / stop audio hanya sekali
        if (playerInside && !audioSource.isPlaying)
            audioSource.Play();
        else if (!playerInside && audioSource.volume <= 0.01f && audioSource.isPlaying)
            audioSource.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerObject)
        {
            playerInside = true;
            if (playerScript != null)
                playerScript.SetMoveSpeed(slowSpeed);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerObject)
        {
            playerInside = false;
            if (playerScript != null)
                playerScript.ResetMoveSpeed();
        }
    }
}
