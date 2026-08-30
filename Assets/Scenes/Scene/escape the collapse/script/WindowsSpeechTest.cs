
using UnityEngine;
using UnityEngine.Windows.Speech;

public class WindowsSpeechTest : MonoBehaviour
{
    private DictationRecognizer recognizer;

    void Start()
    {
        recognizer = new DictationRecognizer();

        recognizer.DictationResult += (text, confidence) =>
        {
            Debug.Log("Recognized: " + text);
        };

        recognizer.DictationError += (error, hresult) =>
        {
            Debug.LogError("Speech Error: " + error);
        };

        recognizer.DictationComplete += (cause) =>
        {
            Debug.Log("Speech Complete: " + cause);
        };

        recognizer.Start();

        Debug.Log("DictationRecognizer started.");
    }

    void OnDestroy()
    {
        if (recognizer != null)
        {
            if (recognizer.Status == SpeechSystemStatus.Running)
                recognizer.Stop();

            recognizer.Dispose();
        }
    }
}