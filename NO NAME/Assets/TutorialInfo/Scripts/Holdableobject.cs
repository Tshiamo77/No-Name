using UnityEngine;

public class HoldableObject : MonoBehaviour
{
    [SerializeField] private string pickupMessage = "Press E to pick up";
    [SerializeField] private string placeMessage = "Press E to place";

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
        IsHeld = true;

        rb.isKinematic = true;
        objectCollider.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Place()
    {
        IsHeld = false;

        transform.SetParent(null);

        rb.isKinematic = false;
        objectCollider.enabled = true;
    }
}