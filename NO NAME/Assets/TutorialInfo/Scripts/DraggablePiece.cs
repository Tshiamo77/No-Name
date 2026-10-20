using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Piece Identification")]
    [SerializeField] private string pieceID = "MapPiece_1";

    [Header("Target Destination Slot")]
    [SerializeField] private RectTransform targetSlot;

    public string PieceID => pieceID;
    public bool IsLocked { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalParent = transform.parent;
    }

    public void SetHomePosition(RectTransform traySlot)
    {
        IsLocked = false;
        transform.SetParent(traySlot);
        originalParent = traySlot;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1.0f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsLocked) return;

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform);
        }

        canvasGroup.blocksRaycasts = false; // Allows seeing through to target slot underneath
        canvasGroup.alpha = 0.75f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsLocked) return;

        // Move directly using event delta to bypass canvas space projection issues
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsLocked) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // Check if dropped directly over any object named PieceIcon or assigned as targetSlot
        GameObject hoveredObj = eventData.pointerCurrentRaycast.gameObject;

        bool isCorrectDrop = false;
        if (hoveredObj != null)
        {
            if (targetSlot != null && (hoveredObj == targetSlot.gameObject || hoveredObj.transform.IsChildOf(targetSlot)))
            {
                isCorrectDrop = true;
            }
            else if (hoveredObj.name.StartsWith("PieceIcon", System.StringComparison.OrdinalIgnoreCase))
            {
                targetSlot = hoveredObj.GetComponent<RectTransform>();
                isCorrectDrop = true;
            }
        }

        if (isCorrectDrop)
        {
            PieceDoorPuzzle puzzleController = GetComponentInParent<PieceDoorPuzzle>();
            if (puzzleController != null)
            {
                puzzleController.OnPieceDroppedInBox(this);
            }
            else
            {
                ReturnToTray();
            }
        }
        else
        {
            ReturnToTray();
        }
    }

    public void LockInBox()
    {
        IsLocked = true;

        if (targetSlot != null)
        {
            transform.SetParent(targetSlot);
            rectTransform.anchoredPosition = Vector2.zero; // Snap to slot center

            TextMeshProUGUI slotText = targetSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (slotText != null)
            {
                slotText.gameObject.SetActive(false);
            }
        }
    }

    public void ReturnToTray()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }
}