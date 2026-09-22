using UnityEngine;

/// <summary>
/// Put this on the SAME object as your HoldableObject component.
/// It tells the puzzle which color this cube is, and can lock/unlock it in a slot.
/// </summary>
public class PuzzleCube : MonoBehaviour
{
    [SerializeField] private PuzzleColor colorId = PuzzleColor.Red;
    [Tooltip("Tints the cube automatically so the visible color always matches Color Id")]
    [SerializeField] private bool tintAutomatically = true;

    public PuzzleColor ColorId => colorId;
    public bool IsLocked { get; private set; }

    private Collider[] colliders;
    private Rigidbody rb;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();

        if (tintAutomatically)
        {
            colorId.ApplyTo(gameObject);
        }
    }

    /// <summary>Freeze the cube in a slot. Colliders are disabled so it can't be picked up again.</summary>
    public void LockAt(Vector3 position, Quaternion rotation)
    {
        IsLocked = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.SetParent(null); // make sure it is no longer parented to the hand
        transform.SetPositionAndRotation(position, rotation);

        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }
    }

    /// <summary>Give the cube back to physics and pop it out with a small impulse.</summary>
    public void Release(Vector3 position, Vector3 impulse)
    {
        IsLocked = false;

        transform.position = position;

        foreach (Collider c in colliders)
        {
            c.enabled = true;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
