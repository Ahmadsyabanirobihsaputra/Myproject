using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeOnTouch : MonoBehaviour
{
    [Tooltip("Name of the scene to load (must be added in Build Settings)")]
    public string sceneToLoad;

    [Tooltip("Drag the Player GameObject here")]
    public GameObject player;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == player)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}