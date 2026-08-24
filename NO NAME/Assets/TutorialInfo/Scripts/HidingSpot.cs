using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Positions")]
    public Transform insidePosition;
    public Transform exitPosition;

    [Header("Prompt Settings")]
    public string promptMessage = "Press E to Hide";
}
