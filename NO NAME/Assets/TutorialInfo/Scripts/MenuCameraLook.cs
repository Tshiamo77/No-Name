using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCameraLook : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.2f; // Adjusted for New Input System delta scale
    [SerializeField] private float maxHorizontalAngle = 30f; // How far left/right they can look
    [SerializeField] private float maxVerticalAngle = 15f;   // How far up/down they can look

    private Vector3 initialRotation;
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Start()
    {
        initialRotation = transform.eulerAngles;
        rotationX = initialRotation.y;
        rotationY = initialRotation.x;

        // Ensure the cursor is unlocked and visible for the menu UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Check if the mouse is active
        if (Mouse.current == null) return;

        // Read delta mouse movement from the New Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        rotationX += mouseX;
        rotationY -= mouseY;

        // Clamp the angles so the camera doesn't spin infinitely away from the house
        float minX = initialRotation.y - maxHorizontalAngle;
        float maxX = initialRotation.y + maxHorizontalAngle;
        rotationX = Mathf.Clamp(rotationX, minX, maxX);

        float minY = initialRotation.x - maxVerticalAngle;
        float maxY = initialRotation.x + maxVerticalAngle;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Apply rotation smoothly to the camera
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }
}