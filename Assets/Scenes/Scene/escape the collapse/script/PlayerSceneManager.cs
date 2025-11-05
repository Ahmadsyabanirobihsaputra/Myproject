using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Daftar nama scene di mana mode FPS aktif (misalnya: Gameplay1, Gameplay2, TutorialRoom, dsb.)")]
    public string[] fpsScenes; // bisa isi lebih dari satu scene di Inspector

    private PlayerMovementWithPauseUI playerMovementScript;

    void Awake()
    {
        // Jangan hancurkan manager ini saat pindah scene
        DontDestroyOnLoad(gameObject);

        // Dengarkan event scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Jalankan pengecekan awal saat scene pertama dimuat
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cari player yang memiliki script PlayerMovementWithPauseUI
        playerMovementScript = FindObjectOfType<PlayerMovementWithPauseUI>();

        // Cek apakah nama scene ini ada di daftar FPS scenes
        bool isFPSScene = fpsScenes != null && fpsScenes.Contains(scene.name);

        if (playerMovementScript != null)
        {
            // Aktifkan atau nonaktifkan kontrol FPS
            playerMovementScript.SetFPSControl(isFPSScene);
        }
        else
        {
            // Jika tidak ada player di scene, pastikan cursor aktif
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
        }
    }
}
