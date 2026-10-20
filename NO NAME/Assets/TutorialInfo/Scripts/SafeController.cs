using System.Collections;
using UnityEngine;
using TMPro;

public class SafeController : MonoBehaviour
{
    [Header("Safe Settings")]
    [SerializeField] private string correctCode = "4821"; // Target 4-digit code
    [SerializeField] private int codeLength = 4;

    [Header("UI & Display")]
    [SerializeField] private TextMeshPro screenText; // 3D TMP text on the safe door screen
    [SerializeField] private float autoClearDelay = 1.5f;

    [Header("Safe Door Opening Animation")]
    [SerializeField] private Transform safeDoorTransform;
    [SerializeField] private float openAngle = 110f;
    [SerializeField] private float openSpeed = 2f;

    public bool IsUnlocked { get; private set; }

    private string currentInput = "";
    private bool isProcessing = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        if (safeDoorTransform != null)
        {
            closedRotation = safeDoorTransform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        }

        UpdateDisplay("----");
    }

    public void EnterDigit(int digit)
    {
        if (IsUnlocked || isProcessing) return;

        currentInput += digit.ToString();
        UpdateDisplay(currentInput);

        // When max digits are entered, validate code
        if (currentInput.Length >= codeLength)
        {
            if (currentInput == correctCode)
            {
                StartCoroutine(SolveSequence());
            }
            else
            {
                StartCoroutine(WrongCodeSequence());
            }
        }
    }

    private IEnumerator SolveSequence()
    {
        isProcessing = true;
        UpdateDisplay("OPEN");
        IsUnlocked = true;

        yield return new WaitForSeconds(0.5f);

        // Smoothly rotate safe door open
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            if (safeDoorTransform != null)
            {
                safeDoorTransform.localRotation = Quaternion.Slerp(closedRotation, openRotation, elapsed);
            }
            yield return null;
        }

        isProcessing = false;
    }

    private IEnumerator WrongCodeSequence()
    {
        isProcessing = true;
        UpdateDisplay("FAIL");

        yield return new WaitForSeconds(autoClearDelay);

        currentInput = "";
        UpdateDisplay("----");
        isProcessing = false;
    }

    private void UpdateDisplay(string text)
    {
        if (screenText != null)
        {
            screenText.text = text;
        }
    }
}