using UnityEngine;
using TMPro;

public class MemoryPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string itemName = "Memory Item"; // Name for your inventory
    [SerializeField] private string promptMessage = "Press E to Pick Up";

    [Header("Memory Popup UI")]
    public GameObject memoryPanelUI; // Drag your MemoryDisplayPanel here in Inspector
    public TextMeshProUGUI memoryTextUI; // Drag the text inside the panel here
    [TextArea] public string memoryDescription = "A fading memory of a dark night...";

    public string PromptMessage => promptMessage;

    private void Start()
    {
        if (memoryPanelUI != null)
            memoryPanelUI.SetActive(false);
    }

    // Called centrally by FPController when the player presses E while looking at this item
    public void PickUpItem()
    {
        Debug.Log($"{itemName} added to inventory!");

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddMemory(itemName);
        }

        // 1. Show the memory panel popup safely
        if (memoryPanelUI != null)
        {
            memoryPanelUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[PICKUP] MemoryPanelUI is not assigned in the Inspector!");
        }

        if (memoryTextUI != null)
        {
            memoryTextUI.text = memoryDescription;
        }

        // 2. Unlock cursor so player can click any UI close buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Hide the world object so it's collected (Guaranteed to run now)
        gameObject.SetActive(false);
    }
}