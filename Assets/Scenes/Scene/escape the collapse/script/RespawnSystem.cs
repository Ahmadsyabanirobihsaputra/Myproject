using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    public static RespawnSystem Instance; // agar bisa diakses dari mana saja (misal dari UI global)

    [Header("Target Player")]
    [Tooltip("Taruh GameObject player di sini (kosongkan untuk cari otomatis).")]
    public GameObject playerObject;

    [Header("Respawn Settings")]
    [Tooltip("Gunakan posisi ini sebagai titik spawn manual (kosongkan untuk pakai posisi awal player).")]
    public Transform customSpawnPoint;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    void Awake()
    {
        // pastikan hanya ada satu instance aktif
        Instance = this;
    }

    void Start()
    {
        // Jika belum diisi manual, cari otomatis player di scene
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogWarning("[RespawnSystem] Player belum diassign dan tidak ditemukan di scene!");
                return;
            }
        }

        // Tentukan posisi spawn
        if (customSpawnPoint != null)
        {
            spawnPosition = customSpawnPoint.position;
            spawnRotation = customSpawnPoint.rotation;
        }
        else
        {
            spawnPosition = playerObject.transform.position;
            spawnRotation = playerObject.transform.rotation;
        }
    }

    void Update()
    {
        // Tekan R untuk respawn manual
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }
    }

    // Fungsi publik agar tombol UI atau sistem global dapat memanggilnya
    public void Respawn()
    {
        if (playerObject == null)
        {
            Debug.LogWarning("[RespawnSystem] Tidak ada player untuk direspawn!");
            return;
        }

        playerObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        Debug.Log($"{playerObject.name} respawned to initial position!");
    }

    // Fungsi statis agar tombol global (seperti UI di Canvas) bisa memanggil respawn
    public static void TriggerRespawn()
    {
        if (Instance != null)
        {
            Instance.Respawn();
        }
        else
        {
            Debug.LogWarning("[RespawnSystem] No active RespawnSystem found in scene!");
        }
    }
}
