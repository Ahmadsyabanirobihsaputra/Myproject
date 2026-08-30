using UnityEngine;
using UnityEngine.UI;



#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

using System.Collections;

public class SimpleWindowsSpeechTest : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    public Text speechText;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    public bool showDebug = true;

    [Tooltip("Automatically restart recognition after it stops.")]
    public bool autoRestart = true;

    [Tooltip("Delay before restarting speech recognition.")]
    public float restartDelay = 0.5f;


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // =========================================================
    // WINDOWS SPEECH
    // =========================================================

    private DictationRecognizer dictationRecognizer;

    private bool isStarting = false;
    private bool isStopping = false;
    private bool isDestroyed = false;


#endif


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        isDestroyed = false;

        StartCoroutine(StartSpeech());

#else

        if (speechText != null)
        {
            speechText.text =
                "Windows Speech hanya tersedia di Windows.";
        }

#endif
    }


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // =========================================================
    // START SPEECH
    // =========================================================

    private IEnumerator StartSpeech()
    {
        if (isStarting)
            yield break;

        if (isDestroyed)
            yield break;

        isStarting = true;


        // -----------------------------------------------------
        // Kalau recognizer lama masih ada
        // -----------------------------------------------------

        if (dictationRecognizer != null)
        {
            yield return StartCoroutine(
                StopAndDisposeRecognizer()
            );
        }


        // -----------------------------------------------------
        // Tunggu 1 frame
        // -----------------------------------------------------

        yield return null;


        if (isDestroyed)
        {
            isStarting = false;
            yield break;
        }


        // -----------------------------------------------------
        // CREATE NEW RECOGNIZER
        // -----------------------------------------------------

        dictationRecognizer =
            new DictationRecognizer();


        // =====================================================
        // HYPOTHESIS
        // =====================================================

        dictationRecognizer.DictationHypothesis +=
            OnDictationHypothesis;


        // =====================================================
        // FINAL RESULT
        // =====================================================

        dictationRecognizer.DictationResult +=
            OnDictationResult;


        // =====================================================
        // ERROR
        // =====================================================

        dictationRecognizer.DictationError +=
            OnDictationError;


        // =====================================================
        // COMPLETE
        // =====================================================

        dictationRecognizer.DictationComplete +=
            OnDictationComplete;


        // =====================================================
        // START
        // =====================================================

        try
        {
            if (showDebug)
            {
                Debug.Log(
                    "[SpeechTest] Starting Windows Dictation..."
                );
            }


            dictationRecognizer.Start();


            if (speechText != null)
            {
                speechText.text =
                    "Listening...";
            }


            if (showDebug)
            {
                Debug.Log(
                    "[SpeechTest] DICTATION STARTED"
                );
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "[SpeechTest] START ERROR: " +
                e.Message
            );


            if (speechText != null)
            {
                speechText.text =
                    "Start Error:\n" +
                    e.Message;
            }
        }


        isStarting = false;
    }


    // =========================================================
    // HYPOTHESIS
    // =========================================================

    private void OnDictationHypothesis(
        string text)
    {
        if (isDestroyed)
            return;


        if (showDebug)
        {
            Debug.Log(
                "[SpeechTest] HYPOTHESIS: " +
                text
            );
        }


        if (speechText != null)
        {
            speechText.text =
                text;
        }
    }


    // =========================================================
    // FINAL RESULT
    // =========================================================

    private void OnDictationResult(
        string text,
        ConfidenceLevel confidence)
    {
        if (isDestroyed)
            return;


        if (showDebug)
        {
            Debug.Log(
                "[SpeechTest] RESULT: " +
                text +
                " | Confidence: " +
                confidence
            );
        }


        if (speechText != null)
        {
            speechText.text =
                text;
        }
    }


    // =========================================================
    // ERROR
    // =========================================================

    private void OnDictationError(
        string error,
        int hresult)
    {
        if (isDestroyed)
            return;


        Debug.LogWarning(
            "[SpeechTest] DICTATION ERROR: " +
            error +
            " | HRESULT: " +
            hresult
        );


        if (speechText != null)
        {
            speechText.text =
                "Speech Error:\n" +
                error;
        }
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    private void OnDictationComplete(
        DictationCompletionCause cause)
    {
        if (isDestroyed)
            return;


        if (showDebug)
        {
            Debug.LogWarning(
                "======================================"
            );

            Debug.LogWarning(
                "[SpeechTest] DICTATION COMPLETE"
            );

            Debug.LogWarning(
                "Cause: " +
                cause
            );

            Debug.LogWarning(
                "======================================"
            );
        }


        // -----------------------------------------------------
        // Tampilkan alasan berhenti
        // -----------------------------------------------------

        if (speechText != null)
        {
            speechText.text =
                "Recognition stopped:\n" +
                cause;
        }


        // -----------------------------------------------------
        // AUTO RESTART
        // -----------------------------------------------------

        if (autoRestart &&
            !isDestroyed)
        {
            StartCoroutine(
                RestartAfterComplete()
            );
        }
    }


    // =========================================================
    // RESTART
    // =========================================================

    private IEnumerator RestartAfterComplete()
    {
        if (isDestroyed)
            yield break;


        if (isStarting)
            yield break;


        // -----------------------------------------------------
        // Tunggu Windows melepaskan session
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            restartDelay
        );


        if (isDestroyed)
            yield break;


        if (showDebug)
        {
            Debug.Log(
                "[SpeechTest] Restarting speech..."
            );
        }


        yield return StartCoroutine(
            StartSpeech()
        );
    }


    // =========================================================
    // STOP + DISPOSE
    // =========================================================

    private IEnumerator StopAndDisposeRecognizer()
    {
        if (dictationRecognizer == null)
            yield break;


        if (isStopping)
            yield break;


        isStopping = true;


        // -----------------------------------------------------
        // Remove events
        // -----------------------------------------------------

        dictationRecognizer.DictationHypothesis -=
            OnDictationHypothesis;

        dictationRecognizer.DictationResult -=
            OnDictationResult;

        dictationRecognizer.DictationError -=
            OnDictationError;

        dictationRecognizer.DictationComplete -=
            OnDictationComplete;


        // -----------------------------------------------------
        // Stop
        // -----------------------------------------------------

        try
        {
            if (dictationRecognizer.Status ==
                SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
        }
        catch (System.Exception e)
        {
            if (showDebug)
            {
                Debug.LogWarning(
                    "[SpeechTest] Stop error: " +
                    e.Message
                );
            }
        }


        // -----------------------------------------------------
        // Tunggu Windows
        // -----------------------------------------------------

        yield return null;

        yield return new WaitForSeconds(
            0.2f
        );


        // -----------------------------------------------------
        // Dispose
        // -----------------------------------------------------

        try
        {
            dictationRecognizer.Dispose();
        }
        catch (System.Exception e)
        {
            if (showDebug)
            {
                Debug.LogWarning(
                    "[SpeechTest] Dispose error: " +
                    e.Message
                );
            }
        }


        dictationRecognizer = null;

        isStopping = false;
    }


    // =========================================================
    // MANUAL START
    // =========================================================

    public void StartSpeechManual()
    {
        if (isDestroyed)
            return;


        if (isStarting)
            return;


        StartCoroutine(
            StartSpeech()
        );
    }


    // =========================================================
    // MANUAL STOP
    // =========================================================

    public void StopSpeech()
    {
        autoRestart = false;


        if (dictationRecognizer == null)
            return;


        if (showDebug)
        {
            Debug.Log(
                "[SpeechTest] Manual stop."
            );
        }


        try
        {
            if (dictationRecognizer.Status ==
                SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                "[SpeechTest] Stop error: " +
                e.Message
            );
        }


        if (speechText != null)
        {
            speechText.text =
                "Speech stopped.";
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        isDestroyed = true;

        autoRestart = false;


        if (dictationRecognizer == null)
            return;


        // -----------------------------------------------------
        // Remove events
        // -----------------------------------------------------

        dictationRecognizer.DictationHypothesis -=
            OnDictationHypothesis;

        dictationRecognizer.DictationResult -=
            OnDictationResult;

        dictationRecognizer.DictationError -=
            OnDictationError;

        dictationRecognizer.DictationComplete -=
            OnDictationComplete;


        // -----------------------------------------------------
        // Stop
        // -----------------------------------------------------

        try
        {
            if (dictationRecognizer.Status ==
                SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
        }
        catch
        {
        }


        // -----------------------------------------------------
        // Dispose
        // -----------------------------------------------------

        try
        {
            dictationRecognizer.Dispose();
        }
        catch
        {
        }


        dictationRecognizer = null;
    }

#endif
}