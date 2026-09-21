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
    [SerializeField] private bool requiresKey = false; // Check this ONLY for the special locked door!
    [SerializeField] private TextMeshProUGUI promptText; // Drag your warning text here (only needed on the locked door)
    [SerializeField] private float messageDisplayTime = 4f;

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
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    // Called when the player interacts with the door (e.g. presses 'T')
    public void ToggleDoor()
    {
        if (requiresKey)
        {
            // Check if the player has the key via the static tracker
            if (KeyManager.HasKey)
            {
                requiresKey = false; // Unlock it permanently for the rest of the run
                Debug.Log("Door unlocked with key!");

                // Remove the key model from the player's hand slot
                Transform handSlot = GameObject.Find("KeyHoldPoint")?.transform;
                if (handSlot != null && handSlot.childCount > 0)
                {
                    Destroy(handSlot.GetChild(0).gameObject);
                }

                ExecuteOpenSequence();
            }
            else
            {
                // Player tried to open it without a key, show your exact message prompt
                ShowLockedMessage();
            }
        }
        else
        {
            ExecuteOpenSequence();
        }
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

    private void ShowLockedMessage()
    {
        if (promptText != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayMessageRoutine());
        }
    }

    private IEnumerator DisplayMessageRoutine()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = "What? It's locked, let's find a key it must be in here somewhere";

        yield return new WaitForSeconds(messageDisplayTime);

        promptText.gameObject.SetActive(false);
    }

    public void ResetDoorToClosed()
    {
        isOpen = false;
        invasionTriggered = false;
        transform.rotation = closedRotation;

        // Re-enable NavMesh obstacle carving
        if (navObstacle != null)
        {
            navObstacle.carving = true;
        }
    }
}