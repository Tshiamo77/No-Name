using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
        StartCoroutine(GameOverRoutine());
    }

    private System.Collections.IEnumerator GameOverRoutine()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Find and turn on the Game Over Panel
        GameObject panel = GameObject.Find("GameOverPanel"); // Replace with your exact UI panel name in the Hierarchy
        if (panel != null) panel.SetActive(true);

        // Wait for 3 seconds so the user can see the pop-up
        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
    }
}
