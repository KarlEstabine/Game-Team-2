using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void StartGame()
    {
        // Load the first level (Assumes "Level1" is the name of your first scene)
        if (GameManager.instance != null && GameManager.instance.levels.Length > 0)
        {
            // Use GameManager's levels array to get the first level
            SceneManager.LoadScene(GameManager.instance.levels[0]);
        }
        else
        {
            // Fall Back load a specific scene directly if GameManager doesn't handle levels
            SceneManager.LoadScene("Level1");
        }
    }

    public void Resume()
    {
        // Unpause the game via resume button
        GameManager.instance.StateUnpause();
    }

    public void Restart()
    {
        // Restart the level via restart button
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Unpause the game
        GameManager.instance.StateUnpause();
    }

    public void Quit()
    {
    #if UNITY_EDITOR
        // If in Unity Editor, stop playing the game
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        // If in a build, quit the application
        Application.Quit();
    #endif
    }

    // Load Next Level (called from the Next Level Menu)
    public void LoadNextLevel()
    {
        if (GameManager.instance != null)
        {
            Debug.Log("ButtonFunctions triggered LoadNextLevel.");
            GameManager.instance.LoadNextLevel(); // Delegate to GameManager
        }
        else
        {
            Debug.LogError("GameManager instance is null! Cannot load the next level.");
        }
    }

    // Return to the Main Menu (Pause Menu, Lose/Win Screen etc)
    public void LoadMainMenu()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadMainMenu();  // Use the GameManager to load the main menu
        }
    }
}
