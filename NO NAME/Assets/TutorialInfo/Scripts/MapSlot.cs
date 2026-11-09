using UnityEngine;
using UnityEngine.EventSystems;

public class MapSlot : MonoBehaviour, IDropHandler
{
    public int requiredPieceID; // Must match the ID of the correct map piece
    public bool IsFilled { get; private set; } = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (IsFilled) return;

        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null)
        {
            MapDragPiece dragPiece = droppedObject.GetComponent<MapDragPiece>();
            if (dragPiece != null && dragPiece.pieceID == requiredPieceID)
            {
                dragPiece.LockIntoSlot(transform);
                IsFilled = true;
                Debug.Log($"[MapSlot] Piece {requiredPieceID} placed correctly!");

                // Check if puzzle overall is completed
                PieceDoorPuzzle puzzleManager = FindFirstObjectByType<PieceDoorPuzzle>();
                if (puzzleManager != null)
                {
                    puzzleManager.CheckPuzzleCompletion();
                }
            }
        }
    }
}