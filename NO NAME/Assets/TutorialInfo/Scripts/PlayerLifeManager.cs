using UnityEngine;
using TMPro;

public class PlayerLifeManager : BaseLifeManager
{
    [Header("UI Reference")]
    public TextMeshProUGUI livesText; // Drag your LivesText here in the inspector

    private void Start()
    {
        UpdateLivesUI();
    }

    // Override the base LoseLife method to update UI and detect running out of lives
    public override void LoseLife()
    {
        base.LoseLife();  // This decreases the life count
        UpdateLivesUI();  // Refresh the screen text

        // Belt and braces: works even if the base class doesn't call OnDeath itself.
        // GameOverManager ignores repeat calls, so calling it twice is harmless.
        if (currentLives <= 0)
        {
            StartGameOver();
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        StartGameOver();
    }

    private void StartGameOver()
    {
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager instance not found in scene!");
        }
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives;
        }
    }

    // Kept in case other scripts still call it
    public void ResetLives()
    {
        currentLives = maxLives;
        UpdateLivesUI();
    }
}