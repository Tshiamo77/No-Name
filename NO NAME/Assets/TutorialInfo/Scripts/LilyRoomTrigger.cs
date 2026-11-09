using UnityEngine;

public class LilyRoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FPController>() != null)
        {
            if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask == GameTask.VisitLilyRoomUpstairs)
            {
                TaskManager.Instance.CompleteTask(GameTask.VisitLilyRoomUpstairs);
            }
        }
    }
}
