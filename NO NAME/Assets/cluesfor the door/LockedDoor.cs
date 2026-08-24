using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public float interactionDistance = 4f;

    public int cluesRequired = 3;

    public ClueManager clueManager;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen = false;
    private Transform player;

    void Start()
    {
        closedRotation = transform.rotation;

        openRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= interactionDistance &&
            Input.GetKeyDown(KeyCode.E))
        {
            if (clueManager.cluesCollected >= cluesRequired)
            {
                isOpen = !isOpen;

                Debug.Log("Door unlocked!");
            }
            else
            {
                Debug.Log(
                    "Door is locked. You need " +
                    cluesRequired + " clues."
                );
            }
        }

        Quaternion targetRotation =
            isOpen ? openRotation : closedRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }
}