using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int pieceID; // Set to 0, 1, or 2 to match target slot
    [SerializeField] private float snapDistance = 50f; // Snapping tolerance in pixels

    public int PieceID => pieceID;
    public bool IsSnapped { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition;
    private RectTransform targetSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void SetTargetSlot(RectTransform slot)
    {
        targetSlot = slot;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsSnapped) return;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsSnapped || canvas == null) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsSnapped) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check distance to designated target snap slot
        if (targetSlot != null)
        {
            float distance = Vector2.Distance(rectTransform.position, targetSlot.position);
            if (distance <= snapDistance)
            {
                // Snap to target position
                rectTransform.position = targetSlot.position;
                IsSnapped = true;

                // Notify puzzle controller
                PieceDoorPuzzle puzzle = FindFirstObjectByType<PieceDoorPuzzle>();
                if (puzzle != null)
                {
                    puzzle.OnPieceSnapped();
                }
                return;
            }
        }

        // Return to tray if missed target
        rectTransform.anchoredPosition = originalPosition;
    }

    public void ResetPiece()
    {
        IsSnapped = false;
        rectTransform.anchoredPosition = originalPosition;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}