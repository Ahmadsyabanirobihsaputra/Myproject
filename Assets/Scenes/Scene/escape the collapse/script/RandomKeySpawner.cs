using UnityEngine;

public class RandomKeySpawner : MonoBehaviour
{
    [Header("Key Objects (Assign all possible key locations)")]
    public GameObject[] possibleKeys;

    private GameObject activeKey;

    void Start()
    {
        if (possibleKeys == null || possibleKeys.Length == 0)
        {
            Debug.LogWarning("No key objects assigned to RandomKeySpawner!");
            return;
        }

        // Disable all keys first
        foreach (GameObject key in possibleKeys)
        {
            key.SetActive(false);
        }

        // Pick a random key
        int randomIndex = Random.Range(0, possibleKeys.Length);
        activeKey = possibleKeys[randomIndex];

        // Enable that one
        activeKey.SetActive(true);

        Debug.Log($"Activated key: {activeKey.name}");
    }
}
