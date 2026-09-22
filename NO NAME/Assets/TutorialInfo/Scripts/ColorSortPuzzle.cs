using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Put this on an empty "ColorPuzzle" object. Once every slot is filled it checks the order:
/// correct = unlock the door, wrong = eject the cubes so the player can try again.
/// </summary>
public class ColorSortPuzzle : MonoBehaviour
{
    [Header("Puzzle Setup")]
    [Tooltip("Slots in order, LEFT to RIGHT as the player sees them when standing at the panel.")]
    [SerializeField] private PuzzleSlot[] slots;
    [Tooltip("The correct color for each slot, in the same order as Slots.")]
    [SerializeField] private PuzzleColor[] solution = { PuzzleColor.Pink, PuzzleColor.Blue, PuzzleColor.Yellow, PuzzleColor.Red, PuzzleColor.Green };

    [Header("Door")]
    [SerializeField] private DoorMovement door;
    [Tooltip("If ticked the door swings open by itself when solved. Otherwise it just unlocks.")]
    [SerializeField] private bool openDoorOnSolve = false;

    [Header("Feedback")]
    [Tooltip("A separate TMP text on your Canvas (don't reuse the interaction prompt).")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private float messageDisplayTime = 3f;
    [SerializeField] private float wrongOrderDelay = 1f;
    [SerializeField] private UnityEvent onSolved;
    [SerializeField] private UnityEvent onWrongOrder;

    public PuzzleColor[] Solution => solution;
    public bool IsSolved { get; private set; }

    private bool isResolving = false;
    private Coroutine messageRoutine;

    private void Awake()
    {
        if (slots == null) return;
        foreach (PuzzleSlot slot in slots)
        {
            if (slot != null) slot.CubePlaced += OnCubePlaced;
        }
    }

    private void OnDestroy()
    {
        if (slots == null) return;
        foreach (PuzzleSlot slot in slots)
        {
            if (slot != null) slot.CubePlaced -= OnCubePlaced;
        }
    }

    private void Start()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("ColorSortPuzzle: no slots assigned!");
        }
        else if (solution == null || solution.Length != slots.Length)
        {
            Debug.LogWarning("ColorSortPuzzle: the Solution list must have the same length as the Slots list.");
        }

        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    private void OnCubePlaced(PuzzleSlot slot)
    {
        if (IsSolved || isResolving) return;

        // Wait until every slot has a cube in it
        foreach (PuzzleSlot s in slots)
        {
            if (!s.IsOccupied) return;
        }

        bool correct = true;
        int count = Mathf.Min(slots.Length, solution.Length);
        for (int i = 0; i < count; i++)
        {
            if (slots[i].CurrentCube.ColorId != solution[i])
            {
                correct = false;
                break;
            }
        }

        if (correct) Solve();
        else StartCoroutine(WrongOrderRoutine());
    }

    private void Solve()
    {
        IsSolved = true;
        Debug.Log("Color puzzle solved!");
        ShowMessage("You hear the lock click open.");

        if (door != null)
        {
            door.UnlockFromPuzzle();
            if (openDoorOnSolve) door.ToggleDoor();
        }

        onSolved.Invoke();
    }

    private IEnumerator WrongOrderRoutine()
    {
        isResolving = true;
        ShowMessage("That's not right... the cubes are pushed back out.");
        onWrongOrder.Invoke();

        yield return new WaitForSeconds(wrongOrderDelay);

        foreach (PuzzleSlot s in slots)
        {
            s.EjectCube();
        }

        isResolving = false;
    }

    private void ShowMessage(string message)
    {
        if (statusText == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    private IEnumerator MessageRoutine(string message)
    {
        statusText.text = message;
        statusText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDisplayTime);

        statusText.gameObject.SetActive(false);
        messageRoutine = null;
    }
}
