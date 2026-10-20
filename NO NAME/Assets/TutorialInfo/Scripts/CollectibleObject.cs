using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [SerializeField] private string objectID = "Piece_1";
    [SerializeField] private string itemName = "Door Piece";
    [SerializeField] private Sprite itemIcon;

    public string ObjectID => objectID;
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;

    private void Awake()
    {
        // Ensure the object has a collider so raycasts hit it
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    /// <summary>Called when the player presses E on this object.</summary>
    public void Collect()
    {
        HouseInventoryManager inventory = FindFirstObjectByType<HouseInventoryManager>();
        if (inventory != null)
        {
            inventory.AddPiece(this);
            Debug.Log($"Collected: {itemName}");
            gameObject.SetActive(false); // Hide item from scene once collected
        }
        else
        {
            Debug.LogError("HouseInventoryManager not found in scene!");
        }
    }
}