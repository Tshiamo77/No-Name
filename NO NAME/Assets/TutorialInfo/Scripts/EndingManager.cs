using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("References")]
    [SerializeField] private MonoBehaviour playerController; // Assign FPController
    [SerializeField] private Transform playerCameraTransform; // Assign Main Camera or Camera root
    [SerializeField] private Enemy enemyInstance;
    [SerializeField] private Transform enemyConfrontationPoint;

    [Header("UI Panels & Choice Buttons")]
    [SerializeField] private GameObject endingChoicePanel;
    [SerializeField] private Button leaveGuiltButton;
    [SerializeField] private Button endItAllButton;
    [SerializeField] private CanvasGroup screenFadeGroup;

    [Header("Dialogue Displays")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Lighting & Atmosphere")]
    [SerializeField] private Light mainDirectionalLight;
    [SerializeField] private Color darkAmbientColor = new Color(0.04f, 0.01f, 0.01f);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip policeSirensSound;
    [SerializeField] private AudioClip gunshotSound;
    [SerializeField] private AudioClip eerieAtmosphereSound;

    private struct DialogueLine
    {
        public string speaker;
        public string message;
        public Color color;

        public DialogueLine(string speaker, string message, Color color)
        {
            this.speaker = speaker;
            this.message = message;
            this.color = color;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (endingChoicePanel != null) endingChoicePanel.SetActive(false);

        if (leaveGuiltButton != null) leaveGuiltButton.onClick.AddListener(OnChooseLeaveGuilt);
        if (endItAllButton != null) endItAllButton.onClick.AddListener(OnChooseEndItAll);
    }

    public void TriggerEndingSequence()
    {
        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // 1. Completely disable player control (Blocks WASD and Mouse Look)
        if (playerController != null) playerController.enabled = false;

        // 2. Shift to dark ambient lighting
        if (mainDirectionalLight != null) mainDirectionalLight.intensity = 0.02f;
        RenderSettings.ambientLight = darkAmbientColor;

        if (audioSource != null && eerieAtmosphereSound != null)
        {
            audioSource.clip = eerieAtmosphereSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 3. Teleport Monster and align both transforms face-to-face
        if (enemyInstance != null && playerController != null)
        {
            Vector3 spawnPos;
            Vector3 playerPos = playerController.transform.position;

            if (enemyConfrontationPoint != null)
            {
                spawnPos = enemyConfrontationPoint.position;
            }
            else
            {
                // Fallback: spawn 1.8 units in front of the player
                spawnPos = playerPos + (playerController.transform.forward * 1.8f);
            }

            // Calculate rotations so they face each other directly
            Vector3 directionToPlayer = (playerPos - spawnPos).normalized;
            directionToPlayer.y = 0f; // Keep rotation horizontal
            Quaternion enemyTargetRotation = Quaternion.LookRotation(directionToPlayer);

            // Warp Enemy and rotate to face player
            enemyInstance.FaceToFaceConfrontation(spawnPos, enemyTargetRotation);

            // Rotate Player transform to look directly at the enemy
            Vector3 directionToEnemy = (spawnPos - playerPos).normalized;
            directionToEnemy.y = 0f;
            playerController.transform.rotation = Quaternion.LookRotation(directionToEnemy);

            // Align camera height towards enemy eyes if camera reference exists
            if (playerCameraTransform != null)
            {
                Vector3 enemyEyePos = spawnPos + Vector3.up * 1.6f;
                playerCameraTransform.LookAt(enemyEyePos);
            }
        }

        // 4. Dramatic Confrontation Dialogue
        DialogueLine[] script = new DialogueLine[]
        {
            new DialogueLine("Monster", "Do you see it now? The blood on your hands never washed away.", Color.red),
            new DialogueLine("Player", "It wasn't an intruder... All along, I was the monster.", Color.white),
            new DialogueLine("Monster", "You locked away the truth, but you can't run from what you've done.", Color.red),
            new DialogueLine("Player", "God help me...", Color.white)
        };

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);

            foreach (var line in script)
            {
                dialogueText.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(line.color)}>{line.speaker}:</color></b> ";

                foreach (char letter in line.message.ToCharArray())
                {
                    dialogueText.text += letter;
                    yield return new WaitForSeconds(0.04f);
                }

                yield return new WaitForSeconds(2.2f);
            }

            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
        }

        // 5. Unlock Cursor & Show Binary Choice
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