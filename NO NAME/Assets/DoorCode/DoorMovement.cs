using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class DoorMovement : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 3f;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float scaleFactor = 4f;

    [Header("Door Sounds")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.7f;

    private AudioSource audioSource;

    [Header("Key Lock Settings")]
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float messageDisplayTime = 4f;

    [Header("Puzzle Lock Settings")]
    [SerializeField] private bool lockedByPuzzle = false;
    [SerializeField]
    private string puzzleLockedMessage =
        "The lock has three colored slots... there must be a pattern somewhere in this house.";

    [Header("Enemy Integration")]
    [SerializeField] private Enemy targetEnemy;

    [Header("Hiding Spot Unlock")]
    [Tooltip("Optional: hiding spots unlocked the first time this door opens.")]
    [SerializeField] private HidingSpot[] linkedHidingSpots;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool invasionTriggered = false;
    private NavMeshObstacle navObstacle;

    public float MaxRange => interactionDistance * scaleFactor;

    void Start()
    {
        closedRotation = transform.rotation;

        openRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );

        navObstacle = GetComponent<NavMeshObstacle>();

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 0f;

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    // Called by FPController when the player interacts with the door.
    public void ToggleDoor()
    {
        // Puzzle lock: the door stays shut until the puzzle is solved.
        if (lockedByPuzzle)
        {
            ShowMessage(puzzleLockedMessage);
            return;
        }

        if (requiresKey)
        {
            if (KeyManager.HasKey)
            {
                requiresKey = false;
                Debug.Log("Door unlocked with key!");

                KeyManager.UseKey();

                ExecuteOpenSequence();
            }
            else
            {
                ShowMessage(
                    "What? It's locked, let's find a key it must be in here somewhere"
                );
            }
        }
        else
        {
            ExecuteOpenSequence();
        }
    }

    /// <summary>
    /// Called by ColorSortPuzzle when the correct order has been placed.
    /// </summary>
    public void UnlockFromPuzzle()
    {
        lockedByPuzzle = false;
        Debug.Log("Door unlocked by puzzle!");
    }

    private void ExecuteOpenSequence()
    {
        // Toggle the door state.
        isOpen = !isOpen;

        // Play the appropriate sound.
        AudioClip soundToPlay = isOpen ? openSound : closeSound;

        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay, soundVolume);
        }

        // Unlock linked hiding spots the first time the door opens.
        if (isOpen && linkedHidingSpots != null)
        {
            foreach (HidingSpot spot in linkedHidingSpots)
            {
                if (spot != null)
                {
                    spot.Unlock();
                }
            }
        }

        // Allow the enemy to pass through an open door.
        if (navObstacle != null)
        {
            navObstacle.carving = !isOpen;
        }

        // Trigger the enemy room invasion countdown only once.
        if (isOpen && !invasionTriggered)
        {
            invasionTriggered = true;

            if (targetEnemy != null)
            {
                targetEnemy.TriggerRoomInvasion(this);
            }
        }
    }

    private void ShowMessage(string message)
    {
        if (promptText != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayMessageRoutine(message));
        }
    }

    private IEnumerator DisplayMessageRoutine(string message)
    {
        promptText.gameObject.SetActive(true);
        promptText.text = message;

        yield return new WaitForSeconds(messageDisplayTime);

        promptText.gameObject.SetActive(false);
    }

    public void ResetDoorToClosed()
    {
        isOpen = false;
        invasionTriggered = false;

        transform.rotation = closedRotation;

        if (navObstacle != null)
        {
            navObstacle.carving = true;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}