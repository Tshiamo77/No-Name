using UnityEngine;

public class MemoryInteractable : MonoBehaviour
{
    [Header("Prompt Settings")]
    [Tooltip("Text to show when crosshair aims at this item.")]
    public string PromptMessage = "Press E to view Memory";

    [Header("Memory Configuration")]
    public MemoryData memoryData;

    public void Interact()
    {
        if (MemorySequenceManager.Instance == null)
        {
            Debug.LogError("MemorySequenceManager.Instance IS NULL! Make sure MemoryAnimatedImage is in the scene.");
            return;
        }

        if (memoryData == null)
        {
            Debug.LogWarning("No MemoryData assigned to " + gameObject.name);
            return;
        }

        MemorySequenceManager.Instance.StartMemorySequence(this, memoryData);
    }
}