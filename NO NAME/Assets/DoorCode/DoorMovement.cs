using UnityEngine;
using UnityEngine.Events;
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

    [Header("Key Lock Settings")]
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float messageDisplayTime = 4f;

    [Header("Puzzle Lock Settings")]
    [SerializeField] private bool lockedByPuzzle = false;
    [SerializeField]
    private string puzzleLockedMessage =
        "Oh its locked, let me solve the puzzle first";

    [Header("Enemy Integration")]
    [SerializeField] private Enemy targetEnemy;

    [Header("Hiding Spot Unlock")]
    [Tooltip("Optional hiding spots unlocked when this door opens.")]
    [SerializeField] private HidingSpot[] linkedHidingSpots;

    [Header("Events")]
    [SerializeField] private UnityEvent onDoorOpened;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool invasionTriggered = false;
    private NavMeshObstacle navObstacle;
    private AudioSource audioSource;
    private bool doorSoundPlaying = false;

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
        audioSource.Stop();

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

        bool isMoving = Quaternion.Angle(
            transform.rotation, targetRotation
        ) > 0.5f;

        if (isMoving)
        {
            if (!doorSoundPlaying)
            {
                StartDoorSound();
            }
        }
        else
        {
            transform.rotation = targetRotation;
            StopDoorSound();
        }
    }

    private void StartDoorSound()
    {
        AudioClip sound = isOpen ? openSound : closeSound;

        if (sound == null || audioSource == null)
            return;

        audioSource.Stop();
        audioSource.clip = sound;
        audioSource.loop = true;
        audioSource.volume = soundVolume;
        audioSource.Play();

        doorSoundPlaying = true;
    }

    private void StopDoorSound()
    {
        if (audioSource != null && doorSoundPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }

        doorSoundPlaying = false;
    }

    public void ToggleDoor()
    {
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

    public void UnlockFromPuzzle()
    {
        lockedByPuzzle = false;
        Debug.Log("Door unlocked by puzzle!");
    }

    private void ExecuteOpenSequence()
    {
        isOpen = !isOpen;

        if (isOpen && linkedHidingSpots != null)
        {
            foreach (HidingSpot spot in linkedHidingSpots)
            {
                if (spot != null)
                    spot.Unlock();
            }
        }

        if (navObstacle != null)
        {
            navObstacle.carving = !isOpen;
        }

        if (isOpen && !invasionTriggered)
        {
            invasionTriggered = true;

            if (targetEnemy != null)
            {
                targetEnemy.TriggerRoomInvasion(this);
            }
        }

        if (isOpen)
        {
            onDoorOpened?.Invoke();
        }
    }

    public void ForceClose()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (navObstacle != null)
            navObstacle.carving = true;
    }

    public void ForceOpen()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (navObstacle != null)
            navObstacle.carving = false;
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
        StopDoorSound();

        if (navObstacle != null)
        {
            navObstacle.carving = true;
        }
    }
}