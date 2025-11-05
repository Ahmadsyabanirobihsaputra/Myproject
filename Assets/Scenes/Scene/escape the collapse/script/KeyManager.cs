using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    [Header("Daftar Kunci di Scene")]
    public GameObject[] keyObjects;

    [HideInInspector] public bool PlayerHasKey = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Matikan semua kunci
        foreach (GameObject key in keyObjects)
            key.SetActive(false);

        // Aktifkan satu kunci secara acak
        if (keyObjects.Length > 0)
        {
            int randomIndex = Random.Range(0, keyObjects.Length);
            keyObjects[randomIndex].SetActive(true);
        }

        PlayerHasKey = false;
    }
}
