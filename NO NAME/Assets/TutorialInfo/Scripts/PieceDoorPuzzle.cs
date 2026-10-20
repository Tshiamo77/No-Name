using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieceDoorPuzzle : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private DoorMovement doorMovement;
    [SerializeField] private int requiredPieceCount = 3;

    [Header("UI Assembly Panel")]
    [SerializeField] private GameObject puzzleUIPanel;
    [SerializeField] private RectTransform[] targetSnapSlots; // 3 target outline slots on the image
    [SerializeField] private DraggablePiece[] draggablePieces; // 3 UI draggable pieces
    [SerializeField] private TextMeshProUGUI statusText;

    private HouseInventoryManager inventory;
    private bool isSolved = false;

    private void Start()
    {
        inventory = FindFirstObjectByType<HouseInventoryManager>();

        if (puzzleUIPanel != null)
            puzzleUIPanel.SetActive(false);

        // Bind target snap locations to draggable pieces
        for (int i = 0; i < draggablePieces.Length; i++)
        {
            if (i < targetSnapSlots.Length && draggablePieces[i] != null)
            {
                draggablePieces[i].SetTargetSlot(targetSnapSlots[i]);
            }
        }
    }

    public void InteractWithDoor()
    {
        if (isSolved) return;

        if (inventory == null) inventory = FindFirstObjectByType<HouseInventoryManager>();

        if (inventory != null && inventory.HasAllPieces(requiredPieceCount))
        {
            OpenPuzzleUI();
        }
    }

    private void OpenPuzzleUI()
    {
        if (puzzleUIPanel == null) return;

        puzzleUIPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowStatusMessage("Drag and arrange the pieces to complete the lock.");
    }

    /// <summary>Called automatically whenever a piece snaps into place.</summary>
    public void OnPieceSnapped()
    {
        int snappedCount = 0;
        foreach (DraggablePiece piece in draggablePieces)
        {
            if (piece != null && piece.IsSnapped) snappedCount++;
        }

        ShowStatusMessage($"Placed {snappedCount}/{requiredPieceCount} pieces.");

        if (snappedCount >= requiredPieceCount)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        isSolved = true;
        ShowStatusMessage("Mechanism aligned! Opening door...");

        if (doorMovement != null)
        {
            doorMovement.UnlockFromPiecePuzzle();
        }

        StartCoroutine(SolveAndOpenSequence());
    }

    private IEnumerator SolveAndOpenSequence()
    {
        yield return new WaitForSeconds(1.2f);

        // Close UI and return mouse control to player
        if (puzzleUIPanel != null)
            puzzleUIPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Open door automatically
        if (doorMovement != null)
        {
            doorMovement.ToggleDoor();
        }
    }

    private void ShowStatusMessage(string msg)
    {
        if (statusText != null)
        {
            statusText.text = msg;
            statusText.gameObject.SetActive(true);
        }
    }
}