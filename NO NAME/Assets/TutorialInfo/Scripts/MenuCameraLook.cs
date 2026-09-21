using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCameraLook : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.2f; // Adjust to speed up/slow down mouse look
    [SerializeField] private float maxHorizontalAngle = 30f; // Max degrees left/right from starting view
    [SerializeField] private float maxVerticalAngle = 15f;   // Max degrees up/down from starting view

    private Vector3 initialRotation;
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Start()
    {
        // Record the initial angle of the camera when the scene loads
        initialRotation = transform.eulerAngles;
        rotationX = initialRotation.y;
        rotationY = initialRotation.x;

        // Keep cursor free so buttons remain clickable
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // Read mouse delta from the New Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        rotationX += mouseX;
        rotationY -= mouseY;

        // Clamp rotation angles so the player can't spin completely around or away from the house
        float minX = initialRotation.y - maxHorizontalAngle;
        float maxX = initialRotation.y + maxHorizontalAngle;
        rotationX = Mathf.Clamp(rotationX, minX, maxX);

        float minY = initialRotation.x - maxVerticalAngle;
        float maxY = initialRotation.x + maxVerticalAngle;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // Apply the clamped rotation to the camera
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }
}
