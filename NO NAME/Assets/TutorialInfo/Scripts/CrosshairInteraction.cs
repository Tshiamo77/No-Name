using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairInteractor : MonoBehaviour
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

    private MemoryInteractable currentMemoryTarget = null;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, interactableLayers))
        {
            if (hit.collider.TryGetComponent<MemoryInteractable>(out MemoryInteractable memory))
            {
                currentMemoryTarget = memory;
                SetCrosshairColor(highlightColor);

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    memory.Interact();
                    Debug.Log("Interacted with memory via crosshair look-at!");
                }
                return;
            }
        }

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