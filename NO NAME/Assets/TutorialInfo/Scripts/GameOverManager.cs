using UnityEngine;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("References")]
    [SerializeField] private GameObject mainCanvasUI;       // Your Main Menu UI Canvas (Start/Quit buttons)
    [SerializeField] private Camera menuCamera;             // The exterior Menu Camera
    [SerializeField] private GameObject playerObject;       // Your Player (FPController)
    [SerializeField] private GameObject gameOverPanel;      // Panel containing the blood drip effect/animation

    [Header("Timings")]
    [SerializeField] private float bloodDripDuration = 3.0f; // Adjust to match the length of your blood drip animation/effect

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // 1. Play the blood drip effect/panel immediately
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 2. Wait for the blood drip effect to finish playing
        yield return new WaitForSeconds(bloodDripDuration);

        // 3. Disable the player object completely
        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }

        // 4. Re-enable the exterior menu camera so it looks at the house/scene
        if (menuCamera != null)
        {
            menuCamera.gameObject.SetActive(true);
        }

        // 5. Hide the blood drip game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 6. Bring back the main menu UI (Start/End buttons) and unlock the cursor
        if (mainCanvasUI != null)
        {
            mainCanvasUI.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Over sequence complete: Returned to main menu view.");
    }
    // Add this inside GameOverManager.cs
    public void ResetEntireGameScene()
    {
        // 1. Reset all doors in the scene to closed and re-enable NavMesh obstacles
        DoorMovement[] allDoors = FindObjectsByType<DoorMovement>(FindObjectsSortMode.None);
        foreach (DoorMovement door in allDoors)
        {
            door.ResetDoorToClosed(); // Make sure your DoorMovement has a method to close and turn carving back on
        }

        // 2. Restart the Game Intro manager
        GameIntroManager introManager = FindFirstObjectByType<GameIntroManager>();
        if (introManager != null)
        {
            introManager.ResetAndReplayIntro();
        }

        // 3. Reset player lives and positioning
        PlayerLifeManager lifeManager = FindFirstObjectByType<PlayerLifeManager>();
        if (lifeManager != null)
        {
            lifeManager.ResetLives();
        }
    }

}