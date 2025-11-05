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
                if (feedbackText) feedbackText.text = text;
                OnSpeechResult(text);
            });
        };

        dictationRecognizer.DictationHypothesis += (text) =>
        {
            if (showDebug)
                Debug.Log($"[VoiceMenuController] Hypothesis: {text}");

            QueueOnMainThread(() =>
            {
                if (feedbackText) feedbackText.text = text;
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
                Debug.Log("[VoiceMenuController] Dictation complete: " + cause);

            if (cause != DictationCompletionCause.Complete)
                QueueOnMainThread(() => StartCoroutine(RestartDictationWithDelay()));
        };

        try
        {
            dictationRecognizer.Start();
            if (feedbackText) feedbackText.text = "Listening (PC)...";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[VoiceMenuController] Failed to start dictation: " + e.Message);
        }
    }

    private IEnumerator RestartDictationWithDelay(float delay = 1f)
    {
        if (isRestarting) yield break;
        isRestarting = true;

        yield return new WaitForSeconds(delay);

        if (dictationRecognizer != null && dictationRecognizer.Status != SpeechSystemStatus.Running)
        {
            try
            {
                dictationRecognizer.Start();
                if (feedbackText) feedbackText.text = "Listening (PC)...";
                if (showDebug) Debug.Log("[VoiceMenuController] Restarted dictation.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[VoiceMenuController] Failed to restart dictation: " + e.Message);
            }
        }

        isRestarting = false;
    }
#endif

    private readonly Queue<System.Action> mainThreadActions = new Queue<System.Action>();
    private string lastHypothesis = "";

    void Awake()
    {
        // Removed singleton and DontDestroyOnLoad to ensure this object is destroyed per scene
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
            Debug.Log($"[VoiceMenuController] Starting in {mode} mode");

        switch (mode)
        {
            case RecognitionMode.MobileSpeech:
                StartMobileSpeech();
                break;
            case RecognitionMode.PCSpeech:
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                StartCoroutine(StartPCSpeechSafe());
#else
                Debug.LogWarning("PC speech recognition is only available on Windows.");
                if (feedbackText) feedbackText.text = "PC speech not supported on this platform.";
#endif
                break;
            default:
                if (feedbackText) feedbackText.text = "Speech recognition not supported.";
                break;
        }
    }

    void Update()
    {
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

    // ------------------------- Mobile -------------------------
    private void StartMobileSpeech()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (listener == null)
        {
            listener = FindObjectOfType<SpeechRecognizerListener>();
            if (listener == null)
            {
                GameObject go = new GameObject("SpeechRecognizerListener");
                listener = go.AddComponent<SpeechRecognizerListener>();
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

        if (feedbackText) feedbackText.text = "Listening (Mobile)...";
#else
        if (feedbackText) feedbackText.text = "KKSpeech only works on Android/iOS.";
#endif
    }

    private void OnSpeechHypothesis(string hypothesis)
    {
        if (string.IsNullOrEmpty(hypothesis)) return;
        if (hypothesis == lastHypothesis) return;
        lastHypothesis = hypothesis;

        if (feedbackText) feedbackText.text = hypothesis;
        CheckVoiceCommands(hypothesis);
    }

    private void CheckVoiceCommands(string recognized)
    {
        recognized = recognized.ToLower();
        foreach (var cmd in voiceCommands)
        {
            if (recognized.Contains(cmd.triggerPhrase.ToLower()))
            {
                if (showDebug) Debug.Log($"Trigger matched! Invoking button: {cmd.targetButton.name}");
                cmd.targetButton.onClick.Invoke();
                if (feedbackText) feedbackText.text = $"Triggered: {cmd.triggerPhrase}";
                lastHypothesis = "";
                return;
            }
        }
    }

    private void OnSpeechResult(string recognized)
    {
        CheckVoiceCommands(recognized);
    }

    private void OnSpeechError(string error)
    {
        Debug.LogWarning($"[VoiceMenuController] Speech error: {error}");
        if (feedbackText) feedbackText.text = "Error: " + error;
    }

    void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (listener != null)
        {
            listener.onFinalResults.RemoveListener(OnSpeechResult);
            listener.onErrorDuringRecording.RemoveListener(OnSpeechError);
            listener.onErrorOnStartRecording.RemoveListener(OnSpeechError);
        }
        SpeechRecognizer.StopIfRecording();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (dictationRecognizer != null)
        {
            try
            {
                if (dictationRecognizer.Status == SpeechSystemStatus.Running)
                    dictationRecognizer.Stop();
                dictationRecognizer.Dispose();
                dictationRecognizer = null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[VoiceMenuController] Error stopping dictation: " + e.Message);
            }
        }
#endif
    }
}
