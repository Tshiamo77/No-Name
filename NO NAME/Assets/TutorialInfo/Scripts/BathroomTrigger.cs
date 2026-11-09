using UnityEngine;

public class BathroomTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FPController>() != null)
        {
            // Advance Task 2 -> Task 3
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(GameTask.VisitBathroom);
            }
        }
    }
}
