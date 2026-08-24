using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Connect this to your "Start Game" button OnClick event
    public void StartGame()
    {
        // Loads the next scene in your Build Settings (usually your gameplay scene)
        // Alternatively, you can use SceneManager.LoadScene("YourSceneNameHere");
        SceneManager.LoadScene("MAIN_HOUSE");
    }

    // Connect this to your "Close / Quit" button OnClick event
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
