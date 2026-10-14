using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Put this on any trigger collider. Fires onPlayerEntered once the player walks into it.
/// Reusable for "player is back in the safe room" zones, "player went upstairs" zones, etc.
///
/// IMPORTANT: Unity only sends trigger events if at least one of the two colliders involved
/// has a Rigidbody. A CharacterController-based player doesn't carry one, so this component
/// adds a kinematic Rigidbody to itself automatically - otherwise OnTriggerEnter never fires,
/// even if the box is positioned correctly.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerZoneTrigger : MonoBehaviour
{
    [Tooltip("Must match the tag on your Player object (the one with the CharacterController).")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Logs every collider that touches this zone, even ones that don't match Player Tag. Useful for diagnosing why a trigger isn't firing.")]
    [SerializeField] private bool logDebug = true;

    public UnityEvent onPlayerEntered;

    private void Reset()
    {
        // Convenience defaults for a trigger zone
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void Awake()
    {
        // Safety net in case Reset() didn't run (e.g. the component was added some other way)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"PlayerZoneTrigger on '{gameObject.name}': its Collider has 'Is Trigger' unticked. Tick it in the Inspector or trigger events won't fire.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (logDebug)
        {
            Debug.Log($"[PlayerZoneTrigger: {gameObject.name}] Something entered: '{other.gameObject.name}' (tag: '{other.tag}')");
        }

        if (other.CompareTag(playerTag))
        {
            if (logDebug) Debug.Log($"[PlayerZoneTrigger: {gameObject.name}] Tag matched '{playerTag}', firing onPlayerEntered.");
            onPlayerEntered.Invoke();
        }
    }
}