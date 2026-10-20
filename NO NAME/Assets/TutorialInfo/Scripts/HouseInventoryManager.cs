using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HouseInventoryManager : MonoBehaviour
{
    public static HouseInventoryManager Instance { get; private set; }

    [Header("UI Pickup Notification")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 2.0f;

    private HashSet<string> collectedItemIDs = new HashSet<string>();
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    public void AddItem(string itemID, string itemDisplayName = "")
    {
        if (!collectedItemIDs.Contains(itemID))
        {
            collectedItemIDs.Add(itemID);

            // Trigger notification banner
            string nameToShow = string.IsNullOrEmpty(itemDisplayName) ? itemID : itemDisplayName;
            ShowNotification($"Collected: {nameToShow}");
        }
    }

    public bool HasItem(string itemID)
    {
        return collectedItemIDs.Contains(itemID);
    }

    /// <summary>Checks if all 3 map pieces have been collected.</summary>
    public bool HasAllPieces()
    {
        return HasItem("MapPiece_1") && HasItem("MapPiece_2") && HasItem("MapPiece_3");
    }

    private void ShowNotification(string message)
    {
        if (notificationText == null) return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = StartCoroutine(NotificationRoutine(message));
    }

    private IEnumerator NotificationRoutine(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }
}