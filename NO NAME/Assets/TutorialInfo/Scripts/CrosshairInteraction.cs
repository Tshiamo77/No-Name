using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance in meters the player can look to highlight/pick up an item.")]
    [SerializeField] private float reachDistance = 3.5f;

    [Tooltip("Layer mask for interactable objects (Optional - defaults to checking all layers).")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("UI Crosshair Reference")]
    [Tooltip("Drag your Canvas Crosshair Image here.")]
    [SerializeField] private Image crosshairImage;

    [Header("Crosshair Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color highlightColor = Color.green;

    private MemoryPickup currentMemoryTarget = null;

    void Update()
    {
        // 1. Cast a ray from the center of the camera forward
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, interactableLayers))
        {
            // 2. Check if the object we are looking at has a MemoryPickup component
            if (hit.collider.TryGetComponent<MemoryPickup>(out MemoryPickup memory))
            {
              
                currentMemoryTarget = memory;
                SetCrosshairColor(highlightColor);

                // Check for 'E' keypress using New Input System
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                   
                    Debug.Log("Interacted with memory via crosshair look-at!");
                }
                return;
            }
        }

        // 3. If looking at nothing or an ordinary wall/object, reset crosshair
        currentMemoryTarget = null;
        SetCrosshairColor(defaultColor);
    }

    private void SetCrosshairColor(Color color)
    {
        if (crosshairImage != null && crosshairImage.color != color)
        {
            crosshairImage.color = color;
        }
    }
}
