using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public static bool hasKey = false;

    [SerializeField] private string promptMessage = "This is the key to open the door.\nPress E to Pick Up";

    public string PromptMessage => promptMessage;

    public void PickUpItem()
    {
        hasKey = true;

        Debug.Log("Key collected!");

        gameObject.SetActive(false);
    }
}