using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Added for TextMeshPro reference

public class PieceDoorPuzzle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject puzzleUIPanel;
    [SerializeField] private TextMeshProUGUI instructionText; // Drag your UI header text here
    [SerializeField] private GameObject[] mapPieceDisplayImages;

    [Header("Display Settings")]
    [SerializeField] private float autoOpenDelay = 3.0f;

    [Header("Door & Demo Integration")]
    [SerializeField] private DoorMovement doorToUnlock;
    [SerializeField] private DemoCompleteSequence demoEndSequence;

    private bool isProcessing = false;

    public void InteractWithDoor()
    {
        OpenPuzzleUI();
    }

    public void OpenPuzzleUI()
    {
        if (isProcessing) return;

        if (HouseInventoryManager.Instance != null && HouseInventoryManager.Instance.HasAllPieces())
        {
            StartCoroutine(DisplayAndUnlockSequence());
        }
    }

    private IEnumerator DisplayAndUnlockSequence()
    {
        isProcessing = true;

        if (puzzleUIPanel != null)
        {
            puzzleUIPanel.SetActive(true);
        }

        // Set top instruction text dynamically
        if (instructionText != null)
        {
            instructionText.text = "We're supposed to have a drag and drop game here but its a work in progress";
        }

        foreach (var pieceImage in mapPieceDisplayImages)
        {
            if (pieceImage != null)
            {
                pieceImage.SetActive(true);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return new WaitForSeconds(autoOpenDelay);

        ClosePuzzleUI();

        if (doorToUnlock != null)
        {
            doorToUnlock.UnlockPuzzle();
        }

        if (demoEndSequence != null)
        {
            demoEndSequence.TriggerEndDemo();
        }

        isProcessing = false;
    }

    public void ClosePuzzleUI()
    {
        if (puzzleUIPanel != null)
        {
            puzzleUIPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnPieceDroppedInBox(DraggablePiece piece)
    {
        piece.ReturnToTray();
    }
}