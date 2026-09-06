using KKSpeech;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using UIText = UnityEngine.UI.Text;
using Debug = UnityEngine.Debug;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

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
    [Tooltip("Pastikan GameObject Text UI di Drag & Drop ke slot ini di Inspector!")]
    public UIText feedbackText;

    [Header("Live Status (Inspector Monitoring)")]
    [Tooltip("Tanda centang jika mikrofon engine sedang aktif.")]
    public bool isListening = false;

    [Tooltip("Tanda centang jika Push To Talk sedang aktif.")]
    public bool isPushToTalkActive = false;

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
    public PTTMode pttMode = PTTMode.HoldToTalk;

    public enum PTTMode
    {
        HoldToTalk,
        Toggle
    }

    public KeyCode pushToTalkKey = KeyCode.V;
    public bool showPushToTalkStatus = true;

    private SpeechRecognizerListener listener;
    private float textOverrideTimer = 0f;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private KeywordRecognizer keywordRecognizer;

    private void InitPCSpeech()
    {
        if (keywordRecognizer != null)
        {
            if (keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();

            keywordRecognizer.Dispose();
            keywordRecognizer = null;
        }

        // Ambil kata kunci dan bersihkan spasi
        string[] keywords = voiceCommands
            .Where(cmd => !string.IsNullOrEmpty(cmd.triggerPhrase))
            .Select(cmd => cmd.triggerPhrase.Trim())
            .Distinct()
            .ToArray();

        if (keywords.Length == 0)
        {
            Debug.LogWarning("[VoiceMenuController] Tidak ada triggerPhrase yang diisi di Inspector!");
            return;
        }

        // Inisialisasi dengan ConfidenceLevel Low agar lebih sensitif
        keywordRecognizer = new KeywordRecognizer(keywords, ConfidenceLevel.Low);
        keywordRecognizer.OnPhraseRecognized += OnPCKeywordRecognized;

        try
        {
            keywordRecognizer.Start();
            isListening = true;
            if (showDebug)
                Debug.Log($"<color=cyan>[VoiceMenuController] KeywordRecognizer ACTIVE dengan {keywords.Length} kata kunci: {string.Join(", ", keywords)}</color>");
        }
        catch (System.Exception e)
        {
            isListening = false;
            Debug.LogError($"[VoiceMenuController] Gagal menjalankan KeywordRecognizer: {e.Message}");
        }
    }

    private void OnPCKeywordRecognized(PhraseRecognizedEventArgs args)
    {
        string text = args.text;
        ConfidenceLevel confidence = args.confidence;

        if (showDebug)
            Debug.Log($"[VoiceMenuController] Raw Keyword Detected: \"{text}\" (Confidence: {confidence})");

        // Gerbang Push To Talk: Hanya proses jika PTT sedang aktif
        if (!isPushToTalkActive)
        {
            if (showDebug)
                Debug.LogWarning($"[VoiceMenuController] Keyword \"{text}\" diabaikan karena Push-To-Talk mati/dilepas.");
            return;
        }

        QueueOnMainThread(() =>
        {
            OnSpeechResult(text);
        });
    }
#endif

    private readonly Queue<System.Action> mainThreadActions = new Queue<System.Action>();

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

        if (showDebug) Debug.Log($"[VoiceMenuController] Initializing in [{mode}] mode.");

        switch (mode)
        {
            case RecognitionMode.MobileSpeech:
                StartMobileSpeech();
                break;

            case RecognitionMode.PCSpeech:
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                InitPCSpeech();
                SetFeedbackText($"Press/Hold [{pushToTalkKey}] to speak", 2f);
#else
                SetFeedbackText("PC speech not supported on this platform.", 3f);
#endif
                break;

            default:
                SetFeedbackText("Speech recognition not supported.", 3f);
                break;
        }
    }

    void Update()
    {
        // -----------------------------------
        // LOGIKA INPUT PUSH TO TALK (FILTER GERBANG)
        // -----------------------------------
        if (pttMode == PTTMode.HoldToTalk)
        {
            isPushToTalkActive = Input.GetKey(pushToTalkKey);
        }
        else if (pttMode == PTTMode.Toggle)
        {
            if (Input.GetKeyDown(pushToTalkKey))
            {
                isPushToTalkActive = !isPushToTalkActive;
            }
        }

        // -----------------------------------
        // UPDATE TEXT UI FEEDBACK
        // -----------------------------------
        if (textOverrideTimer > 0)
        {
            textOverrideTimer -= Time.deltaTime;
        }
        else if (showPushToTalkStatus && feedbackText)
        {
            if (isPushToTalkActive)
            {
                feedbackText.text = "Listening... Speak now!";
            }
            else
            {
                feedbackText.text = (pttMode == PTTMode.HoldToTalk)
                    ? $"Hold [{pushToTalkKey}] to speak"
                    : $"Press [{pushToTalkKey}] to toggle voice";
            }
        }

        // -----------------------------------
        // EXECUTOR MAIN THREAD QUEUE
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

        listener.onFinalResults.RemoveListener(OnSpeechResult);
        listener.onFinalResults.AddListener(OnSpeechResult);

        listener.onErrorDuringRecording.RemoveListener(OnSpeechError);
        listener.onErrorDuringRecording.AddListener(OnSpeechError);

        listener.onErrorOnStartRecording.RemoveListener(OnSpeechError);
        listener.onErrorOnStartRecording.AddListener(OnSpeechError);

        SpeechRecognizer.RequestAccess();
        SpeechRecognizer.SetDetectionLanguage("en-US");
        SpeechRecognizer.StartRecording(true);
        isListening = true;
#endif
    }

    private void OnSpeechResult(string recognized)
    {
        if (string.IsNullOrEmpty(recognized)) return;

        if (showDebug)
            Debug.Log($"<color=white>[VoiceMenuController] PROCESSED VOICE:</color> \"{recognized}\"");

        SetFeedbackText($"Heard: {recognized}", 2.5f);
        CheckVoiceCommands(recognized);
    }

    private void CheckVoiceCommands(string recognized)
    {
        string cleanResult = recognized.ToLower().Trim();

        foreach (var cmd in voiceCommands)
        {
            if (cmd.targetButton == null || string.IsNullOrEmpty(cmd.triggerPhrase))
                continue;

            string targetPhrase = cmd.triggerPhrase.ToLower().Trim();

            if (cleanResult.Equals(targetPhrase) || cleanResult.Contains(targetPhrase))
            {
                if (showDebug)
                    Debug.Log($"<color=green>[VoiceMenuController] SUCCESS MATCH!</color> Phrase: \"{targetPhrase}\" -> Executing: {cmd.targetButton.name}");

                // Picu event Klik pada tombol UI
                cmd.targetButton.onClick.Invoke();
                SetFeedbackText($"TRIGGERED: {cmd.triggerPhrase.ToUpper()}!", 3f);
                return;
            }
        }

        if (showDebug)
            Debug.LogWarning($"[VoiceMenuController] Tidak ada pemicu cocok untuk ucapan: \"{cleanResult}\"");
    }

    private void OnSpeechError(string error)
    {
        Debug.LogWarning($"[VoiceMenuController] Speech Error: {error}");
        SetFeedbackText("Error: " + error, 3f);
    }

    private void SetFeedbackText(string text, float displayDuration)
    {
        if (feedbackText != null)
        {
            feedbackText.text = text;
            textOverrideTimer = displayDuration;
        }
    }

    void OnDestroy()
    {
        isListening = false;
#if UNITY_ANDROID || UNITY_IOS
        if (listener != null)
        {
            listener.onFinalResults.RemoveListener(OnSpeechResult);
            listener.onErrorDuringRecording.RemoveListener(OnSpeechError);
            listener.onErrorOnStartRecording.RemoveListener(OnSpeechError);
        }
        SpeechRecognizer.StopIfRecording();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (keywordRecognizer != null)
        {
            if (keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();

            keywordRecognizer.Dispose();
            keywordRecognizer = null;
        }
#endif
    }
}