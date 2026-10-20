using UnityEngine;

public class SafeButton : MonoBehaviour
{
    [SerializeField] private int digitValue = 0; // Set to 0-9 in the Inspector per button

    public int DigitValue => digitValue;

    /// <summary>Called when the player clicks this physical keypad button.</summary>
    public void OnPress()
    {
        SafeController safe = GetComponentInParent<SafeController>();
        if (safe != null)
        {
            safe.EnterDigit(digitValue);
        }
    }
}
