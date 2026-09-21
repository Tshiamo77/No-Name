using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mainCanvasUI;       // Your Main Menu UI Canvas
    [SerializeField] private Camera menuCamera;             // The exterior Menu Camera
    [SerializeField] private GameObject playerObject;       // Your Player (FPController)
    [SerializeField] private GameIntroManager introManager; // Drag your standalone GameIntroManager here

    private void Awake()
    {
        // 1. Ensure the menu and menu camera are active when the scene starts
        if (mainCanvasUI != null) mainCanvasUI.SetActive(true);
        if (menuCamera != null) menuCamera.gameObject.SetActive(true);

        // 2. Ensure the player starts disabled so they don't move or block the view
        if (playerObject != null) playerObject.SetActive(false);

        // 3. Unlock the mouse cursor so buttons can be clicked easily
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnPlayButtonPressed()
    {
        // 1. Hide the Main Menu UI
        if (mainCanvasUI != null)
        {
            mainCanvasUI.SetActive(false);
        }

        // 2. Turn off the Menu Camera
        if (menuCamera != null)
        {
            menuCamera.gameObject.SetActive(false);
        }

        // 3. Activate the Player (this turns on their camera and control scripts)
        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }

        // 4. Trigger the Intro Manager sequence (Camera sweep + thoughts)
        if (introManager != null)
        {
            introManager.BeginIntro();
        }
        else
        {
            Debug.LogWarning("GameIntroManager reference is missing on MainMenuController!");
        }
    }

    public void OnQuitButtonPressed()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}