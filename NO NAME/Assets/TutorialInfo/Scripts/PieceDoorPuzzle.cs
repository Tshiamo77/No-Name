using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieceDoorPuzzle : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private DoorMovement doorMovement;
    [SerializeField] private int requiredPieceCount = 3;

    [Header("UI Pop-up Panel")]
    [SerializeField] private GameObject puzzleUIPanel;
    [SerializeField] private Image[] slotIcons; // 3 UI Image slots inside the panel
    [SerializeField] private Button[] slotButtons; // 3 UI Buttons corresponding to the slots
    [SerializeField] private TextMeshProUGUI statusText;

    private HashSet<int> placedSlots = new HashSet<int>();
    private HouseInventoryManager inventory;
    private bool isSolved = false;

    private void Start()
    {
        inventory = FindFirstObjectByType<HouseInventoryManager>();

        if (puzzleUIPanel != null)
            puzzleUIPanel.SetActive(false);

        // Bind button click events
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            if (slotButtons[i] != null)
            {
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
            }
        }
    }

    /// <summary>Called when the player tries to open the door.</summary>
    public void InteractWithDoor()
    {
        if (isSolved) return;

        if (inventory == null) inventory = FindFirstObjectByType<HouseInventoryManager>();

        // Check if player has all 3 objects
        if (inventory != null && inventory.HasAllPieces(requiredPieceCount))
        {
            OpenPuzzleUI();
        }
        else
        {
            ShowStatusMessage("Looks like I need to find these objects first");
        }
    }

    private void OpenPuzzleUI()
    {
        if (puzzleUIPanel == null) return;

        puzzleUIPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        List<CollectibleObjectData> items = inventory.CollectedPieces;

        // Display thumbnail icons in UI columns
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < items.Count && items[i].icon != null)
            {
                slotIcons[i].sprite = items[i].icon;
                slotIcons[i].gameObject.SetActive(true);
            }
            else
            {
                slotIcons[i].gameObject.SetActive(false);
            }

            // Disable button if already placed
            if (slotButtons[i] != null)
            {
                slotButtons[i].interactable = !placedSlots.Contains(i);
            }
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (placedSlots.Contains(slotIndex)) return;

        placedSlots.Add(slotIndex);

        // Visually disable or dim the placed UI slot button
        if (slotButtons[slotIndex] != null)
            slotButtons[slotIndex].interactable = false;

        ShowStatusMessage($"Placed piece {placedSlots.Count}/{requiredPieceCount} on the door.");

        // Check if all pieces are placed
        if (placedSlots.Count >= requiredPieceCount)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        isSolved = true;
        ShowStatusMessage("All pieces placed! The door unlocks.");

        if (doorMovement != null)
        {
            doorMovement.UnlockFromPuzzle();
            doorMovement.ToggleDoor();
        }

        StartCoroutine(CloseUIWithDelay());
    }

    private IEnumerator CloseUIWithDelay()
    {
        yield return new WaitForSeconds(1.5f);

        if (puzzleUIPanel != null)
            puzzleUIPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowStatusMessage(string msg)
    {
        if (statusText != null)
        {
            statusText.text = msg;
            statusText.gameObject.SetActive(true);
        }
    }
}