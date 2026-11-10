using System.Collections;
using TMPro;
using UnityEngine;

public class SafeBox : MonoBehaviour
{
    [Header("PIN Settings")]
    [SerializeField] private string correctPIN = "0716";
    [SerializeField] private TextMeshPro screenText;

    [Header("Safe Door Controller")]
    [SerializeField] private SafeDoorController safeDoorController;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip accessGrantedSound;
    [SerializeField] private AudioClip accessDeniedSound;

    private string currentInput = "";
    private bool isUnlocked = false;
    private bool isVerifying = false;

    private void Start()
    {
        UpdateDisplay();
    }

    public void EnterDigit(int digit)
    {
        if (isUnlocked || isVerifying) return;

        PlayClickSound();

        // Limit input to 4 digits
        if (currentInput.Length < 4)
        {
            currentInput += digit.ToString();
            UpdateDisplay();
        }
    }

    public void PressEnter()
    {
        if (isUnlocked || isVerifying) return;

        PlayClickSound();

        StartCoroutine(VerifyPIN());
    }

    public void PressCancel()
    {
        if (isUnlocked || isVerifying) return;

        PlayClickSound();

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
        isVerifying = true;

        yield return new WaitForSeconds(0.2f);

        if (currentInput == correctPIN)
        {
            isUnlocked = true;

            if (screenText != null)
            {
                screenText.text = "<color=green>OPEN</color>";
            }

            if (audioSource != null && accessGrantedSound != null)
            {
                audioSource.PlayOneShot(accessGrantedSound);
            }

            yield return new WaitForSeconds(0.4f);

            // Tell the separate script to open the safe door
            if (safeDoorController != null)
            {
                safeDoorController.OpenSafeDoor();
            }
            else
            {
                Debug.LogWarning("SafeDoorController is not assigned!");
            }

            // Complete the task
            if (TaskManager.Instance != null &&
                TaskManager.Instance.CurrentTask == GameTask.RememberSafePIN)
            {
                TaskManager.Instance.CompleteTask(GameTask.RememberSafePIN);
            }
        }
        else
        {
            if (screenText != null)
            {
                screenText.text = "<color=red>ERR</color>";
            }

            if (audioSource != null && accessDeniedSound != null)
            {
                audioSource.PlayOneShot(accessDeniedSound);
            }

            yield return new WaitForSeconds(1.0f);

            currentInput = "";
            UpdateDisplay();
        }

        isVerifying = false;
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
}