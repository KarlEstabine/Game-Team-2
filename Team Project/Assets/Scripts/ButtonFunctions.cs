using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
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
}
