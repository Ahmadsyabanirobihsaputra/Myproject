using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ButtonPuzzleUI : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI sequenceText;
    public List<string> possibleButtons = new List<string> { "Circle", "Triangle", "Square" };
    public int sequenceLength = 3;

    [Header("Puzzle Objects")]
    public GameObject deactivateObject;  // The object to hide (e.g., locked door)
    public GameObject activateObject;    // The object to show (e.g., open door)

    private List<string> currentSequence = new List<string>();
    private int currentStep = 0;

    private void Start()
    {
        GenerateSequence();
        UpdateUI();
    }

    void GenerateSequence()
    {
        currentSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            string randomButton = possibleButtons[Random.Range(0, possibleButtons.Count)];
            currentSequence.Add(randomButton);
        }
        currentStep = 0;
    }

    void UpdateUI()
    {
        if (sequenceText != null)
        {
            string display = "Sequence: ";
            for (int i = 0; i < currentSequence.Count; i++)
            {
                if (i == currentStep)
                    display += $"<b><color=yellow>{currentSequence[i]}</color></b> ";
                else
                    display += $"{currentSequence[i]} ";
            }
            sequenceText.text = display;
        }
    }

    public bool CanPressButton(string buttonName)
    {
        return currentSequence[currentStep] == buttonName;
    }

    public void NextSequenceStep()
    {
        currentStep++;
        if (currentStep >= currentSequence.Count)
        {
            Debug.Log("Puzzle complete! Activating door!");

            // Only activate/deactivate when the puzzle is completed
            if (deactivateObject != null)
                deactivateObject.SetActive(false);

            if (activateObject != null)
                activateObject.SetActive(true);
        }
        UpdateUI();
    }

    public void ResetSequence()
    {
        Debug.Log("Sequence reset!");
        currentStep = 0;
        UpdateUI();
    }
}
