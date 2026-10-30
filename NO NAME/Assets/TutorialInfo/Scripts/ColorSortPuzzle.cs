using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ColorSortPuzzle : MonoBehaviour
{
    [Header("Puzzle Setup")]
    [Tooltip("Initial cubes placed left to right as seen by the player.")]
    [SerializeField] private PuzzleCube[] cubes;
    [Tooltip("The correct color order, left to right.")]
    [SerializeField] private PuzzleColor[] solution = { PuzzleColor.Pink, PuzzleColor.Blue, PuzzleColor.Yellow, PuzzleColor.Red, PuzzleColor.Green };

    [Header("Swap Settings")]
    [SerializeField] private float swapDuration = 0.5f;

    [Header("Door")]
    [SerializeField] private DoorMovement door;
    [SerializeField] private bool openDoorOnSolve = false;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private float messageDisplayTime = 3f;
    [SerializeField] private UnityEvent onSolved;
    [SerializeField] private UnityEvent onWrongOrder;

    public PuzzleColor[] Solution => solution;
    public bool IsSolved { get; private set; }

    // --- LOCK STATE: Cubes are locked until the player inspects/interacts with the door ---
    public bool IsPuzzleUnlocked { get; private set; } = false;

    private PuzzleCube selectedCube = null;
    private bool isSwapping = false;
    private Coroutine messageRoutine;

    private void Start()
    {
        if (cubes == null || cubes.Length == 0)
        {
            Debug.LogError("ColorSortPuzzle: No cubes assigned!");
        }

        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    public void UnlockPuzzleInteraction()
    {
        if (!IsPuzzleUnlocked)
        {
            IsPuzzleUnlocked = true;
            ShowMessage("You can now swap the color blocks!");
        }
    }

    public void SelectCube(PuzzleCube clickedCube)
    {
        // 0. BLOCK UNTIL DOOR IS ATTEMPTED
        if (!IsPuzzleUnlocked)
        {
            ShowMessage("Oh its locked, let me solve the puzzle first");
            return;
        }

        if (IsSolved || isSwapping || clickedCube == null) return;

        // 1. FIRST SELECTION
        if (selectedCube == null)
        {
            selectedCube = clickedCube;
            selectedCube.SetHighlight(true);
            ShowMessage($"Selected [{selectedCube.ColorId}] Cube. Click another cube to swap.");
            return;
        }

        // 2. DESELECT
        if (selectedCube == clickedCube)
        {
            ShowMessage($"Deselected [{selectedCube.ColorId}] Cube.");
            selectedCube.SetHighlight(false);
            selectedCube = null;
            return;
        }

        // 3. SECOND SELECTION (SWAP)
        PuzzleCube firstCube = selectedCube;
        selectedCube.SetHighlight(false);
        selectedCube = null;

        StartCoroutine(SwapCubesRoutine(firstCube, clickedCube));
    }

    private IEnumerator SwapCubesRoutine(PuzzleCube cubeA, PuzzleCube cubeB)
    {
        isSwapping = true;
        ShowMessage($"Swapping [{cubeA.ColorId}] and [{cubeB.ColorId}]...");

        Vector3 startPosA = cubeA.transform.position;
        Vector3 startPosB = cubeB.transform.position;
        Quaternion startRotA = cubeA.transform.rotation;
        Quaternion startRotB = cubeB.transform.rotation;

        float elapsed = 0f;
        while (elapsed < swapDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / swapDuration);

            cubeA.transform.position = Vector3.Lerp(startPosA, startPosB, t);
            cubeA.transform.rotation = Quaternion.Lerp(startRotA, startRotB, t);

            cubeB.transform.position = Vector3.Lerp(startPosB, startPosA, t);
            cubeB.transform.rotation = Quaternion.Lerp(startRotB, startRotA, t);

            yield return null;
        }

        cubeA.transform.SetPositionAndRotation(startPosB, startRotB);
        cubeB.transform.SetPositionAndRotation(startPosA, startRotA);

        int indexA = System.Array.IndexOf(cubes, cubeA);
        int indexB = System.Array.IndexOf(cubes, cubeB);

        if (indexA != -1 && indexB != -1)
        {
            cubes[indexA] = cubeB;
            cubes[indexB] = cubeA;
        }

        isSwapping = false;

        CheckSolution();
    }

    private void CheckSolution()
    {
        bool correct = true;
        int count = Mathf.Min(cubes.Length, solution.Length);

        for (int i = 0; i < count; i++)
        {
            if (cubes[i] == null || cubes[i].ColorId != solution[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            Solve();
        }
        else
        {
            onWrongOrder.Invoke();
        }
    }

    private void Solve()
    {
        IsSolved = true;
        ShowMessage("Puzzle Solved! You hear the lock click open.");

        if (door != null)
        {
            door.UnlockFromPuzzle();
            if (openDoorOnSolve) door.ToggleDoor();
        }

        onSolved.Invoke();
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