using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A one-shot scripted monster encounter, tied to a single door. Reusable for every door that
/// should trigger a "hide from the monster" beat:
///
/// - When the door opens, the player immediately loses WASD/camera/interaction control and remains
///   stationary until pulled back into the safe-zone Box Collider (NotifyPlayerSafe / safeZoneBoxCollider).
/// - Works across all rooms (whether Include Full Preamble is ON or OFF).
///
/// Wire Door's "On Door Opened ()" UnityEvent to this component's BeginEncounter().
/// Assign Safe Zone Box Collider and/or Forced Player Return Point in the Inspector.
/// </summary>
public class EnemyEncounterTrigger : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private DoorMovement door; // the door this whole encounter is tied to
    [SerializeField] private FPController playerController;
    [SerializeField] private PlayerLifeManager lifeManager;

    [Header("Mode")]
    [Tooltip("ON: full baby-sound/approach/glimpse intro before pulling player to safe zone. OFF: short warning line before pulling player to safe zone.")]
    [SerializeField] private bool includeFullPreamble = true;

    [Header("Positions")]
    [SerializeField] private Transform revealPoint;        // only used when Include Full Preamble is ON
    [SerializeField] private Transform insideRoomPoint;    // where the enemy "enters" to after the countdown
    [SerializeField] private Transform[] searchWaypoints;  // points the enemy wanders between while searching the room
    [SerializeField] private Transform playerReturnPoint;  // where the player is sent back to if caught
    [Tooltip("Point inside your safe-zone BoxCollider where the stationary player is pulled back to across all rooms.")]
    [SerializeField] private Transform forcedPlayerReturnPoint;
    [Tooltip("Assign the safe-zone BoxCollider so arrival is detected directly and unlocks player controls.")]
    [SerializeField] private BoxCollider safeZoneBoxCollider;
    [SerializeField] private float pullBackSpeed = 4f;

    [Header("Narrative Text (the PLAYER's own thoughts; can reuse your existing dialogue/thought UI)")]
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Audio (only used when Include Full Preamble is ON)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip babySoundClip;
    [SerializeField] private AudioClip monsterApproachClip;

    [Header("Audio - Player Safe Voice")]
    [SerializeField] private AudioSource safeNowVoiceAudioSource;
    [SerializeField] private AudioClip safeNowVoiceClip;
    [SerializeField, Range(0f, 1f)] private float safeNowVoiceVolume = 1f;

    [Header("Timings")]
    [SerializeField] private float babyLineDuration = 2.5f;
    [SerializeField] private float approachDelay = 1.5f;
    [SerializeField] private float revealLineDuration = 2f;
    [SerializeField] private float warningLineDuration = 2.5f; // used when Include Full Preamble is OFF
    [SerializeField] private float hideCountdownDuration = 10f;
    [SerializeField] private float enterRoomLineDuration = 2.5f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private float giveUpLineDuration = 2.5f;
    [SerializeField] private float safeNowLineDuration = 3f;
    [SerializeField] private float catchLineDuration = 2.5f;

    [Header("Dialogue Lines - Player's thoughts (narrativeText)")]
    [SerializeField] private string babySoundLine = "What's that sound?";
    [SerializeField] private string revealLine = "WHAT'S THAT, Let me hide";
    [SerializeField] private string warningLine = "Something's coming... I need to hide!";
    [SerializeField] private string hidingThoughtLine = "I better not leave";
    [SerializeField] private string safeNowLine = "It must be safe now, I still need to investigate";

    [Header("Dialogue Lines - The enemy speaking (Enemy's own dialogue panel)")]
    [SerializeField] private string enterRoomLine = "I know you are in here";
    [SerializeField] private string catchMissLine = "Where do you think you are going haha";
    [SerializeField] private string giveUpLine = "Oh I guess you are not here";

    [Header("Debug")]
    [Tooltip("Logs each stage to the Console so you can see exactly where the sequence is at. Turn off once everything works.")]
    [SerializeField] private bool logDebug = true;

    private bool hasRun = false;
    private bool playerIsSafe = false;

    /// <summary>Wire this to the door's On Door Opened () UnityEvent.</summary>
    public void BeginEncounter()
    {
        if (hasRun)
        {
            Log("BeginEncounter called again, ignored (already ran once).");
            return;
        }
        hasRun = true;

        if (playerController == null)
            playerController = FindFirstObjectByType<FPController>();

        // IMMEDIATELY lock WASD, camera look, and interactions the moment the door opens
        if (playerController != null)
        {
            playerController.LockPlayerControl();
        }

        Log("Encounter started - player controls locked.");
        StartCoroutine(EncounterRoutine());
    }

    /// <summary>Wire this to a PlayerZoneTrigger's On Player Entered () event, placed inside the safe room.</summary>
    public void NotifyPlayerSafe()
    {
        Log("NotifyPlayerSafe() received - player entered safe BoxCollider.");
        playerIsSafe = true;
    }

    private bool IsPlayerInsideSafeBox()
    {
        if (playerIsSafe) return true;

        if (safeZoneBoxCollider != null && playerController != null)
        {
            if (safeZoneBoxCollider.bounds.Contains(playerController.transform.position))
            {
                playerIsSafe = true;
                return true;
            }
        }

        return false;
    }

    private IEnumerator EncounterRoutine()
    {
        playerIsSafe = false;

        // Ensure controls stay locked while stationary at the door
        if (playerController != null)
            playerController.LockPlayerControl();

        if (includeFullPreamble)
        {
            // 1. A baby's cry, and the player's thought (player remains stationary)
            Log("Step 1: baby sound + line (player stationary).");
            if (audioSource != null && babySoundClip != null) audioSource.PlayOneShot(babySoundClip);
            ShowLine(babySoundLine);
            yield return new WaitForSeconds(babyLineDuration);

            // 2. Something is coming (player remains stationary)
            Log("Step 2: approach sound (player stationary).");
            if (audioSource != null && monsterApproachClip != null) audioSource.PlayOneShot(monsterApproachClip);
            yield return new WaitForSeconds(approachDelay);

            // 3. A brief glimpse of the enemy (player remains stationary)
            Log("Step 3: reveal enemy briefly (player stationary).");
            if (enemy != null && revealPoint != null) enemy.RevealAt(revealPoint);
            ShowLine(revealLine);
            yield return new WaitForSeconds(revealLineDuration);
            HideLine();
        }
        else
        {
            // Other rooms: player still remains stationary at the door during the warning
            Log("Step 1-3 (other room): warning line while player is stationary.");
            ShowLine(warningLine);
            yield return new WaitForSeconds(warningLineDuration);
            HideLine();
        }

        // 4. Pull the locked player back to the safe-zone BoxCollider (happens for ALL rooms)
        Log("Step 4: pulling the stationary player back to the safe-zone BoxCollider.");
        Transform pullTarget = forcedPlayerReturnPoint != null
            ? forcedPlayerReturnPoint
            : (safeZoneBoxCollider != null ? safeZoneBoxCollider.transform : null);

        if (playerController != null && pullTarget != null)
        {
            yield return StartCoroutine(PullPlayerBack(pullTarget));
        }
        else
        {
            Debug.LogWarning($"[EnemyEncounterTrigger: {gameObject.name}] Missing Forced Player Return Point or Safe Zone Box Collider! Unlocking player.", this);
            if (playerController != null) playerController.UnlockPlayerControl();
        }

        Log("Step 4 done: player reached safe BoxCollider and regained control. Closing door.");
        if (door != null) door.ForceClose();

        // 5. Hide countdown
        Log("Step 5: hide countdown.");
        float timer = hideCountdownDuration;
        while (timer > 0f)
        {
            ShowLine($"Hide quickly! Time remaining: {Mathf.CeilToInt(timer)}s");
            yield return null;
            timer -= Time.deltaTime;
        }
        HideLine();

        // 6. The door opens and the enemy enters, speaking its first line.
        Log("Step 6: door opening, waiting for the swing to finish before the enemy enters.");
        if (door != null) door.ForceOpen();

        float doorWaitTimer = 3f;
        while (door != null && !door.IsFullyOpen && doorWaitTimer > 0f)
        {
            doorWaitTimer -= Time.deltaTime;
            yield return null;
        }

        Log("Step 6: door clear, enemy entering.");
        if (enemy != null && insideRoomPoint != null) enemy.RevealAt(insideRoomPoint);
        if (enemy != null) enemy.SayLine(enterRoomLine, enterRoomLineDuration);
        yield return new WaitForSeconds(enterRoomLineDuration);

        if (playerController != null && playerController.isHiding)
        {
            ShowLine(hidingThoughtLine);
        }

        // 7. The enemy wanders the room while checking whether the player is hiding
        Log("Step 7: enemy searches the room.");
        if (enemy != null) enemy.StartWandering(searchWaypoints);

        float searchTimer = searchDuration;
        bool caught = false;
        while (searchTimer > 0f)
        {
            if (playerController != null && !playerController.isHiding)
            {
                caught = true;
                break;
            }
            searchTimer -= Time.deltaTime;
            yield return null;
        }

        if (enemy != null) enemy.StopWandering();

        if (caught)
        {
            Log("Step 7 result: player caught.");
            HideLine();
            if (enemy != null) enemy.SayLine(catchMissLine, catchLineDuration);

            if (lifeManager != null) lifeManager.LoseLife();

            if (playerController != null && playerReturnPoint != null)
            {
                CharacterController cc = playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                playerController.transform.SetPositionAndRotation(playerReturnPoint.position, playerReturnPoint.rotation);
                playerController.ResetGravityVelocity();

                if (cc != null) cc.enabled = true;
            }

            yield return new WaitForSeconds(catchLineDuration);
        }
        else
        {
            Log("Step 7 result: enemy gives up, player stayed hidden.");
            if (enemy != null) enemy.SayLine(giveUpLine, giveUpLineDuration);
            yield return new WaitForSeconds(giveUpLineDuration);

            ShowLine(safeNowLine);
            PlaySafeNowVoice();

            yield return new WaitForSeconds(safeNowLineDuration);
        }

        HideLine();

        // 8. Either way, the enemy immediately heads back upstairs and resumes patrolling there
        Log("Step 8: enemy returns upstairs.");
        if (enemy != null) enemy.ReturnToUpstairsPatrol();
    }

    private void PlaySafeNowVoice()
    {
        Log("Safe voice method called.");

        if (safeNowVoiceAudioSource == null)
        {
            Debug.LogError("Safe voice AudioSource is not assigned!", this);
            return;
        }

        if (safeNowVoiceClip == null)
        {
            Debug.LogError("Safe voice AudioClip is not assigned!", this);
            return;
        }

        safeNowVoiceAudioSource.enabled = true;
        safeNowVoiceAudioSource.mute = false;
        safeNowVoiceAudioSource.spatialBlend = 0f;

        safeNowVoiceAudioSource.PlayOneShot(
            safeNowVoiceClip,
            safeNowVoiceVolume
        );

        Log("Safe voice playback command sent.");
    }

    /// <summary>
    /// Keeps player controls completely locked and pulls them back until they enter the safe-zone
    /// BoxCollider (NotifyPlayerSafe / IsPlayerInsideSafeBox) or reach the target point inside it.
    /// </summary>
    private IEnumerator PullPlayerBack(Transform target)
    {
        playerController.LockPlayerControl();

        CharacterController cc = playerController.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogWarning("EnemyEncounterTrigger: Player has no CharacterController, can't pull it back.", this);
            playerController.UnlockPlayerControl();
            yield break;
        }

        float elapsed = 0f;
        const float maxDuration = 8f; // safety timeout

        while (!IsPlayerInsideSafeBox() && elapsed < maxDuration)
        {
            Vector3 toTarget = target.position - playerController.transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude < 0.2f)
            {
                playerIsSafe = true;
                break;
            }

            Vector3 step = toTarget.normalized * pullBackSpeed * Time.deltaTime;
            cc.Move(step + Vector3.down * 0.05f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerController.transform.rotation = target.rotation;
        playerController.ResetGravityVelocity();

        // ONLY restore WASD/mouse/interaction control now that the player is inside the BoxCollider
        playerController.UnlockPlayerControl();
    }

    private void ShowLine(string text)
    {
        if (narrativeText == null) return;
        narrativeText.text = text;
        narrativeText.gameObject.SetActive(true);
    }

    private void HideLine()
    {
        if (narrativeText == null) return;
        narrativeText.gameObject.SetActive(false);
    }

    private void Log(string message)
    {
        if (logDebug) Debug.Log($"[EnemyEncounterTrigger: {gameObject.name}] {message}");
    }
}