using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "MAIN_HOUSE";
    [SerializeField] private GameObject settingsPanel;

    public static bool LoadSavedPosition = false;

    public void PlayGame()
    {
        LoadSavedPosition = false;

        PlayerPrefs.DeleteKey("NoName_PlayerPositionX");
        PlayerPrefs.DeleteKey("NoName_PlayerPositionY");
        PlayerPrefs.DeleteKey("NoName_PlayerPositionZ");
        PlayerPrefs.DeleteKey("NoName_PlayerRotationY");

        PlayerPrefs.Save();

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ContinueGame()
    {
        LoadSavedPosition = true;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}