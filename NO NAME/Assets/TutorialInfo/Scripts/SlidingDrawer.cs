using UnityEngine;
using System.Collections;

public class SlidingDrawer : MonoBehaviour
{
    [Header("Drawer Settings")]
    [Tooltip("How far out the drawer slides.")]
    [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 0.45f);
    [SerializeField] private float slideDuration = 0.3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + openOffset;
    }

    public void ToggleDrawer()
    {
        if (!isAnimating)
        {
            StartCoroutine(SlideRoutine(isOpen ? closedPosition : openPosition));
            isOpen = !isOpen;
        }
    }

    private IEnumerator SlideRoutine(Vector3 targetPos)
    {
        isAnimating = true;
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < slideDuration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        isAnimating = false;
    }
}