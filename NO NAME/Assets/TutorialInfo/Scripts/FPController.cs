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
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    [Header("Crosshair & UI")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color normalCrosshairColor = Color.white;
    [SerializeField] private Color interactiveCrosshairColor = Color.green;

    [Header("Hiding Survival Settings")]
    [SerializeField] private TextMeshProUGUI warningPopupText; // Drag a UI text element in the corner here
    private Coroutine hidingTimerCoroutine;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isSprinting = false;

    private DoorMovement currentTargetDoor;
    private MemoryPickup currentTargetPickup;
    private SlidingDrawer currentTargetDrawer;

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
    }

    // --- NEW INPUT SYSTEM CALLBACKS ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    // Dedicated to Pickups / General Interactions (E Key)
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentTargetPickup != null)
        {
            float distance = Vector3.Distance(transform.position, currentTargetPickup.transform.position);
            if (distance <= interactionDistance)
            {
                currentTargetPickup.PickUpItem();
                currentTargetPickup = null;

                if (interactionPromptText != null)
                {
                    interactionPromptText.gameObject.SetActive(false);
                }
            }
        }
    }

    // Dedicated exclusively to Hiding / Unhiding (H Key)
    public void OnHide(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isHiding)
            {
                ToggleHiding(); // Unhide
            }
            else if (currentHidingSpot != null)
            {
                ToggleHiding(); // Hide
            }
        }
    }

    public void OnOpenDoor(InputAction.CallbackContext context)
    {
        if (context.performed && currentTargetDoor != null)
        {
            float distance = Vector3.Distance(transform.position, currentTargetDoor.transform.position);
            if (distance <= currentTargetDoor.MaxRange)
            {
                currentTargetDoor.ToggleDoor();
            }
        }
    }

    // Called automatically by Unity's New Input System when 'OpenDrawer' is triggered (e.g., pressing 'F')
    public void OnOpenDrawer(InputAction.CallbackContext context)
    {
        if (context.performed && currentTargetDrawer != null)
        {
            float distance = Vector3.Distance(transform.position, currentTargetDrawer.transform.position);
            if (distance <= interactionDistance)
            {
                currentTargetDrawer.ToggleDrawer();
            }
        }
    }


    // --- CORE MOVEMENT & LOOK METHODS ---

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

    public void ResetGravityVelocity()
    {
        velocity = Vector3.zero;
    }

    // --- INTERACTION & RAYCASTING ---

    private void CheckForInteractions()
    {
        if (cameraTransform == null && Camera.main == null) return;
        Transform rayOrigin = cameraTransform != null ? cameraTransform : Camera.main.transform;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        bool foundInteractable = false;
        currentTargetDoor = null;
        currentHidingSpot = null;
        currentTargetPickup = null;
        currentTargetDrawer = null;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 1. Check for Hiding Spot
            HidingSpot spot = hit.collider.GetComponentInParent<HidingSpot>();
            if (spot != null)
            {
                foundInteractable = true;
                currentHidingSpot = spot;
                if (interactionPromptText != null && !isHiding)
                {
                    interactionPromptText.text = "Press H to Hide";
                    interactionPromptText.gameObject.SetActive(true);
                }
            }

            // 2. Check for Memory Pickup
            MemoryPickup pickup = hit.transform.GetComponentInParent<MemoryPickup>();
            if (pickup != null)
            {
                float distance = Vector3.Distance(transform.position, pickup.transform.position);
                if (distance <= interactionDistance)
                {
                    foundInteractable = true;
                    currentTargetPickup = pickup;

                    if (interactionPromptText != null && !isHiding)
                    {
                        interactionPromptText.text = pickup.PromptMessage;
                        interactionPromptText.gameObject.SetActive(true);
                    }
                }
            }

            // 3. Check for Door
            DoorMovement door = hit.transform.GetComponentInParent<DoorMovement>();
            if (door != null)
            {
                float distance = Vector3.Distance(transform.position, door.transform.position);
                if (distance <= door.MaxRange)
                {
                    foundInteractable = true;
                    currentTargetDoor = door;

                    if (interactionPromptText != null && !isHiding)
                    {
                        interactionPromptText.text = "Press T to open door";
                        interactionPromptText.gameObject.SetActive(true);
                    }
                }
            }

            // 4. Check for Sliding Drawer
            SlidingDrawer drawer = hit.collider.GetComponentInParent<SlidingDrawer>();
            if (drawer != null)
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

        if (!foundInteractable)
        {
            if (interactionPromptText != null && !isHiding)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
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

            // Start the 5-second survival timer!
            if (hidingTimerCoroutine != null) StopCoroutine(hidingTimerCoroutine);
            hidingTimerCoroutine = StartCoroutine(HidingSurvivalCountdown());
        }
        else if (isHiding)
        {
            // Check if they left BEFORE the 5 seconds were up
            if (hidingTimerCoroutine != null)
            {
                StopCoroutine(hidingTimerCoroutine);
                OnHidingFailedEarly();
            }

            isHiding = false;

            controller.enabled = false;
            if (currentHidingSpot != null && currentHidingSpot.exitPosition != null)
            {
                transform.position = currentHidingSpot.exitPosition.position;
            }
            else
            {
                transform.position = preHidePosition;
            }
            controller.enabled = true;

            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }

            if (warningPopupText != null)
            {
                warningPopupText.gameObject.SetActive(false);
            }
        }
    }

    private System.Collections.IEnumerator HidingSurvivalCountdown()
    {
        float timeLeft = 5f;

        if (warningPopupText != null)
        {
            warningPopupText.gameObject.SetActive(true);
        }

        while (timeLeft > 0f)
        {
            if (warningPopupText != null)
            {
                warningPopupText.text = $"Remain in closet for {Mathf.Ceil(timeLeft)}s or lose a life!";
            }

            yield return null;
            timeLeft -= Time.deltaTime;
        }

        if (warningPopupText != null)
        {
            warningPopupText.text = "Danger passed. You can step out.";
        }

        yield return new WaitForSeconds(1.5f);

        if (warningPopupText != null)
        {
            warningPopupText.gameObject.SetActive(false);
        }
    }

    private void OnHidingFailedEarly()
    {
        Debug.Log("Left hiding too early! Lost a life.");

        if (warningPopupText != null)
        {
            warningPopupText.text = "You left too early! Lost a life!";
            warningPopupText.gameObject.SetActive(true);
        }
    }
}