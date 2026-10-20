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
                // Finds the script even if its GameObject or Parent starts inactive in the hierarchy
                _instance = FindFirstObjectByType<MemorySequenceManager>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    [Header("Player & Hold References")]
    public MonoBehaviour playerController;

    [Header("UI & Overlay References")]
    public GameObject memoryUIPanel;
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

    /// <summary>
    /// Starts the memory sequence cutscene when called by MemoryInteractable.
    /// </summary>
    public void StartMemorySequence(MonoBehaviour pickupObject, ScriptableObject memoryData)
    {
        // 1. Enable this GameObject and the parent UI panel
        gameObject.SetActive(true);

        if (memoryUIPanel != null)
        {
            memoryUIPanel.SetActive(true);
        }

        // 2. Freeze Player Movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 3. Play Animation Sequence
        if (memoryUIAnimator != null)
        {
            memoryUIAnimator.enabled = true;
            memoryUIAnimator.Play(0, -1, 0f);
        }
    }

    /// <summary>
    /// Called directly by the Animation Event at Frame 6 of your animation clip.
    /// </summary>
    public void OnSixthFrameReached()
    {
        StartCoroutine(ExecuteFrameSixSequence());
    }

    private IEnumerator ExecuteFrameSixSequence()
    {
        // 1. Turn off Animator & close the UI panel immediately
        if (memoryUIAnimator != null)
        {
            memoryUIAnimator.enabled = false;
        }

        if (memoryUIPanel != null)
        {
            memoryUIPanel.SetActive(false);
        }

        // 2. Unfreeze Player Movement immediately so the player can explore
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // 3. Display Subtitle Message
        if (subtitleTextUI != null)
        {
            subtitleTextUI.text = "Was that me and my family?";
            subtitleTextUI.gameObject.SetActive(true);
        }

        // 4. Hide Subtitle after 3 seconds
        yield return new WaitForSeconds(3.0f);
        if (subtitleTextUI != null)
        {
            subtitleTextUI.gameObject.SetActive(false);
        }
    }
}