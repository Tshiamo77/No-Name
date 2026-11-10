using System.Collections;
using TMPro;
using UnityEngine;

public class SafeBox : MonoBehaviour
{
    [Header("PIN Settings")]
    [SerializeField] private string correctPIN = "0716";
    [SerializeField] private TextMeshPro screenText;

    [Header("Safe Door Hinge Pivot")]
    [SerializeField] private Transform safeDoorPivot;
    [SerializeField] private float openAngle = -110f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip accessGrantedSound;
    [SerializeField] private AudioClip accessDeniedSound;

    private string currentInput = "";
    private bool isUnlocked = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        if (safeDoorPivot != null)
        {
            closedRotation = safeDoorPivot.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        }

        UpdateDisplay();
    }

    public void EnterDigit(int digit)
    {
        if (isUnlocked) return;

        PlayClickSound();

        // Limit display to maximum 4 digits
        if (currentInput.Length < 4)
        {
            currentInput += digit.ToString();
            UpdateDisplay();
        }
    }

    public void PressEnter()
    {
        if (isUnlocked) return;

        PlayClickSound();

        // Submit the code when user presses Enter
        StartCoroutine(VerifyPIN());
    }

    public void PressCancel()
    {
        if (isUnlocked) return;

        PlayClickSound();

        // Clear the screen back to "----"
        currentInput = "";
        UpdateDisplay();
    }

    private void PlayClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private IEnumerator VerifyPIN()
    {
        yield return new WaitForSeconds(0.2f);

        if (currentInput == correctPIN)
        {
            isUnlocked = true;
            if (screenText != null) screenText.text = "<color=green>OPEN</color>";

            if (audioSource != null && accessGrantedSound != null)
            {
                audioSource.PlayOneShot(accessGrantedSound);
            }

            yield return new WaitForSeconds(0.4f);
            StartCoroutine(AnimateSafeDoorOpen());
        }
        else
        {
            if (screenText != null) screenText.text = "<color=red>ERR</color>";

            if (audioSource != null && accessDeniedSound != null)
            {
                audioSource.PlayOneShot(accessDeniedSound);
            }

            yield return new WaitForSeconds(1.0f);
            currentInput = "";
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (screenText == null || isUnlocked) return;

        if (string.IsNullOrEmpty(currentInput))
        {
            screenText.text = "----";
        }
        else
        {
            screenText.text = currentInput.PadRight(4, '-');
        }
    }

    private IEnumerator AnimateSafeDoorOpen()
    {
        float elapsedTime = 0f;

        // Calculate target rotation relative to starting local rotation
        Quaternion startRot = safeDoorPivot.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, openAngle, 0f);

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * openSpeed;
            safeDoorPivot.localRotation = Quaternion.Slerp(startRot, targetRot, elapsedTime);
            yield return null;
        }

        safeDoorPivot.localRotation = targetRot;
        Debug.Log("[SafeBox] Safe door wide open!");

        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask == GameTask.RememberSafePIN)
        {
            TaskManager.Instance.CompleteTask(GameTask.RememberSafePIN);
        }
    }
}