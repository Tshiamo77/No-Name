using UnityEngine;
using UnityEngine.InputSystem; // 1. Added namespace for the New Input System

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

        // 2. Updated to use the New Input System (Keyboard.current)
        bool interactPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // 1. Only process when E is pressed once this frame
        if (interactPressed)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            // Shoot a long raycast out to find a door
            if (Physics.Raycast(ray, out hit, 100f))
            {
                // 2. Look for the DoorMovement script on the hit object,
                // or search upwards in its parents (fixes ProBuilder child object issues)
                DoorMovement door = hit.transform.GetComponentInParent<DoorMovement>();

                if (door != null)
                {
                    // 3. Calculate distance from player to the door
                    float distance = Vector3.Distance(transform.position, door.transform.position);

                    // 4. Check if player is close enough using that specific door's scale factor
                    if (distance <= door.MaxRange)
                    {
                        door.ToggleDoor();
                    }
                    
                }
            }
        }
    }
}

