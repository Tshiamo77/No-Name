using UnityEngine;

public class PuzzleSlot : MonoBehaviour
{
    [Tooltip("Where the cube sits. Leave empty to use this object's own transform.")]
    [SerializeField] private Transform snapPoint;

    public PuzzleCube CurrentCube { get; private set; }
    public Transform SnapTransform => snapPoint != null ? snapPoint : transform;

    public void AssignCube(PuzzleCube cube)
    {
        CurrentCube = cube;
        if (cube != null)
        {
            cube.CurrentSlot = this;
            // Snaps position and rotation explicitly to the snap point
            cube.transform.position = SnapTransform.position;
            cube.transform.rotation = SnapTransform.rotation;
        }
    }

    public void ClearSlot()
    {
        CurrentCube = null;
    }
}