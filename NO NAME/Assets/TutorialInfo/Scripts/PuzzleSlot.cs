using UnityEngine;

/// <summary>
/// One socket on the sorting panel. Needs a Box Collider with "Is Trigger" ticked
/// so FPController's raycast can target it without blocking the player or the cubes.
/// </summary>
public class PuzzleSlot : MonoBehaviour
{
    [Tooltip("Where the cube sits. Leave empty to use this object's own position and rotation.")]
    [SerializeField] private Transform snapPoint;
    [SerializeField] private float ejectForce = 2f;

    public PuzzleCube CurrentCube { get; private set; }
    public bool IsOccupied => CurrentCube != null;

    /// <summary>Raised after a cube is placed. ColorSortPuzzle listens to this.</summary>
    public event System.Action<PuzzleSlot> CubePlaced;

    public void PlaceCube(PuzzleCube cube)
    {
        if (IsOccupied || cube == null) return;

        Transform target = snapPoint != null ? snapPoint : transform;
        CurrentCube = cube;
        cube.LockAt(target.position, target.rotation);

        CubePlaced?.Invoke(this);
    }

    /// <summary>Pops the cube back out toward the player so they can pick it up and retry.</summary>
    public void EjectCube()
    {
        if (CurrentCube == null) return;

        Transform target = snapPoint != null ? snapPoint : transform;

        Vector3 toPlayer = transform.forward;
        FPController player = FindFirstObjectByType<FPController>();
        if (player != null)
        {
            toPlayer = player.transform.position - target.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f) toPlayer = transform.forward;
        }
        toPlayer.Normalize();

        Vector3 impulse = (toPlayer + Vector3.up * 0.5f) * ejectForce;
        CurrentCube.Release(target.position, impulse);
        CurrentCube = null;
    }
}