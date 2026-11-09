using UnityEngine;
using TMPro;

public enum GameTask
{
    UnlockLivingSpaceDoor = 0,
    PickUpPictureInLivingSpace = 1,
    VisitBathroom = 2,
    SolveBrandonPuzzle = 3,
    PickUpTeddyBear = 4,
    VisitLilyRoomUpstairs = 5,
    PickUpBookOnShelf = 6,
    CollectMapPiecesForMainBedroom = 7,
    RememberSafePIN = 8,
    PickUpGun = 9,
    Completed = 10
}

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Task Messages")]
    [TextArea] public string task0Message = "Objective: Unlock the door to the living space.";
    [TextArea] public string task1Message = "Objective: Pick up the picture on the table in the living space.";
    [TextArea] public string task2Message = "Objective: Visit the bathroom.";
    [TextArea] public string task3Message = "Objective: Solve the color puzzle in Brandon's room.";
    [TextArea] public string task4Message = "Objective: Pick up the teddy bear on Brandon's bed.";
    [TextArea] public string task5Message = "Objective: Visit Lily's room upstairs.";
    [TextArea] public string task6Message = "Objective: Pick up the book on the shelf in Lily's room.";
    [TextArea] public string task7Message = "Objective: Collect all missing map pieces to open the main bedroom.";
    [TextArea] public string task8Message = "Objective: Remember the safe PIN to unlock the door.";
    [TextArea] public string task9Message = "Objective: Pick up the gun.";
    [TextArea] public string completionMessage = "Objective: Survive / Escape!";

    public GameTask CurrentTask { get; private set; } = GameTask.UnlockLivingSpaceDoor;

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
    }

    private void Start()
    {
        UpdateObjectiveUI();
    }

    /// <summary>
    /// Call this whenever a specific task action is completed.
    /// Advances progression only if the completed task matches the active step.
    /// </summary>
    public void CompleteTask(GameTask completedTask)
    {
        if (CurrentTask == completedTask)
        {
            CurrentTask++;
            Debug.Log($"[TaskManager] Advanced to next task: {CurrentTask}");
            UpdateObjectiveUI();
        }
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText == null) return;

        switch (CurrentTask)
        {
            case GameTask.UnlockLivingSpaceDoor:
                objectiveText.text = task0Message;
                break;
            case GameTask.PickUpPictureInLivingSpace:
                objectiveText.text = task1Message;
                break;
            case GameTask.VisitBathroom:
                objectiveText.text = task2Message;
                break;
            case GameTask.SolveBrandonPuzzle:
                objectiveText.text = task3Message;
                break;
            case GameTask.PickUpTeddyBear:
                objectiveText.text = task4Message;
                break;
            case GameTask.VisitLilyRoomUpstairs:
                objectiveText.text = task5Message;
                break;
            case GameTask.PickUpBookOnShelf:
                objectiveText.text = task6Message;
                break;
            case GameTask.CollectMapPiecesForMainBedroom:
                objectiveText.text = task7Message;
                break;
            case GameTask.RememberSafePIN:
                objectiveText.text = task8Message;
                break;
            case GameTask.PickUpGun:
                objectiveText.text = task9Message;
                break;
            case GameTask.Completed:
                objectiveText.text = completionMessage;
                break;
        }

        objectiveText.gameObject.SetActive(true);
    }
}