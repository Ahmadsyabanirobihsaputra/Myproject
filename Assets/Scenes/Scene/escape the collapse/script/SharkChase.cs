using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class SharkChase : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;           // GameObject pemain
    public GameObject waterArea;       // GameObject area air
    public float chaseSpeed = 5f;      // Kecepatan hiu mengejar
    public float chaseRadius = 10f;    // Radius jangkauan pengejaran pemain
    public float returnSpeed = 3f;     // Kecepatan hiu kembali ke posisi semula

    [Header("Scene Settings")]
    [Tooltip("Nama scene yang akan dimuat saat hiu menyentuh pemain")]
    public string nextSceneName = "GameOverScene";

    [Header("Audio Settings")]
    [Tooltip("Suara yang dimainkan saat hiu mendekati pemain")]
    public AudioClip sharkGrowlClip;
    public float soundTriggerDistance = 5f;
    public float soundCooldown = 3f;

    private bool sharkInWater = false;
    private bool playerInWater = false;
    private AudioSource audioSource;
    private float lastSoundTime = -999f;
    private Vector3 startPosition; // posisi awal hiu

    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        startPosition = transform.position; // simpan posisi awal hiu
    }

    void Update()
    {
        if (player == null || waterArea == null) return;

        // Cek apakah hiu dan pemain berada di dalam air
        sharkInWater = IsInsideWater(transform.position);
        playerInWater = IsInsideWater(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerInRange = distanceToPlayer <= chaseRadius;

        // Jika pemain berada di air dan hiu juga di air
        if (sharkInWater && playerInWater && playerInRange)
        {
            // Arahkan hiu ke arah pemain secara horizontal saja
            Vector3 targetDir = (player.position - transform.position);
            targetDir.y = 0; // kunci rotasi di sumbu Y agar tidak mendongak
            targetDir.Normalize();

            if (targetDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
            }

            // Gerakkan hiu ke depan
            transform.position += transform.forward * chaseSpeed * Time.deltaTime;

            // Mainkan suara jika cukup dekat & cooldown selesai
            if (distanceToPlayer <= soundTriggerDistance && Time.time - lastSoundTime > soundCooldown)
            {
                PlaySharkSound();
                lastSoundTime = Time.time;
            }
        }
        else
        {
            // Pemain tidak di air → hiu kembali ke posisi awal
            ReturnToStartPosition();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Jika hiu menyentuh pemain → pindah scene
        if (other.transform == player)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void ReturnToStartPosition()
    {
        // Arah kembali ke posisi awal
        Vector3 dir = (startPosition - transform.position);
        dir.y = 0; // tetap sejajar dengan permukaan air
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 2f);
            transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
        }
    }

    bool IsInsideWater(Vector3 position)
    {
        if (waterArea == null) return false;
        Collider waterCollider = waterArea.GetComponent<Collider>();
        if (waterCollider == null) return false;
        return waterCollider.bounds.Contains(position);
    }

    void PlaySharkSound()
    {
        if (audioSource != null && sharkGrowlClip != null)
            audioSource.PlayOneShot(sharkGrowlClip);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, soundTriggerDistance);
    }
}
