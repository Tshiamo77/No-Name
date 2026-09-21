using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerLifeManager : BaseLifeManager
{
    [Header("UI Reference")]
    public TextMeshProUGUI livesText; // Drag your LivesText here in the inspector

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverDelay = 3f;

    private bool isGameOver = false;

    private void Start()
    {
        UpdateLivesUI();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Allow player to press Enter via New Input System during Game Over to return immediately
        if (isGameOver && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            TriggerInSceneGameOverReset();
        }
    }

    // Override the base LoseLife method to update UI and handle player effects
    public override void LoseLife()
    {
        base.LoseLife(); // This decreases the life count
        UpdateLivesUI(); // Refresh the screen text
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }

    // Call this to restore player lives back to full
    public void ResetLives()
    {
        currentLives = maxLives; // Resets back to your base manager's max lives (e.g., 3)
        isGameOver = false;
        UpdateLivesUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        StartCoroutine(GameOverRoutine());
    }

    private System.Collections.IEnumerator GameOverRoutine()
    {
        isGameOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Find and turn on the Game Over Panel if not assigned directly
        if (gameOverPanel == null)
        {
            GameObject foundPanel = GameObject.Find("GameOverPanel");
            if (foundPanel != null) gameOverPanel = foundPanel;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Wait for the designated delay so the user can see the pop-up
        yield return new WaitForSeconds(gameOverDelay);

        // Trigger the in-scene game over sequence instead of loading a scene
        TriggerInSceneGameOverReset();
    }

    private void TriggerInSceneGameOverReset()
    {
        // Reset lives back to full
        ResetLives();

        // Use GameOverManager to toggle back to the exterior menu view and menu canvas
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager instance not found in scene!");
        }
    }
}