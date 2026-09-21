using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID = "HouseKey";
    [SerializeField] private string workInProgressMessage = "Keep pressing E (Work in Progress)";


    [Header("Position in Hand (tweak these while playing to get it looking right)")]
    [SerializeField] private Vector3 handLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 handLocalEuler = Vector3.zero;

    private bool pickedUp = false;

    public string PromptMessage => workInProgressMessage;
    public string KeyID => keyID;
   

    public void Interact(Transform handSlot)
    {
        if (pickedUp) return;

        if (handSlot == null)
        {
            Debug.LogError("KeyItem: no hand slot assigned! Drag KeyHoldPoint into the 'Key Hold Point' field on FPController.");
            return;
        }

        pickedUp = true;

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.SetParent(handSlot);
        transform.localPosition = handLocalPosition;
        transform.localRotation = Quaternion.Euler(handLocalEuler);

        KeyManager.PickUpKey(gameObject);
        Debug.Log($"Key '{keyID}' picked up and attached to hand.");
    }
}