using UnityEngine;

public class DoorMovement : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 3f;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float scaleFactor = 4f;

    [Header("Enemy Integration")]
    [SerializeField] private Enemy targetEnemy; // Drag your Enemy object here

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool invasionTriggered = false;

    public float MaxRange => interactionDistance * scaleFactor;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // When opened, trigger the enemy room invasion countdown (only once)
        if (isOpen && !invasionTriggered)
        {
            invasionTriggered = true;
            if (targetEnemy != null)
            {
                targetEnemy.TriggerRoomInvasion(this);
            }
        }
    }
}