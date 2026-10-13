using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Positions")]
    public Transform insidePosition;
    public Transform exitPosition;

    [Header("Prompt Settings")]
    public string promptMessage = "Press E to Hide";

    [Header("Unlock Settings")]
    [Tooltip("If false, FPController ignores this spot entirely: no prompt, no crosshair highlight, H does nothing.")]
    [SerializeField] private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    /// <summary>Called by DoorMovement the first time its linked door is opened.</summary>
    public void Unlock()
    {
        isUnlocked = true;
    }
}