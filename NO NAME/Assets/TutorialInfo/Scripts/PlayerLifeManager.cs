using UnityEngine;
using TMPro; // Make sure this is included for TextMeshPro!

public class PlayerLifeManager : BaseLifeManager
{
    [Header("UI Reference")]
    public TextMeshProUGUI livesText; // Drag your LivesText here in the inspector

    private void Start()
    {
        UpdateLivesUI();
    }

    // Override the base LoseLife method to update UI and handle player effects
    public override void LoseLife()
    {
        base.LoseLife(); // This decreases the life count
        UpdateLivesUI(); // Refresh the screen text

        // TODO: Play player damage sound effect or screen flash here
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            // Assuming currentLives is public in your BaseLifeManager
            livesText.text = "Lives: " + currentLives;
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        Debug.Log("Player caught 3 times! Restarting game...");

        // Ensure cursor is visible/unlocked when restarting
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Restart the current active scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
