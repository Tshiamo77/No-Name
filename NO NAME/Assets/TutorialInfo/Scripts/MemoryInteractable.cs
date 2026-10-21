using UnityEngine;

public class MemoryInteractable : MonoBehaviour
{
    [Header("Prompt Settings")]
    public string PromptMessage = "Press E to view Memory";

    [Header("Memory Data Configuration")]
    public MemoryData memoryData;

    public void Interact()
    {
        if (MemorySequenceManager.Instance == null)
        {
            Debug.LogError("MemorySequenceManager Instance missing from scene!");
            return;
        }

        if (memoryData == null)
        {
            Debug.LogWarning("No MemoryData assigned to " + gameObject.name);
            return;
        }

        MemorySequenceManager.Instance.StartMemorySequence(memoryData);
    }
}