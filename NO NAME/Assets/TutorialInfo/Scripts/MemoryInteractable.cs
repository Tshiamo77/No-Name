using UnityEngine;

public class MemoryInteractable : MonoBehaviour
{
    [Header("Settings & Prompt")]
    public string PromptMessage = "Press E to view Memory";
    public ScriptableObject memoryData;

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Interact()
    {
        if (MemorySequenceManager.Instance == null)
        {
            Debug.LogError("MemorySequenceManager.Instance IS NULL! Make sure MemoryAnimatedImage is active in Hierarchy.");
            return;
        }

        MemorySequenceManager.Instance.StartMemorySequence(this, memoryData);
    }

    public void AttachToHand(Transform holdSocket)
    {
        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;

        transform.SetParent(holdSocket);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void DropObject()
    {
        transform.SetParent(null);
        if (rb != null) rb.isKinematic = false;
        if (col != null) col.enabled = true;
    }
}