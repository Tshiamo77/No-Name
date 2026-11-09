using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PieceDoorPuzzle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject puzzleUIPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private MapSlot[] puzzleSlots; // Drag your 3 MapSlot UI objects here in Inspector

    [Header("Door Integration")]
    [SerializeField] private DoorMovement doorToUnlock;

    private bool isSolved = false;

    /// <summary>
    /// Opens the drag-and-drop puzzle UI if the player has collected all map pieces.
    /// </summary>
    public void OpenPuzzleUI()
    {
        if (isSolved) return;

        // Check if player has all pieces in inventory
        if (HouseInventoryManager.Instance != null && HouseInventoryManager.Instance.HasAllPieces())
        {
            if (puzzleUIPanel != null)
            {
                puzzleUIPanel.SetActive(true);
            }

            if (instructionText != null)
            {
                instructionText.text = "Drag the map pieces into their matching slots to restore the map.";
            }

            // Unlock mouse cursor for UI drag and drop
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Lock first-person movement while puzzle UI is open
            FPController player = FindFirstObjectByType<FPController>();
            if (player != null)
            {
                player.SetInteractionsLocked(true);
            }
        }
    }

    /// <summary>
    /// Call this every time a map piece is dropped into a slot (handled automatically by MapSlot.cs).
    /// </summary>
    public void CheckPuzzleCompletion()
    {
        if (isSolved) return;

        // Verify if all slots are filled correctly
        if (puzzleSlots != null && puzzleSlots.Length > 0)
        {
            foreach (MapSlot slot in puzzleSlots)
            {
                if (slot == null || !slot.IsFilled)
                {
                    return; // At least one slot is still empty
                }
            }
        }

        // All slots are filled — puzzle completed!
        StartCoroutine(PuzzleSolvedSequence());
    }

    private IEnumerator PuzzleSolvedSequence()
    {
        isSolved = true;

        if (instructionText != null)
        {
            instructionText.text = "Map restored! The bedroom door unlocks.";
        }

        yield return new WaitForSeconds(1.5f);

        ClosePuzzleUI();

        // Unlock the main bedroom door
        if (doorToUnlock != null)
        {
            doorToUnlock.UnlockFromPuzzle(); // Unlocks door lock condition
            doorToUnlock.ToggleDoor();       // Swings door open
        }

        // Advance Task 7 (Collect Map Pieces) -> Task 8 (Remember Safe PIN)
        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask == GameTask.CollectMapPiecesForMainBedroom)
        {
            TaskManager.Instance.CompleteTask(GameTask.CollectMapPiecesForMainBedroom);
            Debug.Log("[PieceDoorPuzzle] Task advanced to Task 8 (RememberSafePIN).");
        }
    }

    public void ClosePuzzleUI()
    {
        if (puzzleUIPanel != null)
        {
            puzzleUIPanel.SetActive(false);
        }

        // Re-lock mouse cursor back to First-Person control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player movement
        FPController player = FindFirstObjectByType<FPController>();
        if (player != null)
        {
            player.SetInteractionsLocked(false);
        }
    }
}