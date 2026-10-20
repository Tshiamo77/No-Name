using UnityEngine;

public class PuzzleCube : MonoBehaviour
{
    [SerializeField] private PuzzleColor colorId = PuzzleColor.Red;
    [SerializeField] private bool tintAutomatically = true;
    [SerializeField] private Color highlightColor = Color.yellow;

    public PuzzleColor ColorId => colorId;
    public PuzzleSlot CurrentSlot { get; set; }

    private Rigidbody rb;
    private Renderer cubeRenderer;
    private Color originalColor;
    private Material instanceMaterial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cubeRenderer = GetComponentInChildren<Renderer>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (tintAutomatically)
        {
            colorId.ApplyTo(gameObject);
        }

        if (cubeRenderer != null)
        {
            instanceMaterial = cubeRenderer.material;
            originalColor = instanceMaterial.color;
        }
    }

    public void SetHighlight(bool active)
    {
        if (instanceMaterial != null)
        {
            instanceMaterial.color = active ? Color.Lerp(originalColor, highlightColor, 0.7f) : originalColor;
        }
    }

    public void OnInteract()
    {
        ColorSortPuzzle puzzle = FindFirstObjectByType<ColorSortPuzzle>();
        if (puzzle != null)
        {
            puzzle.SelectCube(this);
        }
    }
}