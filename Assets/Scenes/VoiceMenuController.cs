
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using KKSpeech;






#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif
using System.Collections;

public class VoiceMenuController : MonoBehaviour
{
    [System.Serializable]
    public class VoiceCommand
    {
        [Tooltip("The phrase to trigger this button (e.g. 'start', 'play game', 'exit').")]
        public string triggerPhrase;

        [Tooltip("Button that will be clicked when phrase is recognized.")]
        public Button targetButton;
    }

    [Header("Voice Commands")]
    public List<VoiceCommand> voiceCommands = new List<VoiceCommand>();

    [Header("UI Feedback")]
    public Text feedbackText;

    [Header("Debug Settings")]
    public bool showDebug = true;

    [Header("Platform Override")]
    public RecognitionMode recognitionMode = RecognitionMode.Auto;

    public enum RecognitionMode
    {
        Auto,
        MobileSpeech,
        PCSpeech
    }

    [Header("Push To Talk")]
    [Tooltip("Voice commands will only trigger while this key is held.")]
    public KeyCode pushToTalkKey = KeyCode.V;

    [Tooltip("Show Push-to-Talk status in the UI.")]
    public bool showPushToTalkStatus = true;

    private bool pushToTalkActive = false;

    private SpeechRecognizerListener listener;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    private DictationRecognizer dictationRecognizer;
    private bool isRestarting = false;

    private IEnumerator StartPCSpeechSafe()
    {
        // Wait until any existing dictation session is fully stopped
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();

                while (dictationRecognizer.Status == SpeechSystemStatus.Running)
                    yield return null;
            }

            dictationRecognizer.Dispose();
            dictationRecognizer = null;
        }

        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            if (showDebug)
                Debug.Log($"[VoiceMenuController] PC Recognized: {text} (Confidence: {confidence})");

            QueueOnMainThread(() =>
            {
                if (feedbackText)
                    feedbackText.text = text;

                OnSpeechResult(text);
            });
        };

        dictationRecognizer.DictationHypothesis += (text) =>
        {
            if (showDebug)
                Debug.Log($"[VoiceMenuController] Hypothesis: {text}");

            QueueOnMainThread(() =>
            {
                if (feedbackText)
                    feedbackText.text = text;

                OnSpeechHypothesis(text);
            });
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogWarning($"[VoiceMenuController] Dictation error: {error}");
            OnSpeechError(error);
        };

      dictationRecognizer.DictationComplete += (cause) =>
{
    if (showDebug)
        Debug.Log(
            "[VoiceMenuController] Dictation complete: " + cause
        );

    // Always restart the recognizer.
    // This keeps voice recognition continuously active.
    QueueOnMainThread(() =>
    {
        StartCoroutine(RestartDictationWithDelay());
    });
};

        try
        {
            dictationRecognizer.Start();

            if (feedbackText)
                feedbackText.text = "Listening (PC)...";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                "[VoiceMenuController] Failed to start dictation: " + e.Message);
        }
    }

    private IEnumerator RestartDictationWithDelay(float delay = 1f)
    {
        if (isRestarting)
            yield break;

        isRestarting = true;

        yield return new WaitForSeconds(delay);

      if (dictationRecognizer != null)
    {
        try
        {
            // Make sure the recognizer is not already running
            if (dictationRecognizer.Status != SpeechSystemStatus.Running)
            {
                dictationRecognizer.Start();

                if (feedbackText)
                {
                    if (pushToTalkActive)
                        feedbackText.text = "Listening...";
                    else
                        feedbackText.text =
                            $"Hold [{pushToTalkKey}] to speak";
                }

                if (showDebug)
                    Debug.Log(
                        "[VoiceMenuController] Dictation restarted."
                    );
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                "[VoiceMenuController] Failed to restart dictation: "
                + e.Message
            );
        }
    }

    isRestarting = false;
}

#endif

    private readonly Queue<System.Action> mainThreadActions =
        new Queue<System.Action>();

    private string lastHypothesis = "";

    void Awake()
    {
        // No singleton / DontDestroyOnLoad.
    }

    void Start()
    {
        RecognitionMode mode = recognitionMode;

        if (mode == RecognitionMode.Auto)
        {
#if UNITY_ANDROID || UNITY_IOS
            mode = RecognitionMode.MobileSpeech;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            mode = RecognitionMode.PCSpeech;
#else
            mode = RecognitionMode.PCSpeech;
#endif
        }

        if (showDebug)
            Debug.Log(
                $"[VoiceMenuController] Starting in {mode} mode");

        switch (mode)
        {
            case RecognitionMode.MobileSpeech:
                StartMobileSpeech();
                break;

            case RecognitionMode.PCSpeech:

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                StartCoroutine(StartPCSpeechSafe());
#else
                Debug.LogWarning(
                    "PC speech recognition is only available on Windows.");

                if (feedbackText)
                    feedbackText.text =
                        "PC speech not supported on this platform.";
#endif

                break;

            default:

                if (feedbackText)
                    feedbackText.text =
                        "Speech recognition not supported.";

                break;
        }
    }


void Update()
    {
        // -----------------------------------
        // TOGGLE PUSH TO TALK
        // -----------------------------------

        // Press V once = activate
        // Press V again = deactivate
        if (Input.GetKeyDown(pushToTalkKey))
        {
            pushToTalkActive = !pushToTalkActive;

            if (showDebug)
            {
                Debug.Log(
                    "[VoiceMenuController] Push To Talk: " +
                    (pushToTalkActive ? "ON" : "OFF")
                );
            }
        }

        // -----------------------------------
        // UI FEEDBACK
        // -----------------------------------

        if (showPushToTalkStatus && feedbackText)
        {
            if (pushToTalkActive)
            {
                feedbackText.text = "Voice Active";
            }
            else
            {
                feedbackText.text =
                    $"Press [{pushToTalkKey}] to speak";
            }
        }

        // -----------------------------------
        // MAIN THREAD ACTIONS
        // -----------------------------------

        while (mainThreadActions.Count > 0)
        {
            System.Action action = null;

            lock (mainThreadActions)
            {
                if (mainThreadActions.Count > 0)
                    action = mainThreadActions.Dequeue();
            }

            action?.Invoke();
        }
    }

    private void QueueOnMainThread(System.Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }


    // -------------------------
    // Mobile
    // -------------------------

    private void StartMobileSpeech()
    {
#if UNITY_ANDROID || UNITY_IOS

        if (listener == null)
        {
            listener = FindObjectOfType<SpeechRecognizerListener>();

            if (listener == null)
            {
                GameObject go =
                    new GameObject("SpeechRecognizerListener");

                listener =
                    go.AddComponent<SpeechRecognizerListener>();
            }
        }

        listener.onPartialResults.RemoveListener(OnSpeechHypothesis);
        listener.onPartialResults.AddListener(OnSpeechHypothesis);

        listener.onFinalResults.RemoveListener(OnSpeechResult);
        listener.onFinalResults.AddListener(OnSpeechResult);

        listener.onErrorDuringRecording.RemoveListener(OnSpeechError);
        listener.onErrorDuringRecording.AddListener(OnSpeechError);

        listener.onErrorOnStartRecording.RemoveListener(OnSpeechError);
        listener.onErrorOnStartRecording.AddListener(OnSpeechError);

        SpeechRecognizer.RequestAccess();

        SpeechRecognizer.SetDetectionLanguage("en-US");

        SpeechRecognizer.StartRecording(true);

        if (feedbackText)
            feedbackText.text =
                $"Hold [{pushToTalkKey}] to speak";

#else

        if (feedbackText)
            feedbackText.text =
                "KKSpeech only works on Android/iOS.";

#endif
    }

    private void OnSpeechHypothesis(string hypothesis)
    {
        if (string.IsNullOrEmpty(hypothesis))
            return;

        if (hypothesis == lastHypothesis)
            return;

        lastHypothesis = hypothesis;

        if (showDebug)
            Debug.Log(
                $"[VoiceMenuController] Hypothesis: {hypothesis}");

        // Only process voice commands while PTT is held
        if (!pushToTalkActive)
            return;

        if (feedbackText)
            feedbackText.text = hypothesis;

        CheckVoiceCommands(hypothesis);
    }

    private void OnSpeechResult(string recognized)
    {
        if (showDebug)
            Debug.Log(
                $"[VoiceMenuController] Final result: {recognized}");

        // Ignore the result if PTT is not being held
        if (!pushToTalkActive)
        {
            if (showDebug)
                Debug.Log(
                    "[VoiceMenuController] Voice ignored - PTT not active.");

            return;
        }

        CheckVoiceCommands(recognized);
    }

    private void CheckVoiceCommands(string recognized)
    {
        if (string.IsNullOrEmpty(recognized))
            return;

        recognized = recognized.ToLower();

        foreach (var cmd in voiceCommands)
        {
            if (cmd.targetButton == null)
                continue;

            if (recognized.Contains(
                cmd.triggerPhrase.ToLower()))
            {
                if (showDebug)
                {
                    Debug.Log(
                        $"Trigger matched! Invoking button: " +
                        $"{cmd.targetButton.name}");
                }

                cmd.targetButton.onClick.Invoke();

                if (feedbackText)
                    feedbackText.text =
                        $"Triggered: {cmd.triggerPhrase}";

                lastHypothesis = "";

                return;
            }
        }
    }

    private void OnSpeechError(string error)
    {
        Debug.LogWarning(
            $"[VoiceMenuController] Speech error: {error}");

        if (feedbackText)
            feedbackText.text = "Error: " + error;
    }

    void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS

        if (listener != null)
        {
            listener.onPartialResults.RemoveListener(
                OnSpeechHypothesis);

            listener.onFinalResults.RemoveListener(
                OnSpeechResult);

            listener.onErrorDuringRecording.RemoveListener(
                OnSpeechError);

            listener.onErrorOnStartRecording.RemoveListener(
                OnSpeechError);
        }

        SpeechRecognizer.StopIfRecording();

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        if (dictationRecognizer != null)
        {
            try
            {
                if (dictationRecognizer.Status ==
                    SpeechSystemStatus.Running)
                {
                    dictationRecognizer.Stop();
                }

                dictationRecognizer.Dispose();
                dictationRecognizer = null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(
                    "[VoiceMenuController] Error stopping dictation: " +
                    e.Message);
            }
        }

#endif
    }
}

