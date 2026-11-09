using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HouseInventoryManager : MonoBehaviour
{
    public static HouseInventoryManager Instance { get; private set; }

    [Header("Inventory Data")]
    public List<int> collectedPieceIDs = new List<int>();
    public int totalPiecesRequired = 3;

    [Header("UI Column Settings")]
    [SerializeField] private Transform inventoryColumnContainer; // UI Vertical Layout Group on screen corner
    [SerializeField] private GameObject inventoryIconPrefab;      // Prefab with an Image component

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void ClearInventoryUI()
    {
        // Clear collected data
        collectedPieceIDs.Clear();

        // Destroy spawned UI icons in the inventory column
        foreach (Transform child in inventoryColumnContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddMapPiece(int pieceID, Sprite pieceIcon)
    {
        if (!collectedPieceIDs.Contains(pieceID))
        {
            collectedPieceIDs.Add(pieceID);
            Debug.Log($"[Inventory] Map Piece {pieceID} collected!");

            // Spawn image icon in the screen corner column
            if (inventoryColumnContainer != null && inventoryIconPrefab != null)
            {
                GameObject newIcon = Instantiate(inventoryIconPrefab, inventoryColumnContainer);
                Image img = newIcon.GetComponent<Image>();
                if (img != null && pieceIcon != null)
                {
                    img.sprite = pieceIcon;
                }
            }
        }
    }

    public bool HasAllPieces()
    {
        return collectedPieceIDs.Count >= totalPiecesRequired;
    }
}