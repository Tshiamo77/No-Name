
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MAIN_MENU";

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // NEW INPUT SYSTEM: detect the Escape key
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pausePanel == null)
        {
            Debug.LogError("PausePanel is not assigned!");
            return;
        }

        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveProgress()
    {
        PlayerPositionSave save =
            FindFirstObjectByType<PlayerPositionSave>();

        if (save != null)
        {
            save.SavePosition();
            Debug.Log("Player position saved.");
        }
        else
        {
            Debug.LogWarning(
                "PlayerPositionSave was not found in the scene.");
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        PlayerPositionSave save =
            FindFirstObjectByType<PlayerPositionSave>();

        if (save != null)
            save.SavePosition();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        PlayerPositionSave save =
            FindFirstObjectByType<PlayerPositionSave>();

        if (save != null)
            save.SavePosition();

        Application.Quit();
    }
}
