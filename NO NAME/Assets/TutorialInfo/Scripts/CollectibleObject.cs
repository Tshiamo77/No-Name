using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [SerializeField] private string itemID = "MapPiece_1";
    [SerializeField] private string itemDisplayName = "Map Piece 1";

    public string ItemID => itemID;

    public void Collect()
    {
        HouseInventoryManager.Instance.AddItem(itemID, itemDisplayName);
        gameObject.SetActive(false); // Hide 3D mesh from world
    }
    public string ItemName => string.IsNullOrEmpty(itemDisplayName) ? itemID : itemDisplayName;
}