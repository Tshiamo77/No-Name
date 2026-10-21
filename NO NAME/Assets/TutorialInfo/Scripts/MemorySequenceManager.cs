using System.Collections;
using UnityEngine;
using TMPro;

public class MemorySequenceManager : MonoBehaviour
{
    private static MemorySequenceManager _instance;

    public static MemorySequenceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MemorySequenceManager>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    [Header("Player Reference")]
    public MonoBehaviour playerController;

    [Header("UI Overlay References")]
    public GameObject memoryDisplayPanel;
    public Animator memoryUIAnimator;
    public TextMeshProUGUI subtitleTextUI;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void StartMemorySequence(MemoryData data)
    {
        if (data == null)
        {
            Debug.LogWarning("MemoryData is missing!");
            return;
        }

        // Ensure this GameObject is active before starting coroutine
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(RunMemorySequenceRoutine(data));
    }

    private IEnumerator RunMemorySequenceRoutine(MemoryData data)
    {
        // 1. Freeze Player Control during animation display
        if (playerController != null) playerController.enabled = false;

        if (subtitleTextUI != null) subtitleTextUI.gameObject.SetActive(false);

        // 2. Open Display Panel & Enable Animator
        if (memoryDisplayPanel != null) memoryDisplayPanel.SetActive(true);

        if (memoryUIAnimator != null)
        {
            memoryUIAnimator.gameObject.SetActive(true);
            memoryUIAnimator.enabled = true;

            if (data.memoryAnimationClip != null)
            {
                memoryUIAnimator.Play(data.memoryAnimationClip.name, 0, 0f);
            }
        }

        // 3. Play Animation for specified duration (8 seconds)
        yield return new WaitForSeconds(data.memoryDisplayDuration);

        // 4. Disable Animation UI immediately when timer ends
        if (memoryUIAnimator != null)
        {
            memoryUIAnimator.enabled = false;
            memoryUIAnimator.gameObject.SetActive(false);
        }

        // 5. Restore Player Control immediately after memory ends
        if (playerController != null) playerController.enabled = true;

        // 6. Display Subtitles sequentially while player plays
        if (subtitleTextUI != null)
        {
            subtitleTextUI.text = data.subtitleLine1;
            subtitleTextUI.gameObject.SetActive(true);
            yield return new WaitForSeconds(data.subtitleDisplayTime);

            subtitleTextUI.text = data.subtitleLine2;
            yield return new WaitForSeconds(data.subtitleDisplayTime);

            subtitleTextUI.gameObject.SetActive(false);
        }
    }
}