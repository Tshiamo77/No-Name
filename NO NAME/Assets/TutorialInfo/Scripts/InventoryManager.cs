using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // Singleton pattern so any script can easily check inventory (e.g., InventoryManager.Instance)
    public static InventoryManager Instance;

    [Header("Inventory Data")]
    public List<string> collectedMemories = new List<string>();
    public List<string> collectedItems = new List<string>(); // e.g., "HouseKey"

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add a memory
    public void AddMemory(string memoryName)
    {
        if (!collectedMemories.Contains(memoryName))
        {
            collectedMemories.Add(memoryName);
            Debug.Log($"<color=cyan>[Inventory] Memory collected: {memoryName}</color>");
        }
    }

    // Method to add a physical item or key
    public void AddItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
            Debug.Log($"<color=cyan>[Inventory] Item collected: {itemName}</color>");
        }
    }

    // Check if the player has a specific key/item
    public bool HasItem(string itemName)
    {
        return collectedItems.Contains(itemName);
    }
}


