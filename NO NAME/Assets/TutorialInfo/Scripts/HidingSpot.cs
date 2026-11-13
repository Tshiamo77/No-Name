using System.Collections;
using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Positions")]
    public Transform insidePosition;
    [Tooltip("Camera/player position when peeking out so walls don't block the view. Falls back to insidePosition if empty.")]
    public Transform peekPosition;
    public Transform exitPosition;

    [Header("Prompt Settings")]
    public string promptMessage = "Press H to Hide";

    [Header("Unlock Settings")]
    [Tooltip("If false, FPController ignores this spot entirely: no prompt, no crosshair highlight, H does nothing.")]
    [SerializeField] private bool isUnlocked = false;

    [Header("Closet Door Settings (Optional - leave empty for non-door spots like beds)")]
    [Tooltip("Assign the hinge pivot Transform of the closet door.")]
    public Transform doorPivot;
    public float closedAngle = 0f;
    public float openAngle = 90f;      // Full swing when entering/exiting
    public float peekAngle = 15f;      // Slightly cracked open while hiding
    public float doorSwingSpeed = 6f;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip doorCreakClip;

    public bool IsUnlocked => isUnlocked;

    private FPController playerController;
    private float targetYAngle;
    private bool wasPlayerHidingHere = false;
    private Coroutine doorRoutine;

    private void Start()
    {
        targetYAngle = closedAngle;
        playerController = FindFirstObjectByType<FPController>();
    }

    private void Update()
    {
        if (doorPivot == null) return;

        // Smoothly rotate the closet door toward its current target angle
        Quaternion targetRot = Quaternion.Euler(0f, targetYAngle, 0f);
        doorPivot.localRotation = Quaternion.Slerp(
            doorPivot.localRotation,
            targetRot,
            Time.deltaTime * doorSwingSpeed
        );

        if (playerController == null) return;

        // Detect when the player enters or exits THIS specific hiding spot
        bool isHidingHereNow = playerController.isHiding && IsPlayerAtThisSpot();

        if (isHidingHereNow && !wasPlayerHidingHere)
        {
            wasPlayerHidingHere = true;
            if (doorRoutine != null) StopCoroutine(doorRoutine);
            doorRoutine = StartCoroutine(SwingOpenThenPeekRoutine());
        }
        else if (!isHidingHereNow && wasPlayerHidingHere)
        {
            wasPlayerHidingHere = false;
            if (doorRoutine != null) StopCoroutine(doorRoutine);
            doorRoutine = StartCoroutine(SwingOpenThenCloseRoutine());
        }
    }

    private bool IsPlayerAtThisSpot()
    {
        bool nearInside = insidePosition != null &&
            Vector3.Distance(playerController.transform.position, insidePosition.position) < 1.5f;

        bool nearPeek = peekPosition != null &&
            Vector3.Distance(playerController.transform.position, peekPosition.position) < 1.5f;

        return nearInside || nearPeek;
    }

    private IEnumerator SwingOpenThenPeekRoutine()
    {
        PlayCreakSound();
        targetYAngle = openAngle;      // Swing wide open as player steps in
        yield return new WaitForSeconds(0.45f);

        PlayCreakSound();
        targetYAngle = peekAngle;      // Settle slightly cracked open while hiding
    }

    private IEnumerator SwingOpenThenCloseRoutine()
    {
        PlayCreakSound();
        targetYAngle = openAngle;      // Swing wide open as player steps out
        yield return new WaitForSeconds(0.45f);

        PlayCreakSound();
        targetYAngle = closedAngle;    // Close completely behind them
    }

    private void PlayCreakSound()
    {
        if (audioSource != null && doorCreakClip != null)
        {
            audioSource.PlayOneShot(doorCreakClip);
        }
    }

    /// <summary>Called by DoorMovement the first time its linked door is opened.</summary>
    public void Unlock()
    {
        isUnlocked = true;
    }
}