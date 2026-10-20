using System.Collections.Generic;
using UnityEngine;

public class HouseInventoryManager : MonoBehaviour
{
    private List<CollectibleObjectData> collectedPieces = new List<CollectibleObjectData>();

    public List<CollectibleObjectData> CollectedPieces => collectedPieces;

    public void AddPiece(CollectibleObject piece)
    {
        CollectibleObjectData data = new CollectibleObjectData
        {
            objectID = piece.ObjectID,
            itemName = piece.ItemName,
            icon = piece.ItemIcon
        };

        collectedPieces.Add(data);
        Debug.Log($"Collected item: {piece.ItemName}. Total collected: {collectedPieces.Count}/3");
    }

    public bool HasAllPieces(int requiredAmount = 3)
    {
        return collectedPieces.Count >= requiredAmount;
    }
}

[System.Serializable]
public class CollectibleObjectData
{
    public string objectID;
    public string itemName;
    public Sprite icon;
}
