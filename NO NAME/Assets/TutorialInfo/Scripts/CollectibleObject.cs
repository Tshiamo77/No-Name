using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [Header("Item Configuration")]
    public string ItemName = "Map Piece";
    public int PieceID; // Assign 1, 2, or 3 for each piece
    public Sprite UIInventoryIcon; // The icon image shown in the UI column

    public void Collect()
    {
        // Add item to inventory with its UI icon
        if (HouseInventoryManager.Instance != null)
        {
            HouseInventoryManager.Instance.AddMapPiece(PieceID, UIInventoryIcon);
        }

        // Hide 3D physical object from world
        gameObject.SetActive(false);
    }
}