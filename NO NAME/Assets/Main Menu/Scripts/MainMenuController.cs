
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "MAIN_HOUSE";
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject noProgressMessage;
    [SerializeField] private float messageDuration = 3f;

    public static bool LoadSavedPosition = false;

    private Coroutine messageCoroutine;

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
        bool hasSavedPosition =
            PlayerPrefs.HasKey("NoName_PlayerPositionX") &&
            PlayerPrefs.HasKey("NoName_PlayerPositionY") &&
            PlayerPrefs.HasKey("NoName_PlayerPositionZ");

        if (!hasSavedPosition)
        {
            LoadSavedPosition = false;
            ShowNoProgressMessage();
            return;
        }

        LoadSavedPosition = true;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void ShowNoProgressMessage()
    {
        if (noProgressMessage == null)
        {
            Debug.LogWarning("No Progress Message has not been assigned.");
            return;
        }

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(DisplayMessage());
    }

    private IEnumerator DisplayMessage()
    {
        noProgressMessage.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        noProgressMessage.SetActive(false);
        messageCoroutine = null;
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
