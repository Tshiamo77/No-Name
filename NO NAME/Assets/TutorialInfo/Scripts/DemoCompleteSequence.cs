using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DemoCompleteSequence : MonoBehaviour
{
    [Header("End Demo UI References")]
    [SerializeField] private GameObject endDemoPanel; // Solid black UI panel overlay
    [SerializeField] private TextMeshProUGUI endDemoText; // Message text
    [SerializeField] private Image fadeImage; // Background Image on endDemoPanel
    [SerializeField] private float fadeDuration = 2.0f;
    [SerializeField] private float displayDuration = 4.0f;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MAIN_MENU";

    private bool isEnding = false;

    private void Awake()
    {
        if (endDemoPanel != null)
        {
            endDemoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Triggers the black screen overlay, display message, and transition to MAIN_MENU.
    /// </summary>
    public void TriggerEndDemo()
    {
        if (isEnding) return;
        StartCoroutine(EndDemoSequence());
    }

    private IEnumerator EndDemoSequence()
    {
        isEnding = true;

        // Unlock mouse cursor for menu transition
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (endDemoPanel != null)
        {
            endDemoPanel.SetActive(true);
        }

        if (endDemoText != null)
        {
            endDemoText.text = "Well, some secrets are better revealed later.\nWait for next release to see the final room.";
            endDemoText.alpha = 0f;
        }

        // Fade in black panel and text message
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
            }

            if (endDemoText != null)
            {
                endDemoText.alpha = alpha;
            }

            yield return null;
        }

        // Hold message on screen before switching scenes
        yield return new WaitForSeconds(displayDuration);

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}