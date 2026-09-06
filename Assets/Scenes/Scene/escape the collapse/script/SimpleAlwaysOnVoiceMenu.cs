using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Alias eksplisit untuk menghindari konflik nama class
using UIText = UnityEngine.UI.Text;
using Debug = UnityEngine.Debug;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class SimpleAlwaysOnVoiceMenu : MonoBehaviour
{
    [System.Serializable]
    public class VoiceCommand
    {
        [Tooltip("Kata kunci pemicu (contoh: 'start', 'keluar', 'play').")]
        public string triggerPhrase;

        [Tooltip("Tombol UI yang akan diklik saat kata diucapkan.")]
        public Button targetButton;
    }

    [Header("Daftar Perintah Suara")]
    public List<VoiceCommand> voiceCommands = new List<VoiceCommand>();

    [Header("UI Feedback (Opsional)")]
    public UIText feedbackText;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private KeywordRecognizer keywordRecognizer;

    void Start()
    {
        InitKeywordRecognizer();
    }

    private void InitKeywordRecognizer()
    {
        // 1. Filter dan ambil semua kata kunci yang valid dari Inspector
        string[] keywords = voiceCommands
            .Where(cmd => !string.IsNullOrEmpty(cmd.triggerPhrase))
            .Select(cmd => cmd.triggerPhrase.Trim().ToLower())
            .Distinct()
            .ToArray();

        if (keywords.Length == 0)
        {
            Debug.LogWarning("[AlwaysOnVoice] Tidak ada kata kunci pemicu yang diisi di Inspector!");
            if (feedbackText) feedbackText.text = "Tidak ada perintah suara terdaftar.";
            return;
        }

        // 2. Inisialisasi KeywordRecognizer dengan ConfidenceLevel Low (lebih mudah mengenali ucapan)
        keywordRecognizer = new KeywordRecognizer(keywords, ConfidenceLevel.Low);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;

        // 3. Jalankan perekam suara secara langsung
        try
        {
            keywordRecognizer.Start();
            Debug.Log($"<color=green>[AlwaysOnVoice] Engine Aktif!</color> Mendengarkan kata: {string.Join(", ", keywords)}");
            
            if (feedbackText) 
                feedbackText.text = "Mendengarkan perintah suara...";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AlwaysOnVoice] Gagal menjalankan engine suara: {e.Message}");
            if (feedbackText) feedbackText.text = "Gagal mengaktifkan mikrofon.";
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        string spokenWord = args.text.ToLower().Trim();
        Debug.Log($"<color=yellow>[AlwaysOnVoice] Terdeteksi Suara:</color> \"{spokenWord}\" (Confidence: {args.confidence})");

        // Cari perintah yang cocok di daftar
        foreach (var cmd in voiceCommands)
        {
            if (cmd.targetButton == null || string.IsNullOrEmpty(cmd.triggerPhrase))
                continue;

            if (spokenWord.Equals(cmd.triggerPhrase.ToLower().Trim()))
            {
                Debug.Log($"<color=cyan>[AlwaysOnVoice] MATCH!</color> Memicu tombol: {cmd.targetButton.name}");

                // Jalankan fungsi onClick pada tombol UI
                cmd.targetButton.onClick.Invoke();

                if (feedbackText)
                    feedbackText.text = $"Pemicu Sukses: {cmd.triggerPhrase.ToUpper()}";

                return;
            }
        }
    }

    void OnDestroy()
    {
        // Bersihkan recognizer saat Scene ditutup atau GameObject dihancurkan
        if (keywordRecognizer != null)
        {
            if (keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();

            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Dispose();
            keywordRecognizer = null;
        }
    }
#else
    void Start()
    {
        Debug.LogWarning("[AlwaysOnVoice] Speech Recognition bawaan ini hanya didukung di Windows PC / Unity Editor Windows.");
        if (feedbackText) feedbackText.text = "Platform tidak didukung.";
    }
#endif
}