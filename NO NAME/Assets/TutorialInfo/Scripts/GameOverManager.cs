using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private RectTransform bloodImageRect;
    [SerializeField] private float dripDuration = 1.5f;
    [SerializeField] private string mainMenuSceneName = "MAIN_MENU";

    private static GameOverManager instance;
    public static GameOverManager Instance => instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (bloodImageRect != null)
        {
            bloodImageRect.gameObject.SetActive(true);

            // Ensure pivot is at the top center
            bloodImageRect.pivot = new Vector2(0.5f, 1f);

            // Scale Y from 0 (hidden at top) to 1 (fully stretched down)
            Vector3 localScale = bloodImageRect.localScale;
            localScale.y = 0f;
            bloodImageRect.localScale = localScale;

            float elapsed = 0f;
            while (elapsed < dripDuration)
            {
                float t = elapsed / dripDuration;
                float curvedT = Mathf.SmoothStep(0f, 1f, t);

                localScale.y = Mathf.Lerp(0f, 1f, curvedT);
                bloodImageRect.localScale = localScale;

                elapsed += Time.deltaTime;
                yield return null;
            }

            localScale.y = 1f;
            bloodImageRect.localScale = localScale;
        }

        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

