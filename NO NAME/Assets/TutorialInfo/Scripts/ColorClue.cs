using UnityEngine;

/// <summary>
/// Shows the puzzle's solution as colored blocks in another room.
/// It reads the colors straight from ColorSortPuzzle, so the clue can never go out of sync.
/// </summary>
public class ColorClue : MonoBehaviour
{
    [SerializeField] private ColorSortPuzzle puzzle;
    [Tooltip("The clue blocks, LEFT to RIGHT as the player reads them.")]
    [SerializeField] private Renderer[] displays;

    private void Start()
    {
        if (puzzle == null || displays == null)
        {
            Debug.LogWarning("ColorClue: assign the Puzzle and the Displays.");
            return;
        }

        PuzzleColor[] solution = puzzle.Solution;
        for (int i = 0; i < displays.Length && i < solution.Length; i++)
        {
            if (displays[i] != null)
            {
                displays[i].material.color = solution[i].ToColor();
            }
        }
    }
}