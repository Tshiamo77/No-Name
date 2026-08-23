using UnityEngine;
using System.Collections.Generic;

public class GameInventoryManager : MonoBehaviour
{
    public static GameInventoryManager Instance;

    // Track collected items and memories using strings or IDs
    public HashSet<string> collectedItems = new HashSet<string>();
    public HashSet<int> collectedMemories = new HashSet<int>();

    private void Awake()
    {
        // Singleton pattern to keep this manager alive across scenes/actions
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
            Debug.Log($"<color=green>[INVENTORY] Picked up item: {itemName}</color>");
        }
    }

    public bool HasItem(string itemName)
    {
        return collectedItems.Contains(itemName);
    }

    public void CollectMemory(int memoryID)
    {
        if (!collectedMemories.Contains(memoryID))
        {
            collectedMemories.Add(memoryID);
            Debug.Log($"<color=cyan>[MEMORY] Unlocked Memory ID: {memoryID}</color>");
        }
    }

    public bool HasMemory(int memoryID)
    {
        return collectedMemories.Contains(memoryID);
    }
}

