using UnityEngine;

public class DoorMovement : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public float interactionDistance = 5f;
    public float scaleFactor = 4f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;

    public bool imTest = false;

    // This property gives the player script easy access to this door's max range
    public float MaxRange => interactionDistance * scaleFactor;

    void Start()
    {
        closedRotation = transform.rotation;

        openRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );
    }

    void Update()
    {
        // Smoothly rotate the door based on its state
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    // This public method will be triggered safely by the player script
    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }

    void OnDrawGizmos()
    {
        if (imTest)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance * scaleFactor);
        }
    }
}