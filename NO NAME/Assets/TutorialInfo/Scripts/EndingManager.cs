using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("Player Reference")]
    [SerializeField] private MonoBehaviour playerController;

    [Header("UI Panels & Choice Buttons")]
    [SerializeField] private GameObject endingChoicePanel;
    [SerializeField] private Button leaveGuiltButton;
    [SerializeField] private Button endItAllButton;
    [SerializeField] private CanvasGroup screenFadeGroup; // Attach full-screen black UI Image CanvasGroup

    [Header("Dialogue & Text")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField]
    private string[] revelationDialogue = new string[]
    {
        "Wait... this photo... these memories...",
        "It wasn't an intruder...",
        "It was me. I... I killed my own family."
    };
    [SerializeField] private float dialogueTypingSpeed = 0.05f;

    [Header("Lighting & Visual FX")]
    [SerializeField] private Light mainDirectionalLight;
    [SerializeField] private Color darkAmbientColor = new Color(0.02f, 0.01f, 0.01f);

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip policeSirensSound;
    [SerializeField] private AudioClip gunshotSound;
    [SerializeField] private AudioClip eerieAtmosphereSound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (endingChoicePanel != null) endingChoicePanel.SetActive(false);

        // Bind button click listeners
        if (leaveGuiltButton != null) leaveGuiltButton.onClick.AddListener(OnChooseLeaveGuilt);
        if (endItAllButton != null) endItAllButton.onClick.AddListener(OnChooseEndItAll);
    }

    public void TriggerEndingSequence()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // 1. Ensure Player remains locked in place
        if (playerController != null) playerController.enabled = false;

        // 2. Shift lighting to dark, eerie mood
        if (mainDirectionalLight != null) mainDirectionalLight.intensity = 0.05f;
        RenderSettings.ambientLight = darkAmbientColor;

        // 3. Play dark ambient sound
        if (audioSource != null && eerieAtmosphereSound != null)
        {
            audioSource.Stop();
            audioSource.clip = eerieAtmosphereSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 4. Type out revelation dialogue line by line
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            foreach (string line in revelationDialogue)
            {
                dialogueText.text = "";
                foreach (char letter in line.ToCharArray())
                {
                    dialogueText.text += letter;
                    yield return new WaitForSeconds(dialogueTypingSpeed);
                }
                yield return new WaitForSeconds(2.5f);
            }
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
        }

        // 5. Unlock cursor and reveal Choice UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(true);
        }
    }

    private void OnChooseLeaveGuilt()
    {
        if (endingChoicePanel != null) endingChoicePanel.SetActive(false);
        StartCoroutine(LeaveGuiltRoutine());
    }

    private IEnumerator LeaveGuiltRoutine()
    {
        // Play police sirens sound
        if (audioSource != null && policeSirensSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(policeSirensSound);
        }

        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(FadeToBlack(3.0f));

        SceneManager.LoadScene("MAIN_MENU");
    }

    private void OnChooseEndItAll()
    {
        if (endingChoicePanel != null) endingChoicePanel.SetActive(false);
        StartCoroutine(EndItAllRoutine());
    }

    private IEnumerator EndItAllRoutine()
    {
        // Play gunshot sound
        if (audioSource != null && gunshotSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(gunshotSound);
        }

        yield return new WaitForSeconds(1.2f);
        yield return StartCoroutine(FadeToBlack(2.0f));

        SceneManager.LoadScene("MAIN_MENU");
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (screenFadeGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenFadeGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
    }
}