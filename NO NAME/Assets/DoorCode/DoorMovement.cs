using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.AI;

public class DoorMovement : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 3f;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float scaleFactor = 4f;

    [Header("Key Lock Settings")]
    [SerializeField] private bool requiresKey = false; // Check this ONLY for the key-locked door!
    [SerializeField] private TextMeshProUGUI promptText; // Separate text object for lock messages
    [SerializeField] private float messageDisplayTime = 4f;

    [Header("Puzzle Lock Settings")]
    [SerializeField] private bool lockedByPuzzle = false; // Check this ONLY for the color puzzle door!
    [SerializeField] private string puzzleLockedMessage = "The lock has three colored slots... there must be a pattern somewhere in this house.";

    [Header("Enemy Integration")]
    [SerializeField] private Enemy targetEnemy; // Drag your Enemy object here

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool invasionTriggered = false;
    private NavMeshObstacle navObstacle;

    public float MaxRange => interactionDistance * scaleFactor;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        navObstacle = GetComponent<NavMeshObstacle>();

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    // Called by FPController when the player presses T while looking at the door
    public void ToggleDoor()
    {
        // Puzzle lock comes first: the door stays shut until the puzzle unlocks it
        if (lockedByPuzzle)
        {
            ShowMessage(puzzleLockedMessage);
            return;
        }

        if (requiresKey)
        {
            if (KeyManager.HasKey)
            {
                requiresKey = false; // Unlocked permanently
                Debug.Log("Door unlocked with key!");

                // Removes the key model from the hand and clears HasKey
                KeyManager.UseKey();

                ExecuteOpenSequence();
            }
            else
            {
                ShowMessage("What? It's locked, let's find a key it must be in here somewhere");
            }
        }
        else
        {
            ExecuteOpenSequence();
        }
    }

    /// <summary>Called by ColorSortPuzzle when the correct order has been placed.</summary>
    public void UnlockFromPuzzle()
    {
        lockedByPuzzle = false;
        Debug.Log("Door unlocked by puzzle!");
    }

    private void ExecuteOpenSequence()
    {
        isOpen = !isOpen;

        // Toggle NavMeshObstacle carving so the enemy can pass through open doors
        if (navObstacle != null)
        {
            navObstacle.carving = !isOpen;
        }

        // When opened, trigger the enemy room invasion countdown (only once)
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
    }
}