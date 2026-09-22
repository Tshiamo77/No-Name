using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on your blood drip panel (the object GameOverManager switches on).
/// Every time the panel is enabled, the blood image runs down the screen.
/// </summary>
public class BloodDripEffect : MonoBehaviour
{
    public enum DripMode
    {
        FillDown,   // Blood is revealed from the top edge downward, so drips grow down the screen
        SlideDown   // The whole image slides in from above the screen
    }

    [Header("References")]
    [Tooltip("The UI Image showing the blood. Drag the blood drip image here. If left empty, the first child Image is used.")]
    [SerializeField] private Image bloodImage;

    [Header("Effect")]
    [SerializeField] private DripMode mode = DripMode.FillDown;
    [Tooltip("How long the drip takes to finish. Keep GameOverManager's Blood Drip Duration at least 0.5s longer than this.")]
    [SerializeField] private float dripDuration = 2.5f;
    [SerializeField] private AnimationCurve dripCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private RectTransform rect;
    private Vector2 restPosition;
    private Coroutine routine;

    private void Awake()
    {
        if (bloodImage == null) bloodImage = FindBloodImage();

        if (bloodImage == null)
        {
            Debug.LogWarning("BloodDripEffect: no UI Image found. Assign the blood Image.", this);
            return;
        }

        rect = bloodImage.rectTransform;
        restPosition = rect.anchoredPosition;

        if (mode == DripMode.FillDown)
        {
            if (bloodImage.sprite == null)
            {
                Debug.LogWarning("BloodDripEffect: the Image needs a Source Image for Fill Down mode.", this);
            }

            bloodImage.type = Image.Type.Filled;
            bloodImage.fillMethod = Image.FillMethod.Vertical;
            bloodImage.fillOrigin = (int)Image.OriginVertical.Top;
        }
    }

    /// <summary>
    /// The panel usually has its own background Image, so look at the CHILDREN first.
    /// Falls back to the panel's own Image only if there are no child Images.
    /// </summary>
    private Image FindBloodImage()
    {
        Image own = GetComponent<Image>();

        foreach (Image img in GetComponentsInChildren<Image>(true))
        {
            if (img != own) return img;
        }

        return own;
    }

    private void OnEnable()
    {
        if (bloodImage == null) return;

        if (routine != null) StopCoroutine(routine);

        SetProgress(0f); // start hidden before the first frame is drawn
        routine = StartCoroutine(PlayDrip());
    }

    private IEnumerator PlayDrip()
    {
        float duration = Mathf.Max(0.01f, dripDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetProgress(dripCurve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetProgress(1f);
        routine = null;
    }

    private void SetProgress(float progress)
    {
        if (mode == DripMode.FillDown)
        {
            bloodImage.fillAmount = progress;
        }
        else
        {
            float height = rect.rect.height > 0f ? rect.rect.height : Screen.height;
            rect.anchoredPosition = restPosition + Vector2.up * height * (1f - progress);
        }
    }
}