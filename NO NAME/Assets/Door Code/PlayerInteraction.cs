using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerInteraction: Camera not found inside Player.");
        }
    }

    void Update()
    {
        if (playerCamera == null) return;

        // 1. Only process when E is pressed once this frame
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            // Shoot a long raycast out to find a door
            if (Physics.Raycast(ray, out hit, 100f))
            {
                // 2. Look for the ProBuilderDoor script on the hit object, 
                // or search upwards in its parents (fixes ProBuilder child object issues)
                ProBuilderDoor door = hit.transform.GetComponentInParent<ProBuilderDoor>();

                if (door != null)
                {
                    // 3. Calculate distance from player to the door
                    float distance = Vector3.Distance(transform.position, door.transform.position);

                    // 4. Check if player is close enough using that specific door's scale factor
                    if (distance <= door.MaxRange)
                    {
                        door.ToggleDoor();
                    }
                    else if (door.imTest)
                    {
                        Debug.Log($"Too far away from {door.gameObject.name}. Distance: {distance}, Max Allowed: {door.MaxRange}");
                    }
                }
            }
        }
    }
}