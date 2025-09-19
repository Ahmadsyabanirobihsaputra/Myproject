using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class ShootingRangeRandomizer : MonoBehaviour
{
    [Header("All Targets in Scene")]
    public List<TargetMarker> allTargets = new List<TargetMarker>(); // assign all possible targets in Inspector
    public int maxActiveTargets = 3; // how many should be visible at once

    [Header("UI")]
    public Text clueUIText; // shows the clue of the chosen target

    [Header("Sniper Reference")]
    public ShootingRangeSniper sniperScript;

    private List<TargetMarker> activeTargets = new List<TargetMarker>();
    private List<TargetMarker> inactiveTargets = new List<TargetMarker>();
    private TargetMarker currentClueTarget;

    void Start()
    {
        InitializeTargets();
        PickNewClueTarget();
    }

    void InitializeTargets()
    {
        // Put all in inactive list
        inactiveTargets.Clear();
        activeTargets.Clear();

        foreach (var marker in allTargets)
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(false);
                inactiveTargets.Add(marker);
            }
        }

        // Activate initial set
        for (int i = 0; i < Mathf.Min(maxActiveTargets, inactiveTargets.Count); i++)
        {
            ActivateRandomInactiveTarget();
        }
    }

    void ActivateRandomInactiveTarget()
    {
        if (inactiveTargets.Count == 0) return;

        int idx = Random.Range(0, inactiveTargets.Count);
        TargetMarker chosen = inactiveTargets[idx];
        inactiveTargets.RemoveAt(idx);

        chosen.gameObject.SetActive(true);
        activeTargets.Add(chosen);
    }

    void DeactivateTarget(TargetMarker marker)
    {
        if (marker == null) return;

        marker.gameObject.SetActive(false);
        activeTargets.Remove(marker);
        inactiveTargets.Add(marker);
    }

    public void OnTargetHit(TargetPart part)
    {
        // Find which target was hit
        TargetMarker marker = part.GetComponentInParent<TargetMarker>();
        if (marker == null) return;

        // Disable that one
        DeactivateTarget(marker);

        // Enable a new random one
        ActivateRandomInactiveTarget();

        // Always re-roll clue among currently active targets
        PickNewClueTarget();
    }

    public void PickNewClueTarget()
    {
        if (activeTargets.Count == 0) return;

        // reset all first
        foreach (var marker in activeTargets)
        {
            if (marker.head != null)
                marker.head.isCorrectTarget = false;
        }

        // pick one active as the clue
        currentClueTarget = activeTargets[Random.Range(0, activeTargets.Count)];

        if (currentClueTarget != null && currentClueTarget.head != null)
        {
            currentClueTarget.head.isCorrectTarget = true;

            // tell sniper
            if (sniperScript != null)
                sniperScript.requiredTargetHead = currentClueTarget.head;

            // update UI
            if (clueUIText != null)
                clueUIText.text = currentClueTarget.clue;
        }
    }
}
