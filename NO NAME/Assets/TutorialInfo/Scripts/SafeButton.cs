using System.Collections;
using UnityEngine;

public enum ButtonType
{
    Digit,
    Enter,
    Cancel
}

public class SafeButton : MonoBehaviour
{
    [Header("Button Configuration")]
    public ButtonType buttonType = ButtonType.Digit;
    [SerializeField] public int DigitValue; // Only used if ButtonType is Digit (0-9)
    [SerializeField] private SafeBox parentSafe; // Drag SafeBox_Root here

    [Header("Press Animation Settings")]
    [SerializeField] private float pressDepth = 0.015f;
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private Vector3 pressDirection = new Vector3(0, 0, 1);

    private Vector3 initialLocalPos;
    private bool isPressing = false;

    private void Awake()
    {
        initialLocalPos = transform.localPosition;
    }

    public void OnPress()
    {
        if (isPressing) return;

        // Visual push animation
        StartCoroutine(PressRoutine());

        // Send interaction to SafeBox script based on button type
        if (parentSafe != null)
        {
            switch (buttonType)
            {
                case ButtonType.Digit:
                    parentSafe.EnterDigit(DigitValue);
                    break;
                case ButtonType.Enter:
                    parentSafe.PressEnter();
                    break;
                case ButtonType.Cancel:
                    parentSafe.PressCancel();
                    break;
            }
        }
    }

    private IEnumerator PressRoutine()
    {
        isPressing = true;
        Vector3 pressedPos = initialLocalPos + (pressDirection.normalized * pressDepth);

        // Push in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            transform.localPosition = Vector3.Lerp(initialLocalPos, pressedPos, t);
            yield return null;
        }

        // Return back out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            transform.localPosition = Vector3.Lerp(pressedPos, initialLocalPos, t);
            yield return null;
        }

        transform.localPosition = initialLocalPos;
        isPressing = false;
    }
}