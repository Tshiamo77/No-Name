using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.AI;

public class LockedDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private TextMeshProUGUI promptText; // Drag your on-screen message text here
    [SerializeField] private float messageDisplayTime = 4f;

    private NavMeshObstacle navObstacle;
    private bool isOpen = false;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
    }

    public void InteractWithDoor()
    {
        if (isLocked)
        {
            // Check if player has the key
            bool hasKey = KeyManager.HasKey;

            if (hasKey)
            {
                isLocked = false;
                Debug.Log("Door unlocked with key!");

                // Find the key object currently in the player's hand and remove it
                Transform handSlot = GameObject.Find("KeyHoldPoint")?.transform;
                if (handSlot != null && handSlot.childCount > 0)
                {
                    Destroy(handSlot.GetChild(0).gameObject); // Removes held key model from hand
                }

                ToggleDoorOpen();
            }
            else
            {
                // Player tried to open it without a key, show your exact message prompt
                ShowLockedMessage();
            }
        }
        else
        {
            ToggleDoorOpen();
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

    private void ToggleDoorOpen()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            if (navObstacle != null) navObstacle.carving = false; // Let enemy pass
            Debug.Log("Door opened.");
        }
        else
        {
            if (navObstacle != null) navObstacle.carving = true; // Block enemy
            Debug.Log("Door closed.");
        }
    }
}

// Built-in static tracker so KeyManager is always available
public static class KeyManager
{
    public static bool HasKey = false;
}