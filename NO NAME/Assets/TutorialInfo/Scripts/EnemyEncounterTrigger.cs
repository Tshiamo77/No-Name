using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A one-shot scripted monster encounter, tied to a single door.
/// Wire Door's "On Door Opened ()" UnityEvent to this component's BeginEncounter().
/// Wire a PlayerZoneTrigger placed just inside the safe room to this component's NotifyPlayerSafe().
///
/// Sequence: baby sound + text -> monster approach sound -> brief glimpse of the enemy ->
/// "let me hide" text -> wait for player to return to the safe zone -> door auto-closes ->
/// hide countdown -> door force-opens, enemy enters and speaks -> enemy wanders the room while
/// searching -> catch, or the enemy gives up and leaves -> enemy relocates and goes fully
/// dormant until woken by the next encounter/trigger.
/// </summary>
public class EnemyEncounterTrigger : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private DoorMovement door; // the door this whole encounter is tied to
    [SerializeField] private FPController playerController;
    [SerializeField] private PlayerLifeManager lifeManager;

    [Header("Positions")]
    [SerializeField] private Transform revealPoint;       // where the enemy is briefly glimpsed
    [SerializeField] private Transform insideRoomPoint;    // where the enemy "enters" to after the countdown
    [SerializeField] private Transform[] searchWaypoints;  // points the enemy wanders between while searching the room
    [SerializeField] private Transform restPointAfter;     // where the enemy goes fully dormant afterward
    [SerializeField] private Transform playerReturnPoint;  // where the player is sent back to if caught

    [Header("Narrative Text (the PLAYER's own thoughts; can reuse your existing dialogue/thought UI)")]
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip babySoundClip;
    [SerializeField] private AudioClip monsterApproachClip;

    [Header("Timings")]
    [SerializeField] private float babyLineDuration = 2.5f;
    [SerializeField] private float approachDelay = 1.5f;
    [SerializeField] private float revealLineDuration = 2f;
    [SerializeField] private float hideCountdownDuration = 10f;
    [SerializeField] private float enterRoomLineDuration = 2.5f;
    [SerializeField] private float searchDuration = 8f;
    [SerializeField] private float giveUpLineDuration = 2.5f;
    [SerializeField] private float safeNowLineDuration = 3f;
    [SerializeField] private float catchLineDuration = 2.5f;

    [Header("Dialogue Lines - Player's thoughts (narrativeText)")]
    [SerializeField] private string babySoundLine = "What's that sound?";
    [SerializeField] private string revealLine = "WHAT'S THAT, Let me hide";
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

    /// <summary>Wire this to a PlayerZoneTrigger's On Player Entered () event, placed inside the safe room.</summary>
    public void NotifyPlayerSafe()
    {
        Log("NotifyPlayerSafe() received.");
        playerIsSafe = true;
    }

    private IEnumerator EncounterRoutine()
    {
        playerIsSafe = false;

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

        // 4. Wait for the player to run back inside; then the door closes itself
        Log("Step 4: waiting for the player to reach the safe zone (NotifyPlayerSafe)...");
        yield return new WaitUntil(() => playerIsSafe);
        Log("Step 4 done: player is safe. Closing door.");
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

        // 6. The door opens and the enemy enters, speaking its first line
        Log("Step 6: door opens, enemy enters.");
        if (door != null) door.ForceOpen();
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

        // 8. Either way, the enemy leaves and goes fully dormant until the next story beat
        Log("Step 8: enemy relocates and goes dormant.");
        if (enemy != null && restPointAfter != null) enemy.GoDormantAt(restPointAfter);
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