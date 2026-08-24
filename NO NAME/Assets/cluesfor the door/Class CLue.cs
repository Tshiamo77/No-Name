using UnityEngine;

public class ClassCLue : MonoBehaviour
{
    public ClueManager clueManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            clueManager.AddClue();

            Destroy(gameObject);
        }
    }
}