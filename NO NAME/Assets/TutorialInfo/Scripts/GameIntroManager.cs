using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class GameIntroManager : MonoBehaviour
{
    [Header("UI & References")]
    [SerializeField] private TextMeshProUGUI dialogueTextUI; // Drag your UI text element here
    [SerializeField] private FPController fpController; // Drag your Player object here
    [SerializeField] private Transform playerCameraTransform; // Drag your Player Camera here

    [Header("Settings")]
    [SerializeField] private float thoughtDisplayTime = 3.5f;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Lock player movement and look at start
        if (fpController != null) fpController.enabled = false;
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);

        // 2. Full camera sweep: Left -> Right -> Center
        if (playerCameraTransform != null)
        {
            yield return StartCoroutine(PerformCameraSweep());
        }

        // 3. Turn on dialogue UI and run through the opening thoughts
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);

        string[] introThoughts = {
            "Wonder what’s happening?",
            "Who am I...",
            "Where am I...",
            "I should find clues and find out what’s going on."
        };

        // 4. GIVE FULL CONTROL BACK TO PLAYER WHILE THOUGHTS PLAY
        if (fpController != null) fpController.enabled = true;

        foreach (string thought in introThoughts)
        {
            if (dialogueTextUI != null) dialogueTextUI.text = thought;
            yield return new WaitForSeconds(thoughtDisplayTime);
        }

        // 5. Hide text once thoughts finish
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
    }

    private IEnumerator PerformCameraSweep()
    {
        Quaternion startRot = playerCameraTransform.localRotation;

        // Define rotation offsets
        Quaternion lookLeft = startRot * Quaternion.Euler(0f, -70f, 0f);
        Quaternion lookRight = startRot * Quaternion.Euler(0f, 70f, 0f);

        // Phase 1: Pan from center to Left (1 second)
        float elapsed = 0f;
        float duration = 1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerCameraTransform.localRotation = Quaternion.Slerp(startRot, lookLeft, elapsed / duration);
            yield return null;
        }

        // Phase 2: Pan from Left all the way across to Right (1.5 seconds)
        elapsed = 0f;
        duration = 1.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerCameraTransform.localRotation = Quaternion.Slerp(lookLeft, lookRight, elapsed / duration);
            yield return null;
        }

        // Phase 3: Settle back slightly to center/neutral view (1 second)
        elapsed = 0f;
        duration = 1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerCameraTransform.localRotation = Quaternion.Slerp(lookRight, startRot, elapsed / duration);
            yield return null;
        }
    }
}