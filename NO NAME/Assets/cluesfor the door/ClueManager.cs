using UnityEngine;

public class ClueManager : MonoBehaviour
{
    public int cluesCollected = 0;

    public void AddClue()
    {
        cluesCollected++;

        Debug.Log("Clues collected: " + cluesCollected);
    }
}
