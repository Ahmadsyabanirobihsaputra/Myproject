
using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif


public class WindowsSpeechDiagnostic : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    public Text outputText;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Diagnostic Settings")]

    public bool showDebug = true;

    [Tooltip("Delay before starting speech recognition.")]
    public float startDelay = 1f;

    [Tooltip("Seconds before checking recognizer status.")]
    public float statusCheckDelay = 2f;

    [Tooltip("Automatically restart after Canceled / Timeout.")]
    public bool autoRestart = false;

    [Tooltip("Delay before automatic restart.")]
    public float restartDelay = 2f;


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // =========================================================
    // WINDOWS SPEECH
    // =========================================================

    private DictationRecognizer recognizer;

    private bool isStarting = false;

    private bool isDestroying = false;

    private bool restartRunning = false;

    private int sessionNumber = 0;

#endif


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        ClearOutput();

        Log("========================================");
        Log("WINDOWS SPEECH DIAGNOSTIC");
        Log("========================================");

        Log(
            "Unity Platform: " +
            Application.platform
        );

        Log(
            "Unity Version: " +
            Application.unityVersion
        );

        Log(
            "Operating System: " +
            SystemInfo.operatingSystem
        );

        Log("");

        CheckMicrophone();

        Log("");
        Log("Starting speech test...");

        StartCoroutine(
            StartSpeechDelayed()
        );

#else

        Log(
            "This diagnostic requires Windows."
        );

#endif
    }


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // =========================================================
    // MICROPHONE CHECK
    // =========================================================

    private void CheckMicrophone()
    {
        Log("----------------------------------------");
        Log("MICROPHONE CHECK");
        Log("----------------------------------------");

        string[] microphones =
            Microphone.devices;


        if (microphones == null ||
            microphones.Length == 0)
        {
            LogWarning(
                "NO MICROPHONE DETECTED."
            );

            return;
        }


        Log(
            "Microphones detected: " +
            microphones.Length
        );


        for (int i = 0;
             i < microphones.Length;
             i++)
        {
            Log(
                "Microphone [" +
                i +
                "]: " +
                microphones[i]
            );
        }
    }


    // =========================================================
    // DELAY BEFORE START
    // =========================================================

    private IEnumerator StartSpeechDelayed()
    {
        if (startDelay > 0f)
        {
            Log(
                "Waiting " +
                startDelay +
                " seconds before starting..."
            );

            yield return new WaitForSeconds(
                startDelay
            );
        }


        if (isDestroying)
            yield break;


        StartSpeech();
    }


    // =========================================================
    // START SPEECH
    // =========================================================

    private void StartSpeech()
    {
        if (isStarting)
        {
            LogWarning(
                "StartSpeech() already running."
            );

            return;
        }


        if (isDestroying)
            return;


        isStarting = true;

        sessionNumber++;


        Log("");
        Log("========================================");
        Log(
            "STARTING SPEECH SESSION #" +
            sessionNumber
        );
        Log("========================================");


        // -----------------------------------------------------
        // Dispose old recognizer
        // -----------------------------------------------------

        if (recognizer != null)
        {
            LogWarning(
                "Old recognizer exists."
            );

            DisposeRecognizer();

            Log(
                "Old recognizer disposed."
            );
        }


        // -----------------------------------------------------
        // Create recognizer
        // -----------------------------------------------------

        Log(
            "Creating DictationRecognizer..."
        );


        try
        {
            recognizer =
                new DictationRecognizer(
                    ConfidenceLevel.Low
                );
        }
        catch (Exception e)
        {
            LogError(
                "FAILED TO CREATE RECOGNIZER."
            );

            LogError(
                e.ToString()
            );

            isStarting = false;

            return;
        }


        Log(
            "Recognizer created successfully."
        );


        // =====================================================
        // TIMEOUT SETTINGS
        // =====================================================

        recognizer.InitialSilenceTimeoutSeconds =
            30f;

        recognizer.AutoSilenceTimeoutSeconds =
            30f;


        Log(
            "Initial Silence Timeout: " +
            recognizer.InitialSilenceTimeoutSeconds
        );

        Log(
            "Auto Silence Timeout: " +
            recognizer.AutoSilenceTimeoutSeconds
        );


        // =====================================================
        // EVENT SUBSCRIPTIONS
        // =====================================================

        recognizer.DictationHypothesis +=
            OnDictationHypothesis;


        recognizer.DictationResult +=
            OnDictationResult;


        recognizer.DictationError +=
            OnDictationError;


        recognizer.DictationComplete +=
            OnDictationComplete;


        Log(
            "Speech events registered."
        );


        // =====================================================
        // STATUS BEFORE START
        // =====================================================

        Log("");

        Log(
            "STATUS BEFORE START: " +
            recognizer.Status
        );


        // =====================================================
        // START
        // =====================================================

        try
        {
            Log("");
            Log(
                "Calling recognizer.Start()..."
            );


            recognizer.Start();


            Log(
                "recognizer.Start() returned."
            );
        }
        catch (Exception e)
        {
            LogError(
                "START() EXCEPTION"
            );

            LogError(
                e.ToString()
            );

            isStarting = false;

            return;
        }


        // =====================================================
        // WAIT ONE FRAME
        // =====================================================

        StartCoroutine(
            CheckRecognizerAfterStart()
        );
    }


    // =========================================================
    // CHECK STATUS AFTER START
    // =========================================================

    private IEnumerator CheckRecognizerAfterStart()
    {
        yield return null;


        if (isDestroying)
        {
            isStarting = false;
            yield break;
        }


        if (recognizer == null)
        {
            LogError(
                "Recognizer became NULL after Start()."
            );

            isStarting = false;

            yield break;
        }


        // -----------------------------------------------------
        // Status after one frame
        // -----------------------------------------------------

        Log("");
        Log(
            "STATUS AFTER 1 FRAME: " +
            recognizer.Status
        );


        // -----------------------------------------------------
        // If running
        // -----------------------------------------------------

        if (recognizer.Status ==
            SpeechSystemStatus.Running)
        {
            Log("");
            Log("========================================");
            Log("SPEECH RECOGNIZER IS RUNNING");
            Log("========================================");

            Log("");
            Log(">>> NOW SPEAK <<<");
            Log("");
            Log("Try saying:");
            Log("HELLO UNITY");
            Log("");

            if (outputText != null)
            {
                outputText.text =
                    "LISTENING...\n\n" +
                    "Say:\n" +
                    "HELLO UNITY";
            }
        }
        else
        {
            LogWarning(
                "Recognizer is NOT Running."
            );

            LogWarning(
                "Current status: " +
                recognizer.Status
            );
        }


        // -----------------------------------------------------
        // Wait additional time
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            statusCheckDelay
        );


        if (isDestroying)
        {
            isStarting = false;
            yield break;
        }


        if (recognizer == null)
        {
            isStarting = false;
            yield break;
        }


        Log("");
        Log(
            "STATUS AFTER " +
            statusCheckDelay +
            " SECONDS: " +
            recognizer.Status
        );


        isStarting = false;
    }


    // =========================================================
    // HYPOTHESIS
    // =========================================================

    private void OnDictationHypothesis(
        string text)
    {
        Log("");
        Log("========================================");
        Log("HYPOTHESIS RECEIVED");
        Log("========================================");

        Log(
            "Text: " +
            text
        );


        if (outputText != null)
        {
            outputText.text =
                "HYPOTHESIS:\n\n" +
                text;
        }
    }


    // =========================================================
    // RESULT
    // =========================================================

    private void OnDictationResult(
        string text,
        ConfidenceLevel confidence)
    {
        Log("");
        Log("========================================");
        Log("RESULT RECEIVED");
        Log("========================================");

        Log(
            "Text: " +
            text
        );

        Log(
            "Confidence: " +
            confidence
        );


        if (outputText != null)
        {
            outputText.text =
                "RESULT:\n\n" +
                text +
                "\n\nConfidence: " +
                confidence;
        }
    }


    // =========================================================
    // ERROR
    // =========================================================

    private void OnDictationError(
        string error,
        int hresult)
    {
        LogError("");
        LogError(
            "========================================"
        );

        LogError(
            "DICTATION ERROR"
        );

        LogError(
            "========================================"
        );

        LogError(
            "Error: " +
            error
        );

        LogError(
            "HRESULT: " +
            hresult
        );

        LogError(
            "HRESULT HEX: 0x" +
            hresult.ToString("X8")
        );


        if (outputText != null)
        {
            outputText.text =
                "DICTATION ERROR\n\n" +
                error +
                "\n\nHRESULT: 0x" +
                hresult.ToString("X8");
        }
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    private void OnDictationComplete(
        DictationCompletionCause cause)
    {
        Log("");
        LogWarning(
            "========================================"
        );

        LogWarning(
            "DICTATION COMPLETE"
        );

        LogWarning(
            "========================================"
        );

        LogWarning(
            "Cause: " +
            cause
        );


        if (recognizer != null)
        {
            LogWarning(
                "Recognizer Status: " +
                recognizer.Status
            );
        }


        // =====================================================
        // HANDLE COMPLETION CAUSE
        // =====================================================

        if (cause ==
            DictationCompletionCause.Complete)
        {
            Log(
                "Dictation completed normally."
            );
        }
        else if (
            cause ==
            DictationCompletionCause.TimeoutExceeded)
        {
            LogWarning(
                "Windows stopped dictation because " +
                "the session timed out."
            );
        }
        else if (
            cause ==
            DictationCompletionCause.Canceled)
        {
            LogWarning(
                "Windows CANCELED the dictation session."
            );

            LogWarning(
                "No speech result was received."
            );

            LogWarning(
                "This means the Windows Speech API " +
                "ended the recognition session."
            );
        }
        else if (
            cause ==
            DictationCompletionCause.PauseLimitExceeded)
        {
            LogWarning(
                "Windows stopped dictation because " +
                "the pause limit was exceeded."
            );
        }
        else
        {
            // -------------------------------------------------
            // IMPORTANT:
            // Do NOT reference DictationCompletionCause.Unknown
            // because that enum does not exist in this Unity API.
            // -------------------------------------------------

            LogWarning(
                "Unhandled completion cause: " +
                cause
            );
        }


        // =====================================================
        // UI
        // =====================================================

        if (outputText != null)
        {
            outputText.text =
                "DICTATION COMPLETE\n\n" +
                "Cause:\n" +
                cause +
                "\n\n" +
                "Check Console.";
        }


        // =====================================================
        // AUTO RESTART
        // =====================================================

        if (autoRestart &&
            !isDestroying &&
            !restartRunning)
        {
            StartCoroutine(
                RestartSpeechAfterComplete()
            );
        }
    }


    // =========================================================
    // AUTO RESTART
    // =========================================================

    private IEnumerator RestartSpeechAfterComplete()
    {
        if (restartRunning)
            yield break;


        if (isDestroying)
            yield break;


        restartRunning = true;


        Log("");
        Log(
            "Restarting speech in " +
            restartDelay +
            " seconds..."
        );


        yield return new WaitForSeconds(
            restartDelay
        );


        if (isDestroying)
        {
            restartRunning = false;
            yield break;
        }


        DisposeRecognizer();


        yield return new WaitForSeconds(
            0.5f
        );


        if (!isDestroying)
        {
            StartSpeech();
        }


        restartRunning = false;
    }


    // =========================================================
    // MANUAL STOP
    // =========================================================

    public void StopSpeech()
    {
        Log("");
        Log(
            "Manual StopSpeech() called."
        );


        if (recognizer == null)
        {
            LogWarning(
                "Recognizer is NULL."
            );

            return;
        }


        try
        {
            Log(
                "Current Status: " +
                recognizer.Status
            );


            if (recognizer.Status ==
                SpeechSystemStatus.Running)
            {
                Log(
                    "Calling recognizer.Stop()..."
                );

                recognizer.Stop();
            }
            else
            {
                LogWarning(
                    "Recognizer is not running."
                );
            }
        }
        catch (Exception e)
        {
            LogError(
                "Stop exception:"
            );

            LogError(
                e.ToString()
            );
        }
    }


    // =========================================================
    // MANUAL RESTART
    // =========================================================

    public void RestartSpeech()
    {
        if (isDestroying)
            return;


        Log("");
        Log(
            "Manual RestartSpeech() called."
        );


        StopAllCoroutines();


        restartRunning = false;
        isStarting = false;


        DisposeRecognizer();


        StartCoroutine(
            ManualRestartRoutine()
        );
    }


    private IEnumerator ManualRestartRoutine()
    {
        yield return new WaitForSeconds(
            1f
        );


        if (!isDestroying)
        {
            StartSpeech();
        }
    }


    // =========================================================
    // DISPOSE
    // =========================================================

    private void DisposeRecognizer()
    {
        if (recognizer == null)
            return;


        Log(
            "Disposing recognizer..."
        );


        try
        {
            if (recognizer.Status ==
                SpeechSystemStatus.Running)
            {
                recognizer.Stop();
            }
        }
        catch (Exception e)
        {
            LogWarning(
                "Stop during dispose failed: " +
                e.Message
            );
        }


        try
        {
            recognizer.DictationHypothesis -=
                OnDictationHypothesis;

            recognizer.DictationResult -=
                OnDictationResult;

            recognizer.DictationError -=
                OnDictationError;

            recognizer.DictationComplete -=
                OnDictationComplete;
        }
        catch (Exception e)
        {
            LogWarning(
                "Event cleanup failed: " +
                e.Message
            );
        }


        try
        {
            recognizer.Dispose();
        }
        catch (Exception e)
        {
            LogWarning(
                "Dispose failed: " +
                e.Message
            );
        }


        recognizer = null;


        Log(
            "Recognizer disposed."
        );
    }

#endif


    // =========================================================
    // CLEAR OUTPUT
    // =========================================================

    private void ClearOutput()
    {
        if (outputText != null)
        {
            outputText.text = "";
        }
    }


    // =========================================================
    // LOG
    // =========================================================

    private void Log(
        string message)
    {
        string finalMessage =
            "[SpeechDiagnostic] " +
            message;


        if (showDebug)
        {
            Debug.Log(
                finalMessage
            );
        }


        if (outputText != null)
        {
            outputText.text +=
                "\n" +
                message;
        }
    }


    // =========================================================
    // WARNING
    // =========================================================

    private void LogWarning(
        string message)
    {
        string finalMessage =
            "[SpeechDiagnostic] " +
            message;


        Debug.LogWarning(
            finalMessage
        );


        if (outputText != null)
        {
            outputText.text +=
                "\n" +
                message;
        }
    }


    // =========================================================
    // ERROR
    // =========================================================

    private void LogError(
        string message)
    {
        string finalMessage =
            "[SpeechDiagnostic] " +
            message;


        Debug.LogError(
            finalMessage
        );


        if (outputText != null)
        {
            outputText.text +=
                "\n" +
                message;
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        isDestroying = true;

        StopAllCoroutines();

        DisposeRecognizer();

#endif
    }
}
