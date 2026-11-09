using System.Collections;
using UnityEngine;

public class DoorMapSocket : MonoBehaviour
{
    [Header("UI Frame Settings")]
    [SerializeField] private GameObject[] emptyFrames; // The 3 empty slot objects on the door

    [Header("Door Reference")]
    [SerializeField] private DoorMovement doorToUnlock;

    private bool isUnlocked = false;

    public void InteractWithDoor()
    {
        if (isUnlocked) return;

        // Check if player collected all 3 pieces
        if (HouseInventoryManager.Instance != null && HouseInventoryManager.Instance.HasAllPieces())
        {
            StartCoroutine(UnlockSequence());
        }
        else
        {
            int currentCount = HouseInventoryManager.Instance != null ? HouseInventoryManager.Instance.collectedPieceIDs.Count : 0;
            Debug.Log($"Door locked! You have {currentCount}/3 map pieces.");
        }
    }

    private IEnumerator UnlockSequence()
    {
        isUnlocked = true;

        // Turn on all 3 filled frames on the door panel sequentially
        foreach (GameObject frame in emptyFrames)
        {
            if (frame != null)
            {
                frame.SetActive(true);
                yield return new WaitForSeconds(0.4f); // Brief delay between each piece snapping in
            }
        }

        // Optional Touch: Clear icons from side inventory UI column
        if (HouseInventoryManager.Instance != null)
        {
            HouseInventoryManager.Instance.ClearInventoryUI();
        }

        yield return new WaitForSeconds(0.5f);

        // Unlock and open the bedroom door
        if (doorToUnlock != null)
        {
            doorToUnlock.UnlockFromPuzzle();
            doorToUnlock.ToggleDoor();
        }

        // Advance Task 7 -> Task 8 in TaskManager
        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask == GameTask.CollectMapPiecesForMainBedroom)
        {
            TaskManager.Instance.CompleteTask(GameTask.CollectMapPiecesForMainBedroom);
            Debug.Log("[DoorMapSocket] Task advanced to Task 8 (RememberSafePIN).");
        }
    }
}