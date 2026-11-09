using UnityEngine;

public class MemoryInteractable : MonoBehaviour
{
    [Header("Prompt Settings")]
    public string PromptMessage = "Press E to view Memory";

    [Header("Memory Data Configuration")]
    public MemoryData memoryData;

    [Header("Final Memory Settings")]
    [Tooltip("Check this ONLY for the safe's final memory item to trigger the ending sequence.")]
    public bool isFinalMemory = false;

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

        // 1. Play the memory sequence
        MemorySequenceManager.Instance.StartMemorySequence(memoryData, isFinalMemory);

        // 2. Check and advance TaskManager depending on which task is active
        if (TaskManager.Instance != null)
        {
            if (TaskManager.Instance.CurrentTask == GameTask.PickUpPictureInLivingSpace)
            {
                TaskManager.Instance.CompleteTask(GameTask.PickUpPictureInLivingSpace);
            }
            else if (TaskManager.Instance.CurrentTask == GameTask.PickUpTeddyBear)
            {
                TaskManager.Instance.CompleteTask(GameTask.PickUpTeddyBear);
            }
            else if (TaskManager.Instance.CurrentTask == GameTask.PickUpBookOnShelf)
            {
                TaskManager.Instance.CompleteTask(GameTask.PickUpBookOnShelf);
            }
            // Add your safe PIN task / final memory task check here if applicable
        }
    }
}