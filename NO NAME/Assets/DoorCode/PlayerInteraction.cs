using UnityEngine;
using UnityEngine.InputSystem; // 1. Added namespace for the New Input System

public class PlayerInteraction : MonoBehaviour
{
    [Header("Walk Animation")]
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private float minWalkSpeed = 0.1f;
    [SerializeField] private float fadeTime = 0.15f;

    private Camera playerCamera;
    private Animation anim;
    private Vector3 lastPosition;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerInteraction: Camera not found inside Player.");
        }

        // Setup legacy Animation component for walking
        anim = GetComponentInChildren<Animation>();
        lastPosition = transform.position;

        if (anim == null)
        {
            Debug.LogError("PlayerInteraction: Animation component not found inside Player.");
        }
        else if (walkClip != null)
        {
            walkClip.wrapMode = WrapMode.Loop;
            anim.AddClip(walkClip, walkClip.name);
        }
        else
        {
            Debug.LogError("PlayerInteraction: Walk Clip is not assigned in the Inspector.");
        }
    }

    void Update()
    {
        HandleDoorInteraction();
        HandleWalkAnimation();
    }

    private void HandleDoorInteraction()
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

    private void HandleWalkAnimation()
    {
        if (anim == null || walkClip == null || Time.deltaTime <= 0f) return;

        Vector3 currentPos = transform.position;
        Vector3 horizontalMove = new Vector3(
            currentPos.x - lastPosition.x,
            0f,
            currentPos.z - lastPosition.z
        );
        float speed = horizontalMove.magnitude / Time.deltaTime;
        lastPosition = currentPos;

        if (speed > minWalkSpeed)
        {
            if (!anim.IsPlaying(walkClip.name))
            {
                anim.CrossFade(walkClip.name, fadeTime);
            }
        }
        else if (anim.IsPlaying(walkClip.name))
        {
            // Reset to frame 0 so the character doesn't freeze mid-stride
            anim[walkClip.name].time = 0f;
            anim.Sample();
            anim.Stop();
        }
    }
}