using UnityEngine;

public class BookDrawing : MonoBehaviour
{
    [SerializeField] private GameObject drawingPanel;

    public void ShowDrawing()
    {
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideDrawing()
    {
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}