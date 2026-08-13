using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MemoryPickup : MonoBehaviour
{
    [SerializeField] private string memorySceneName = "MEMORY_01";
    [SerializeField] private int memoryID = 1;

    [Header("Input System Setup")]
    [Tooltip("Bind this to the 'E' key in the Inspector")]
    public InputAction interactAction;

    private bool isPlayerNearby = false;

    private void OnEnable()
    {
        // This is crucial: Enable the action explicitly so it listens regardless of cursor state
        interactAction.Enable();

        // When 'E' is pressed, run the ExecutePickup method
        interactAction.performed += ExecutePickup;
    }

    private void OnDisable()
    {
        interactAction.Disable();
        interactAction.performed -= ExecutePickup;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("[NEAR MEMORY] Ready to interact.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    // This method fires exactly once the instant 'E' is pressed
    private void ExecutePickup(InputAction.CallbackContext context)
    {
        if (isPlayerNearby)
        {
            Debug.Log("[INPUT RECEIVED] Loading scene: " + memorySceneName);

            // Save progress
            PlayerPrefs.SetInt($"Memory_{memoryID}_Collected", 1);
            PlayerPrefs.Save();

            // Unlock the cursor before leaving the house, just in case the memory scene needs it
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(memorySceneName);
        }
    }

    private void OnGUI()
    {
        if (isPlayerNearby)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 100, 400, 50), "PRESS [ E ] TO PICK UP", style);
        }
    }
}


