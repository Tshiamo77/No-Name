using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class RoomLightSwitch : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light roomLight;
    [SerializeField] private bool lightStartsOn = false;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Key toggleKey = Key.F;

    [Header("Player Settings")]
    [SerializeField] private Camera playerCamera;

    [Header("UI Prompt Settings")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string turnOnMessage = "Press F to turn on the light";
    [SerializeField] private string turnOffMessage = "Press F to turn off the light";

    private bool isPlayerLookingAtSwitch = false;

    void Start()
    {
        if (roomLight != null)
        {
            roomLight.enabled = lightStartsOn;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        CheckForPlayerInteraction();
    }

    private void CheckForPlayerInteraction()
    {
        isPlayerLookingAtSwitch = false;

        if (playerCamera == null || roomLight == null)
        {
            HidePrompt();
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                isPlayerLookingAtSwitch = true;
            }
        }

        if (isPlayerLookingAtSwitch)
        {
            ShowPrompt();

            if (Keyboard.current != null &&
                Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                ToggleLight();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private void ToggleLight()
    {
        roomLight.enabled = !roomLight.enabled;

        UpdatePrompt();

        Debug.Log(roomLight.enabled
            ? "Room light switched ON."
            : "Room light switched OFF.");
    }

    private void ShowPrompt()
    {
        if (promptText == null)
            return;

        promptText.gameObject.SetActive(true);
        UpdatePrompt();
    }

    private void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null || roomLight == null)
            return;

        promptText.text = roomLight.enabled
            ? turnOffMessage
            : turnOnMessage;
    }
}