using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class TypewriterEffect : MonoBehaviour
{
    [Header("UI Text Target")]
    public Text uiText; // Text UI yang mau dipakai
    [TextArea]
    public string fullText; // Teks lengkap yang mau ditampilkan

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f; // Kecepatan (semakin kecil semakin cepat)

    private Coroutine typingCoroutine;

    void Start()
    {
        StartTyping();
    }

    // Mulai efek ketik
    public void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        uiText.text = "";
        foreach (char c in fullText)
        {
            uiText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
