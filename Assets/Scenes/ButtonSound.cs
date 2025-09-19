using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    [Header("UI Button")]
    public Button button; // The button to listen for clicks

    [Header("Sound Settings")]
    public AudioSource audioSource; // The AudioSource that will play the sound
    public AudioClip soundClip;     // The sound effect to play

    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
    }
}
