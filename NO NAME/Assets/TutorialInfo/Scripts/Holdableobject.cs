using UnityEngine;

public class HoldableObject : MonoBehaviour
{
    [SerializeField] private string pickupMessage = "Press E to pick up";
    [SerializeField] private string placeMessage = "Press E to place";

    [Header("Optional Book Drawing")]
    [SerializeField] private BookDrawing bookDrawing;

    public string PickupMessage => pickupMessage;
    public string PlaceMessage => placeMessage;

    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private Collider objectCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
    }

    public void PickUp(Transform holdPoint)
    {
        if (holdPoint == null)
        {
            Debug.LogError("HoldPoint has not been assigned!", gameObject);
            return;
        }

        IsHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (bookDrawing != null)
        {
            bookDrawing.ShowDrawing();
        }

        Debug.Log("Picked up: " + gameObject.name);
    }

    public void Place()
    {
        IsHeld = false;

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
    }
}