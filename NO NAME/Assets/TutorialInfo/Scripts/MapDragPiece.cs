using UnityEngine;
using UnityEngine.EventSystems;

public class MapDragPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int pieceID; // Must match the ID on MapSlot (1, 2, or 3)

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private Vector3 originalPosition;
    private Transform originalParent;
    private bool isLocked = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        originalPosition = rectTransform.position;
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false; // Allow raycast to hit MapSlot behind image
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        if (parentCanvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // If not dropped on a valid slot, snap back to original position
        rectTransform.position = originalPosition;
    }

    public void LockIntoSlot(Transform slotTransform)
    {
        isLocked = true;
        transform.SetParent(slotTransform);
        rectTransform.anchoredPosition = Vector2.zero; // Snap cleanly center
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1.0f;
    }
}
