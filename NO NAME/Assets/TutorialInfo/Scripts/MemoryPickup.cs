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

    [Header("Memory Popup UI")]
    public GameObject memoryPanelUI; // Drag your MemoryDisplayPanel here in Inspector
    public TextMeshProUGUI memoryTextUI; // Drag the text inside the panel here
    [TextArea] public string memoryDescription = "A fading memory of a dark night...";

    private Camera playerCamera;
    private bool isPlayerLookingAtItem = false;

    private void Start()
    {

        Cursor.lockState= CursorLockMode.Locked;
        Cursor.visible= false;
        playerCamera = Camera.main;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (crosshairImage != null)
            crosshairImage.color = defaultCrosshairColor;

        if (memoryPanelUI != null)
            memoryPanelUI.SetActive(false);
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

    public void PickUpItem()
    {
        Debug.Log($"{itemName} added to inventory!");

        if (InventoryManager.Instance !=null)

        { 
            InventoryManager.Instance.AddMemory(itemName); 
        }

        // 1. Show the memory panel popup
        if (memoryPanelUI != null)
        {
            memoryPanelUI.SetActive(true);
        }

        if (memoryTextUI != null)
        {
            memoryTextUI.text = memoryDescription;
        }

        // 2. Pause game or unlock cursor so player can click the X button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Hide the world object so it's collected
        gameObject.SetActive(false);
        ResetInteractionState();
    }
}

