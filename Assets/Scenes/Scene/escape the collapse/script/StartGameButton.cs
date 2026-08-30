
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    // =========================================================
    // SCENE ROULETTE
    // =========================================================

    [Header("Scene Roulette")]
    [Tooltip("SceneRouletteHandler used to select the first random level.")]
    public SceneRouletteHandler sceneRouletteHandler;


    // =========================================================
    // START GAME
    // =========================================================

    public void StartGame()
    {
        // Make sure SceneRouletteHandler is assigned
        if (sceneRouletteHandler == null)
        {
            Debug.LogError(
                "[StartGameButton] SceneRouletteHandler is not assigned!"
            );

            return;
        }


        // Start the global game timer
        if (GameTimeManager.Instance == null)
        {
            Debug.LogError(
                "[StartGameButton] GameTimeManager does not exist!"
            );

            return;
        }

        GameTimeManager.Instance.StartTimer();


        // Load the first random level
        sceneRouletteHandler.LoadRandomScene();
    }
}