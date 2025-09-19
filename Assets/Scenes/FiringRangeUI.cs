using UnityEngine;
using UnityEngine.UI;


public class FiringRangeUI : MonoBehaviour
{
    [Header("UI References")]
    public Text shotsText;
    public Text hitsText;
    public Text missesText;
    public Text accuracyText;

    void Update()
    {
        if (FiringRangeManager.Instance != null)
        {
            shotsText.text = "Shots: " + FiringRangeManager.Instance.totalShots;
            hitsText.text = "Hits: " + FiringRangeManager.Instance.hits;
            missesText.text = "Misses: " + FiringRangeManager.Instance.misses;
            accuracyText.text = "Accuracy: " +
                FiringRangeManager.Instance.GetAccuracy().ToString("F1") + "%";
        }
    }
}
