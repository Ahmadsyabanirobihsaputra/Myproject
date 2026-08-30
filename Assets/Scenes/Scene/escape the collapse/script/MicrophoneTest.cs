
using UnityEngine;
using UnityEngine.UI;


public class MicrophoneTest : MonoBehaviour
{
    [Header("UI")]
    public Text statusText;

    [Header("Microphone")]
    public int microphoneIndex = 0;

    private AudioClip microphoneClip;

    private string selectedMicrophone;

    private void Start()
    {
        Debug.Log("======================================");
        Debug.Log("[MicrophoneTest] Starting...");
        Debug.Log("======================================");


        // =====================================================
        // LIST MICROPHONES
        // =====================================================

        string[] microphones =
            Microphone.devices;


        if (microphones.Length == 0)
        {
            Debug.LogError(
                "[MicrophoneTest] NO MICROPHONE FOUND!"
            );

            SetText(
                "NO MICROPHONE FOUND!"
            );

            return;
        }


        Debug.Log(
            "[MicrophoneTest] Microphones found: " +
            microphones.Length
        );


        for (int i = 0; i < microphones.Length; i++)
        {
            Debug.Log(
                "[MicrophoneTest] [" +
                i +
                "] " +
                microphones[i]
            );
        }


        // =====================================================
        // SELECT MICROPHONE
        // =====================================================

        microphoneIndex =
            Mathf.Clamp(
                microphoneIndex,
                0,
                microphones.Length - 1
            );


        selectedMicrophone =
            microphones[microphoneIndex];


        Debug.Log(
            "[MicrophoneTest] Selected microphone: " +
            selectedMicrophone
        );


        // =====================================================
        // START MICROPHONE
        // =====================================================

        try
        {
            microphoneClip =
                Microphone.Start(
                    selectedMicrophone,
                    true,
                    10,
                    44100
                );
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "[MicrophoneTest] " +
                "Microphone.Start failed: " +
                e.Message
            );

            SetText(
                "Microphone.Start failed:\n" +
                e.Message
            );

            return;
        }


        // =====================================================
        // UI
        // =====================================================

        SetText(
            "Microphone:\n" +
            selectedMicrophone +
            "\n\nSpeak now..."
        );


        Debug.Log(
            "[MicrophoneTest] Microphone started."
        );
    }


    private void Update()
    {
        if (microphoneClip == null)
            return;


        // =====================================================
        // CHECK MICROPHONE POSITION
        // =====================================================

        int position =
            Microphone.GetPosition(
                selectedMicrophone
            );


        if (position <= 0)
            return;


        // =====================================================
        // GET AUDIO DATA
        // =====================================================

        float[] samples =
            new float[128];


        int startPosition =
            position - samples.Length;


        if (startPosition < 0)
            return;


        microphoneClip.GetData(
            samples,
            startPosition
        );


        // =====================================================
        // CALCULATE VOLUME
        // =====================================================

        float sum = 0f;


        for (int i = 0; i < samples.Length; i++)
        {
            sum +=
                samples[i] *
                samples[i];
        }


        float rms =
            Mathf.Sqrt(
                sum / samples.Length
            );


        float volume =
            rms * 100f;


        // =====================================================
        // DEBUG
        // =====================================================

        if (volume > 0.1f)
        {
            Debug.Log(
                "[MicrophoneTest] " +
                "Microphone detected sound. " +
                "Volume: " +
                volume.ToString("F2")
            );
        }


        // =====================================================
        // UI
        // =====================================================

        SetText(
            "Microphone:\n" +
            selectedMicrophone +
            "\n\n" +
            "Volume: " +
            volume.ToString("F2") +
            "\n\n" +
            (
                volume > 0.1f
                    ? "SOUND DETECTED"
                    : "Listening..."
            )
        );
    }


    private void SetText(string message)
    {
        if (statusText != null)
        {
            statusText.text =
                message;
        }
    }


    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(selectedMicrophone))
        {
            if (Microphone.IsRecording(
                selectedMicrophone))
            {
                Microphone.End(
                    selectedMicrophone
                );
            }
        }


        Debug.Log(
            "[MicrophoneTest] Microphone stopped."
        );
    }
}