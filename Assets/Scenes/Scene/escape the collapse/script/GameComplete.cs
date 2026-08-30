using UnityEngine;
using UnityEngine.SceneManagement;

public class GameComplete : MonoBehaviour
{
    public string resultScene = "Result";

    public void CompleteGame()
    {
        GameTimeManager.Instance.StopTimer();

        SceneManager.LoadScene(resultScene);
    }
}