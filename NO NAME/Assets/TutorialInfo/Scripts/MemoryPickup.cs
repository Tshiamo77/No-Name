using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // Required for the New Input System

public class MemoryPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private GameObject interactionUI; // The "Press E to Pick Up" panel/text
    [SerializeField] private Image crosshairImage; // Drag your crosshair UI Image here
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color targetCrosshairColor = Color.green;
    [SerializeField] private string itemName = "Memory Item"; // Name for your inventory

    private Camera playerCamera;
    private bool isPlayerLookingAtItem = false;

    private void Start()
    {
        playerCamera = Camera.main;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (crosshairImage != null)
            crosshairImage.color = defaultCrosshairColor;
    }

    private void Update()
    {
        CheckForInteractable();

        // New Input System check for pressing 'E'
        if (isPlayerLookingAtItem && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUpItem();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.transform == transform)
            {
                isPlayerLookingAtItem = true;

                if (crosshairImage != null)
                    crosshairImage.color = targetCrosshairColor;

                if (interactionUI != null)
                    interactionUI.SetActive(true);

                return;
            }
        }

        ResetInteractionState();
    }

    private void ResetInteractionState()
    {
        isPlayerLookingAtItem = false;

        if (crosshairImage != null)
            crosshairImage.color = defaultCrosshairColor;

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void PickUpItem()
    {
        Debug.Log($"{itemName} added to inventory!");

        // TODO: Hook up your actual inventory manager call here, for example:
        // InventoryManager.Instance.AddItem(itemName);
        // Or using FindFirstObjectByType if your inventory uses a manager script:
        // InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        // if (inventory != null) { inventory.AddItem(gameObject); }

        ResetInteractionState();

        // Instead of destroying the object, we deactivate it so it's hidden in the world
        // while it sits in your inventory until used/consumed.
        gameObject.SetActive(false);
    }
}

