using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 90f;
    private float verticalRotation = 0f;

    [Header("Hiding Settings")]
    public bool isHiding = false;
    private HidingSpot currentHidingSpot;
    private Vector3 preHidePosition;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private float interactionRadius = 0.08f;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    [Header("Crosshair & UI")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color normalCrosshairColor = Color.white;
    [SerializeField] private Color interactiveCrosshairColor = Color.green;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isSprinting = false;

    private DoorMovement currentTargetDoor;
    private MemoryPickup currentTargetPickup;
    private SlidingDrawer currentTargetDrawer;
    private HoldableObject currentTargetHoldable;
    private KeyItem currentTargetKey;
    private PuzzleCube currentTargetCube;
    private HoldableObject heldObject;

    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform keyHoldPoint;

    private bool interactionsLocked = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleLook();

        if (isHiding)
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.text = "Press H to Unhide";
                interactionPromptText.gameObject.SetActive(true);
            }
            return;
        }

        HandleMovement();
        CheckForInteractions();

        // Detects left-click to select or swap puzzle cubes
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentTargetCube != null)
            {
                currentTargetCube.OnInteract();
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    public void SetInteractionsLocked(bool locked) => interactionsLocked = locked;

    public void ResetGravityVelocity()
    {
        velocity = Vector3.zero;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (interactionsLocked || !context.performed) return;

        if (heldObject != null)
        {
            heldObject.Place();
            heldObject = null;
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
            return;
        }

        // Pick up KeyItem
        if (currentTargetKey != null)
        {
            currentTargetKey.Interact(keyHoldPoint);
            currentTargetKey = null;
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
            return;
        }

        // Pick up general Holdable Object
        if (currentTargetHoldable != null && holdPoint != null)
        {
            heldObject = currentTargetHoldable;
            heldObject.PickUp(holdPoint);
            currentTargetHoldable = null;
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
            return;
        }

        // Memory Pickup
        if (currentTargetPickup != null)
        {
            currentTargetPickup.PickUpItem();
            currentTargetPickup = null;
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
        }
    }

    public void OnHide(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleHiding();
        }
    }

    public void OnOpenDoor(InputAction.CallbackContext context)
    {
        if (interactionsLocked) return;
        if (context.performed && currentTargetDoor != null)
        {
            currentTargetDoor.ToggleDoor();
        }
    }

    public void OnOpenDrawer(InputAction.CallbackContext context)
    {
        if (interactionsLocked) return;
        if (context.performed && currentTargetDrawer != null)
        {
            currentTargetDrawer.ToggleDrawer();
        }
    }

    public void HandleMovement()
    {
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseX);
    }

    private void CheckForInteractions()
    {
        if (interactionsLocked)
        {
            ClearTargets();
            if (crosshairImage != null) crosshairImage.color = normalCrosshairColor;
            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
            return;
        }

        Transform rayOrigin = cameraTransform != null ? cameraTransform : Camera.main.transform;
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        bool foundInteractable = false;
        ClearTargets();

        if (Physics.Raycast(ray, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide) ||
            Physics.SphereCast(ray, interactionRadius, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
        {
            // 1. Puzzle Cube
            PuzzleCube cube = hit.transform.GetComponentInParent<PuzzleCube>();
            if (cube != null)
            {
                foundInteractable = true;
                currentTargetCube = cube;
                if (interactionPromptText != null && !isHiding)
                {
                    // Shows "Click to select [Red] Cube"
                    interactionPromptText.text = $"Click to select [{cube.ColorId}] Cube";
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            // 2. Hiding Spot
            else if (hit.collider.GetComponentInParent<HidingSpot>() is HidingSpot spot && spot.IsUnlocked)
            {
                foundInteractable = true;
                currentHidingSpot = spot;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = "Press H to Hide";
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            // 3. Memory Pickup
            else if (hit.transform.GetComponentInParent<MemoryPickup>() is MemoryPickup pickup)
            {
                foundInteractable = true;
                currentTargetPickup = pickup;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = pickup.PromptMessage;
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            // 4. KeyItem
            else if (hit.transform.GetComponentInParent<KeyItem>() is KeyItem keyItem)
            {
                foundInteractable = true;
                currentTargetKey = keyItem;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = keyItem.PromptMessage;
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            // 5. General Holdable Object
            else if (hit.transform.GetComponentInParent<HoldableObject>() is HoldableObject holdable)
            {
                foundInteractable = true;
                currentTargetHoldable = holdable;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = holdable.PickupMessage;
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            // 6. Door
            else if (hit.transform.GetComponentInParent<DoorMovement>() is DoorMovement door)
            {
                foundInteractable = true;
                currentTargetDoor = door;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = "Press T to open door";
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            // 7. Sliding Drawer
            else if (hit.collider.GetComponentInParent<SlidingDrawer>() is SlidingDrawer drawer)
            {
                foundInteractable = true;
                currentTargetDrawer = drawer;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = "Press F to slide drawer";
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = foundInteractable ? interactiveCrosshairColor : normalCrosshairColor;
        }

        if (!foundInteractable && interactionPromptText != null && !isHiding)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void ClearTargets()
    {
        currentTargetDoor = null;
        currentHidingSpot = null;
        currentTargetPickup = null;
        currentTargetDrawer = null;
        currentTargetHoldable = null;
        currentTargetKey = null;
        currentTargetCube = null;
    }

    private void ToggleHiding()
    {
        if (!isHiding && currentHidingSpot != null)
        {
            isHiding = true;
            preHidePosition = transform.position;
            controller.enabled = false;
            transform.position = currentHidingSpot.insidePosition.position;
            transform.rotation = currentHidingSpot.insidePosition.rotation;
            controller.enabled = true;
        }
        else if (isHiding)
        {
            isHiding = false;
            controller.enabled = false;
            transform.position = (currentHidingSpot != null && currentHidingSpot.exitPosition != null)
                ? currentHidingSpot.exitPosition.position
                : preHidePosition;
            controller.enabled = true;

            if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
        }
    }
}