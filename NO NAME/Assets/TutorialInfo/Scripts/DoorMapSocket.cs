using System.Collections;
using UnityEngine;

public class DoorMapSocket : MonoBehaviour
{
    [Header("UI Frame Settings")]
    [SerializeField] private GameObject[] emptyFrames; // The 3 frame objects on the door

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

        // 1. Reveal each frame object on the door sequentially
        foreach (GameObject frame in emptyFrames)
        {
            if (frame != null)
            {
                frame.SetActive(true);

                // Ensure child mesh renderers are visible
                MeshRenderer renderer = frame.GetComponentInChildren<MeshRenderer>();
                if (renderer != null) renderer.enabled = true;

                yield return new WaitForSeconds(0.4f);
            }
        }

        // 2. Clear inventory icons from side UI column
        if (HouseInventoryManager.Instance != null)
        {
            HouseInventoryManager.Instance.ClearInventoryUI();
        }

        yield return new WaitForSeconds(0.5f);

        // 3. Unlock and toggle open the bedroom door
        if (doorToUnlock != null)
        {
            // Call the correct method for 3-piece map puzzle lock!
            doorToUnlock.UnlockFromPiecePuzzle();
            doorToUnlock.ToggleDoor();

            Debug.Log("[DoorMapSocket] Sent UnlockFromPiecePuzzle and ToggleDoor commands!");
        }

        // 4. Advance Task System
        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask == GameTask.CollectMapPiecesForMainBedroom)
        {
            TaskManager.Instance.CompleteTask(GameTask.CollectMapPiecesForMainBedroom);
            Debug.Log("[DoorMapSocket] Task advanced to Task 8 (RememberSafePIN).");
        }
    }

       
}