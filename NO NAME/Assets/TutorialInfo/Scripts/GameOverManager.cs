using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("References")]
    [SerializeField] private GameObject gameOverPanel;  // Panel containing the blood drip effect/animation
    [SerializeField] private GameObject playerObject;   // Your Player (frozen while the blood drips)

    [Header("Timings")]
    [SerializeField] private float bloodDripDuration = 3.0f; // Match the length of your blood drip animation
    [SerializeField] private bool allowEnterToSkip = true;   // Press Enter to skip the wait

    private bool isRunning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Safe to call from several scripts (Enemy, PlayerLifeManager...).
    /// Only the first call starts the sequence; the rest are ignored.
    /// </summary>
    public void TriggerGameOver()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // 1. Freeze the player so they can't look around or walk during the blood drip
        if (playerObject != null)
        {
            FPController fp = playerObject.GetComponent<FPController>();
            if (fp != null) fp.enabled = false;
        }

        // 2. Show the blood drip and let it play
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        float timer = 0f;
        while (timer < bloodDripDuration)
        {
            if (allowEnterToSkip && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 3. Reload the current scene. MainMenuController.Awake then shows the menu,
        //    the menu camera and a disabled player, exactly like the first launch.
        Debug.Log("Game Over: reloading scene and returning to the main menu.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}