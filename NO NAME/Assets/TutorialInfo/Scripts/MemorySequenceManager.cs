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

    [Header("UI & Overlay References")]
    public GameObject memoryUIPanel;
    public Animator memoryUIAnimator;
    public TextMeshProUGUI subtitleTextUI;

    [Header("Camera Reference")]
    public Transform mainCameraTransform;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void StartMemorySequence(MonoBehaviour pickupObject, MemoryData memoryData)
    {
        StartCoroutine(RunMemorySequenceRoutine(memoryData));
    }

    private IEnumerator RunMemorySequenceRoutine(MemoryData data)
    {
        // 1. Freeze Player Controls & Ensure Subtitles are Hidden initially
        if (playerController != null) playerController.enabled = false;
        if (subtitleTextUI != null) subtitleTextUI.gameObject.SetActive(false);

        // 2. Open UI & Start Animation
        if (memoryUIPanel != null) memoryUIPanel.SetActive(true);

        if (memoryUIAnimator != null)
        {
            memoryUIAnimator.enabled = true;
            memoryUIAnimator.Play(0, -1, 0f);
        }

        // 3. Wait for the animation duration specified in MemoryData (e.g., 8 seconds)
        float animDuration = (data != null) ? data.animationDuration : 8.0f;
        yield return new WaitForSeconds(animDuration);

        // 4. Close UI Panel immediately after animation ends
        if (memoryUIAnimator != null) memoryUIAnimator.enabled = false;
        if (memoryUIPanel != null) memoryUIPanel.SetActive(false);

        // 5. Spin the Camera for specified duration (e.g., 2 seconds)
        float spinTime = (data != null) ? data.spinDuration : 2.0f;
        float speed = (data != null) ? data.spinSpeed : 180.0f;
        yield return StartCoroutine(SpinCameraRoutine(spinTime, speed));

        // 6. Restore Player Control after spin completes
        if (playerController != null) playerController.enabled = true;

        // 7. Sequentially show Subtitles while player moves around freely
        if (data != null && subtitleTextUI != null)
        {
            // Primary Subtitle ("Was that me and my family?")
            subtitleTextUI.text = data.primarySubtitle;
            subtitleTextUI.gameObject.SetActive(true);
            yield return new WaitForSeconds(data.subtitleDisplayTime);

            // Secondary Subtitle ("There should be more clues around here...")
            subtitleTextUI.text = data.secondarySubtitle;
            yield return new WaitForSeconds(data.subtitleDisplayTime);

            // Hide Text
            subtitleTextUI.gameObject.SetActive(false);
        }
    }

    private IEnumerator SpinCameraRoutine(float duration, float speed)
    {
        if (mainCameraTransform == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float step = speed * Time.deltaTime;
            mainCameraTransform.Rotate(Vector3.up, step, Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}