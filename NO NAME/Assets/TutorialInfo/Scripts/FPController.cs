using UnityEngine;
using UnityEngine.InputSystem;

public class FPController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f; // For your Run mechanic
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float lookSensitivity = 2f;
    public float verticalLookLimit = 90f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isRunning = false;
    private bool isHiding = false; // Tracks the Hide state

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();

        // TODO: [ASSET COMPLETE] Add logic here to disable enemy detection when isHiding is true.
        // TODO: [ASSET COMPLETE] Implement raycast interaction for Open Doors when your door assets and scripts are ready.
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void ResetGravityVelocity()
    {
        velocity = Vector3.zero;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Linked to your 'Sprint' action in the Player Input component events
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed) isRunning = true;
        if (context.canceled) isRunning = false;
    }

    // Linked to your 'Hide' action once added to your Controls asset
    public void OnHide(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isHiding = !isHiding;
            Debug.Log($"[HIDE MECHANIC] Hiding state changed to: {isHiding}");

            // TODO: [ASSET COMPLETE] Toggle player model visibility, collision, or snap player position into a wardrobe/shadow mesh here.
        }
    }

    // Linked to your 'Interact/PickUp' action from your Controls asset
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("[INTERACT] Interaction button pressed.");

            // TODO: [ASSET COMPLETE] If you decide to use a central raycast system for opening doors or picking up items, trigger it here.
        }
    }

    public void HandleMovement()
    {
        if (isHiding) return;

        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleLook()
    {
        if (isHiding) return;

        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}