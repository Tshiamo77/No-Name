using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A one-shot scripted monster encounter, tied to a single door. Reusable for every door that
/// should trigger a "hide from the monster" beat:
///
/// - The FIRST/main encounter: tick Include Full Preamble. Plays baby sound -> approach sound ->
///   brief glimpse at Reveal Point -> waits for the player to return to a safe zone -> door closes.
/// - Any OTHER door: untick Include Full Preamble. No baby sound, no glimpse, no safe zone needed -
///   it just shows Warning Line for Warning Line Duration seconds, then closes the door automatically.
///
/// From there every encounter is identical: hide countdown -> door opens, enemy enters and speaks ->
/// enemy wanders the room while searching -> catch, or the enemy gives up -> enemy goes back to
/// patrolling upstairs (via Enemy.ReturnToUpstairsPatrol(), configured once on the Enemy itself).
///
/// Wire Door's "On Door Opened ()" UnityEvent to this component's BeginEncounter().
/// If Include Full Preamble is ticked, also wire a PlayerZoneTrigger placed just inside the safe
/// room to this component's NotifyPlayerSafe(). If it's unticked, no safe-zone trigger is needed at all.
/// </summary>
public class EnemyEncounterTrigger : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private DoorMovement door; // the door this whole encounter is tied to
    [SerializeField] private FPController playerController;
    [SerializeField] private PlayerLifeManager lifeManager;

    [Header("Mode")]
    [Tooltip("ON: full baby-sound/approach/glimpse intro, waits for a safe-zone trigger before closing the door. OFF: just a short warning line, then the door closes automatically - no safe zone needed.")]
    [SerializeField] private bool includeFullPreamble = true;

    [Header("Positions")]
    [SerializeField] private Transform revealPoint;       // only used when Include Full Preamble is ON
    [SerializeField] private Transform insideRoomPoint;    // where the enemy "enters" to after the countdown
    [SerializeField] private Transform[] searchWaypoints;  // points the enemy wanders between while searching the room
    [SerializeField] private Transform playerReturnPoint;  // where the player is sent back to if caught
    [Tooltip("Only used when Include Full Preamble is ON. Right after the glimpse, the player is frozen and walked back to this point, which should sit inside your safe-zone trigger, so they can never get stuck outside.")]
    [SerializeField] private Transform forcedPlayerReturnPoint;
    [SerializeField] private float pullBackSpeed = 4f;

    [Header("Narrative Text (the PLAYER's own thoughts; can reuse your existing dialogue/thought UI)")]
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Audio (only used when Include Full Preamble is ON)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip babySoundClip;
    [SerializeField] private AudioClip monsterApproachClip;

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
        Log("Encounter started.");
        StartCoroutine(EncounterRoutine());
    }

    /// <summary>Wire this to a PlayerZoneTrigger's On Player Entered () event, placed inside the safe room (only needed when Include Full Preamble is ON).</summary>
    public void NotifyPlayerSafe()
    {
        Log("NotifyPlayerSafe() received.");
        playerIsSafe = true;
    }

    private IEnumerator EncounterRoutine()
    {
        playerIsSafe = false;

        if (includeFullPreamble)
        {
            // 1. A baby's cry, and the player's thought
            Log("Step 1: baby sound + line.");
            if (audioSource != null && babySoundClip != null) audioSource.PlayOneShot(babySoundClip);
            ShowLine(babySoundLine);
            yield return new WaitForSeconds(babyLineDuration);

            // 2. Something is coming
            Log("Step 2: approach sound.");
            if (audioSource != null && monsterApproachClip != null) audioSource.PlayOneShot(monsterApproachClip);
            yield return new WaitForSeconds(approachDelay);

            // 3. A brief glimpse of the enemy
            Log("Step 3: reveal enemy briefly.");
            if (enemy != null && revealPoint != null) enemy.RevealAt(revealPoint);
            ShowLine(revealLine);
            yield return new WaitForSeconds(revealLineDuration);
            HideLine();

            // 4. Freeze the player and walk them back inside, through the safe zone, then close the door.
            // This both prevents them wandering off after seeing the monster AND guarantees the
            // sequence can never hang forever waiting on a trigger that didn't fire.
            Log("Step 4: pulling the player back to the safe zone.");
            if (playerController != null && forcedPlayerReturnPoint != null)
            {
                yield return StartCoroutine(PullPlayerBack(forcedPlayerReturnPoint));
            }

            // Safety net: give the safe-zone trigger a moment to fire naturally from the walk-back;
            // if it somehow still hasn't (e.g. the box is misaligned), force it so nothing gets stuck.
            float safetyTimer = 2f;
            while (!playerIsSafe && safetyTimer > 0f)
            {
                safetyTimer -= Time.deltaTime;
                yield return null;
            }
            if (!playerIsSafe)
            {
                Log("Safe-zone trigger never fired after the pull-back - forcing it directly so the sequence can't get stuck.");
                playerIsSafe = true;
            }

            Log("Step 4 done: player is safe. Closing door.");
            if (door != null) door.ForceClose();
        }
        else
        {
            // Simple version: no baby cry, no glimpse, no safe zone - just a warning, then auto-close
            Log("Step 1-4 (simple mode): warning line, then auto-close.");
            ShowLine(warningLine);
            yield return new WaitForSeconds(warningLineDuration);
            HideLine();
            if (door != null) door.ForceClose();
        }

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
        // Wait for the door's swing to actually finish before warping the enemy in, otherwise the
        // enemy can spawn into/collide with the still-moving door mesh and get stuck against it.
        Log("Step 6: door opening, waiting for the swing to finish before the enemy enters.");
        if (door != null) door.ForceOpen();

        float doorWaitTimer = 3f; // safety timeout in case the door never reports fully open
        while (door != null && !door.IsFullyOpen && doorWaitTimer > 0f)
        {
            doorWaitTimer -= Time.deltaTime;
            yield return null;
        }

        Log("Step 6: door clear, enemy entering.");
        if (enemy != null && insideRoomPoint != null) enemy.RevealAt(insideRoomPoint);
        if (enemy != null) enemy.SayLine(enterRoomLine, enterRoomLineDuration);
        yield return new WaitForSeconds(enterRoomLineDuration);

        // If the player is already hiding, let them know they're doing the right thing.
        // This checks once at the start of the search rather than continuously.
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
            yield return new WaitForSeconds(safeNowLineDuration);
        }

        HideLine();

        // 8. Either way, the enemy immediately heads back upstairs and resumes patrolling there
        Log("Step 8: enemy returns upstairs.");
        if (enemy != null) enemy.ReturnToUpstairsPatrol();
    }

    /// <summary>
    /// Freezes player input and walks them (via real CharacterController.Move calls, not a teleport)
    /// toward target, so they naturally pass through and trigger any safe-zone collider on the way.
    /// Has its own timeout so it can never run forever even if something is misconfigured.
    /// </summary>
    private IEnumerator PullPlayerBack(Transform target)
    {
        playerController.enabled = false; // stop normal input from fighting the scripted walk

        CharacterController cc = playerController.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogWarning("EnemyEncounterTrigger: Player has no CharacterController, can't pull it back.", this);
            playerController.enabled = true;
            yield break;
        }

        float elapsed = 0f;
        const float maxDuration = 6f; // safety timeout

        while (elapsed < maxDuration)
        {
            Vector3 toTarget = target.position - playerController.transform.position;
            toTarget.y = 0f; // horizontal distance only; gravity handles vertical separately

            if (toTarget.magnitude < 0.2f) break; // close enough

            Vector3 step = toTarget.normalized * pullBackSpeed * Time.deltaTime;
            cc.Move(step + Vector3.down * 0.05f); // small downward nudge keeps it grounded on slopes/steps

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerController.transform.rotation = target.rotation;
        playerController.ResetGravityVelocity();
        playerController.enabled = true; // give control back
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