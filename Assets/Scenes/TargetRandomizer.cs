using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class TargetRandomizer : MonoBehaviour
{
    [Header("Stickman Targets")]
    public List<GameObject> stickmen = new List<GameObject>(); // All possible stickmen
    public int numberOfActiveTargets = 3; // How many to enable

    [Header("UI")]
    public Text clueUIText; // UI text field to show clue

    [Header("Sniper Reference")]
    public WorldCrosshairSniper sniperScript; // Sniper script reference

    private GameObject chosenTarget; // The chosen "real" stickman

    void Start()
    {
        RandomizeTargets();
        PickRealTarget();
    }

    void RandomizeTargets()
    {
        // Disable all stickmen
        foreach (GameObject s in stickmen)
        {
            if (s != null) s.SetActive(false);
        }

        // Shuffle list (Fisher-Yates)
        for (int i = 0; i < stickmen.Count; i++)
        {
            GameObject temp = stickmen[i];
            int randomIndex = Random.Range(i, stickmen.Count);
            stickmen[i] = stickmen[randomIndex];
            stickmen[randomIndex] = temp;
        }

        // Enable the first N
        int activeCount = Mathf.Clamp(numberOfActiveTargets, 1, stickmen.Count);
        for (int i = 0; i < activeCount; i++)
        {
            if (stickmen[i] != null) stickmen[i].SetActive(true);
        }
    }

    void PickRealTarget()
    {
        // Gather active stickmen
        List<GameObject> activeOnes = stickmen.FindAll(s => s.activeSelf);
        if (activeOnes.Count == 0) return;

        // Pick ONE to be the real target
        chosenTarget = activeOnes[Random.Range(0, activeOnes.Count)];

        // Clear all targets’ "isCorrectTarget"
        foreach (GameObject s in activeOnes)
        {
            TargetMarker tm = s.GetComponent<TargetMarker>();
            if (tm != null && tm.head != null)
                tm.head.isCorrectTarget = false;
        }

        // Mark chosen one
        TargetMarker marker = chosenTarget.GetComponent<TargetMarker>();
        if (marker != null && sniperScript != null)
        {
            // Assign winning head
            marker.head.isCorrectTarget = true;
            sniperScript.requiredTargetHead = marker.head;

            // Show clue in UI
            if (clueUIText != null)
                clueUIText.text = marker.clue;
        }
    }
}
