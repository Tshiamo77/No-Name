using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;

    [Header("Look Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    private float xRotation = 0f;

    [Header("Hiding Settings")]
    public bool isHiding = false;
    private HidingSpot currentHidingSpot;
    private Vector3 preHidePosition;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleMouseLook();

        if (isHiding)
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.text = "Press E to Unhide";
                interactionPromptText.gameObject.SetActive(true);
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleHiding();
            }
            return;
        }

        HandleMovement();
        CheckForInteractions();
    }

    private void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ORIGINAL MOUSE LOOK UNTOUCHED
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseX);
    }

    public void ResetGravityVelocity()
    {
        velocity = Vector3.zero;
    }

    private void CheckForInteractions()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            HidingSpot spot = hit.collider.GetComponent<HidingSpot>();
            if (spot != null)
            {
                currentHidingSpot = spot;
                if (interactionPromptText != null)
                {
                    interactionPromptText.text = "Press E to Hide";
                    interactionPromptText.gameObject.SetActive(true);
                }

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    ToggleHiding();
                }
                return;
            }

            MemoryPickup pickup = hit.collider.GetComponent<MemoryPickup>();
            if (pickup != null)
            {
                if (interactionPromptText != null)
                {
                    interactionPromptText.text = "Press E to Pick Up";
                    interactionPromptText.gameObject.SetActive(true);
                }

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    pickup.PickUpItem();
                }
                return;
            }
        }

        currentHidingSpot = null;
        if (interactionPromptText != null && !isHiding)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void ToggleHiding()
    {
        if (!isHiding && currentHidingSpot != null)
        {
            HidingSpot spotToUse = currentHidingSpot;

            isHiding = true;
            preHidePosition = transform.position;

            controller.enabled = false;
            transform.position = spotToUse.insidePosition.position;
            transform.rotation = spotToUse.insidePosition.rotation;
            controller.enabled = true;

            currentHidingSpot = spotToUse;
        }
        else if (isHiding)
        {
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

            currentHidingSpot = null;
        }
    }

    public void OnInteractOrHide(InputAction.CallbackContext context)
    {
        if (context.performed && currentHidingSpot != null)
        {
            ToggleHiding();
        }
    }
}

