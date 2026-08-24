using UnityEngine;
using System.Collections;
using TMPro;

public class GameIntroManager : MonoBehaviour
{
    [Header("UI & References")]
    [SerializeField] private TextMeshProUGUI dialogueTextUI; // Drag your UI text element here
    [SerializeField] private FPController fpController; // Drag your Player object here
    [SerializeField] private Transform playerCameraTransform; // Drag your Player Camera here

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Lock player movement and look at start
        if (fpController != null) fpController.enabled = false;

        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);

        // 2. Involuntary look around first
        yield return StartCoroutine(ForceCameraPan());

        // 3. Turn on dialogue UI and run through the opening thoughts
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);

        string[] introThoughts = {
"Wonder what’s happening?",
"Who am I...",
"Where am I...",
"I should find clues and find out what’s going on."
};

        foreach (string thought in introThoughts)
        {
            if (dialogueTextUI != null) dialogueTextUI.text = thought;
            yield return new WaitForSeconds(3.5f); // Duration each thought stays on screen
        }

        // 4. Hide text and give control back to player
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        if (fpController != null) fpController.enabled = true;
    }

    private IEnumerator ForceCameraPan()
    {
        float duration = 2f;
        float elapsed = 0f;
        Quaternion startRot = playerCameraTransform.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 60f, 0f); // Pans the camera right slightly

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerCameraTransform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            yield return null;
        }
    }
}

