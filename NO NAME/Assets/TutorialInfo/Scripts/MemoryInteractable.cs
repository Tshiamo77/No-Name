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

    [Header("Safe Lock Settings")]
    [Tooltip("Check this if this item is inside a closed safe and should not be interactable until opened.")]
    public bool startsLockedInSafe = false;

    // ADDED: Memory sound settings
    [Header("Memory Sound Settings")]
    [SerializeField] private AudioSource memoryAudioSource;
    [SerializeField] private AudioClip memoryStartSound;
    [SerializeField, Range(0f, 1f)] private float memorySoundVolume = 1f;

    private Collider itemCollider;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();

        if (startsLockedInSafe && itemCollider != null)
        {
            itemCollider.enabled = false;
        }
    }

    /// <summary>
    /// Call this from your Safe script or Safe Door UnityEvent when the safe unlocks/opens.
    /// </summary>
    public void UnlockFromSafe()
    {
        startsLockedInSafe = false;
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }

    public void Interact()
    {
        if (startsLockedInSafe)
            return;

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

        // ADDED: Play sound when memory starts
        PlayMemorySound();

        // ADDED: Start player voice 2 seconds before memory sound ends
        if (memoryStartSound != null)
        {
            MemorySequenceManager.Instance.PlayMemoryVoiceNearEnd(memoryData, memoryStartSound);
        }

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
        }
    }

    // ADDED: Plays the memory sound
    private void PlayMemorySound()
    {
        if (memoryAudioSource == null)
        {
            Debug.LogWarning("Memory AudioSource is not assigned on " + gameObject.name);
            return;
        }

        if (memoryStartSound == null)
        {
            Debug.LogWarning("Memory sound clip is not assigned on " + gameObject.name);
            return;
        }

        memoryAudioSource.PlayOneShot(memoryStartSound, memorySoundVolume);
    }
}