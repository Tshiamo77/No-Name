using UnityEngine;

public class UIEnder : MonoBehaviour
{
    // Attach this to your Close Button (X) or call this method when closing UI
    public void CloseMemoryPanel(GameObject panelToClose)
    {
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }

        // Lock the mouse back to the center and hide it for first-person look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("<color=cyan>[UI] Memory closed, mouse re-locked.</color>");
    }
}

