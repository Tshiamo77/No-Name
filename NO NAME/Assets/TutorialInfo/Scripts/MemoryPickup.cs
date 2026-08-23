using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryPickup : MonoBehaviour
{
    [Header("Scene Settings")]
    public string memorySceneName = "MEMORY_01";
    public int memoryID = 1;

    [Header("UI Feedback")]
    public GameObject interactionPromptUI; // Optional: drag your text pop-up here

    private bool isPlayerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionPromptUI != null) interactionPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
        }
    }

    // This method is called by your crosshair interaction script when you click or press 'E'
    public void Interact()
    {
        Debug.Log($"<color=cyan>[MEMORY PICKUP] Loading scene: {memorySceneName}</color>");

        // Save progress to PlayerPrefs so the game knows this memory was collected
        PlayerPrefs.SetInt($"Memory_{memoryID}_Collected", 1);
        PlayerPrefs.Save();

        // Check if scene exists in build settings before loading
        if (Application.CanStreamedLevelBeLoaded(memorySceneName))
        {
            SceneManager.LoadScene(memorySceneName);
        }
        else
        {
            Debug.LogError($"<color=red>[ERROR] Scene '{memorySceneName}' cannot be loaded! Check your Build Settings spelling.</color>");
        }
    }
}


