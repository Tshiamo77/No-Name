using System.Collections;
using UnityEngine;

public class SafeDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = -110f;
    [SerializeField] private float openSpeed = 2f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen = false;
    private bool isAnimating = false;

    private void Start()
    {
        closedRotation = transform.localRotation;

        openRotation = closedRotation *
                       Quaternion.Euler(0f, openAngle, 0f);
    }

    public void OpenSafeDoor()
    {
        if (isOpen || isAnimating)
            return;

        StartCoroutine(OpenDoorAnimation());
    }

    private IEnumerator OpenDoorAnimation()
    {
        isAnimating = true;

        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;

            transform.localRotation = Quaternion.Slerp(
                closedRotation,
                openRotation,
                elapsed
            );

            yield return null;
        }

        transform.localRotation = openRotation;

        isOpen = true;
        isAnimating = false;

        Debug.Log("Safe door opened!");
    }
}