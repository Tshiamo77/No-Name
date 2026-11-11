
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Caught, Dormant }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Room Invasion & Countdown")]
    [SerializeField] private float hideCountdown = 10f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private TextMeshProUGUI warningPromptText;

    private bool isRoomInvasionActive = false;

    [Header("Player Safe Voice")]
    [SerializeField] private AudioSource playerVoiceAudioSource;
    [SerializeField] private AudioClip safeAfterHidingVoice;
    [SerializeField, Range(0f, 1f)] private float safeVoiceVolume = 1f;

    private bool hasPlayedSafeVoice = false;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float wanderSpeed = 1f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolWaitTime = 2f;
    private float waitTimer;

    [Header("Vision Settings")]
    [SerializeField] private float visionDistance = 15f;
    [SerializeField][Range(0, 180)] private float visionAngle = 90f;

    [Header("Catch Settings")]
    [SerializeField] private float catchDistance = 1.0f;
    [SerializeField] private Transform initialSpawnPoint;
    private bool isHandlingCatch = false;

    [Header("Upstairs Home (where the enemy idles/returns to by default)")]
    [SerializeField] private Transform upstairsReturnPoint;
    [SerializeField] private Transform[] upstairsWaypoints;

    [Header("Downstairs Home (used when the player goes upstairs)")]
    [SerializeField] private Transform downstairsReturnPoint;
    [SerializeField] private Transform[] downstairsWaypoints;

    [Header("Creepy Quotes")]
    [SerializeField] private float quoteTriggerDistance = 6f;

    [SerializeField]
    private string[] creepyQuotes = new string[]
    {
        "You cannot leave this house...",
        "I can hear your heartbeat.",
        "There is no escape.",
        "You're only delaying the inevitable."
    };

    private bool hasSpokenOnSight = false;

    [Header("Dialogue UI Reference")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI quoteText;
    [SerializeField] private float quoteDisplayTime = 3f;

    private static readonly WaitForSeconds oneSecond =
        new WaitForSeconds(1f);

    private static readonly WaitForSeconds oneAndHalfSecond =
        new WaitForSeconds(1.5f);

    private static readonly WaitForSeconds shortWait =
        new WaitForSeconds(0.1f);

    private FPController playerController;
    private Coroutine dialogueRoutine;
    private Coroutine wanderRoutine;

    private bool IsPlayerHiding =>
        playerController != null && playerController.isHiding;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (player != null)
            playerController = player.GetComponent<FPController>();

        if (playerController == null)
            playerController = FindFirstObjectByType<FPController>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerVoiceAudioSource != null)
        {
            playerVoiceAudioSource.playOnAwake = false;
            playerVoiceAudioSource.loop = false;
            playerVoiceAudioSource.spatialBlend = 0f;
        }

        if (currentState == EnemyState.Dormant)
        {
            if (agent != null)
                agent.isStopped = true;

            if (upstairsWaypoints != null &&
                upstairsWaypoints.Length > 0)
            {
                StartWandering(upstairsWaypoints);
            }
        }
        else
        {
            SetRandomPatrolDestination();
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dormant)
            return;

        if (player == null)
            return;

        if (IsPlayerHiding && currentState == EnemyState.Chase)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
            SetRandomPatrolDestination();
            hasSpokenOnSight = false;
        }

        if (currentState == EnemyState.Caught || isHandlingCatch)
            return;

        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (!IsPlayerHiding && distanceToPlayer <= catchDistance)
        {
            StartCoroutine(CatchPlayerSequence());
            return;
        }

        if (CanSeePlayer())
        {
            currentState = EnemyState.Chase;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);

            if (!hasSpokenOnSight &&
                distanceToPlayer <= quoteTriggerDistance)
            {
                TriggerCreepyQuote();
                hasSpokenOnSight = true;
            }
        }
        else
        {
            if (currentState == EnemyState.Chase)
            {
                currentState = EnemyState.Patrol;
                agent.speed = patrolSpeed;
                SetRandomPatrolDestination();
                hasSpokenOnSight = false;
            }
        }

        if (currentState == EnemyState.Patrol)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                waitTimer += Time.deltaTime;

                if (waitTimer >= patrolWaitTime)
                {
                    SetRandomPatrolDestination();
                    waitTimer = 0f;
                }
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        if (IsPlayerHiding) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= visionDistance)
        {
            float angleBetweenEnemyAndPlayer =
                Vector3.Angle(transform.forward, directionToPlayer);

            if (angleBetweenEnemyAndPlayer <= visionAngle / 2f)
                return true;
        }

        return false;
    }

    private void TriggerCreepyQuote()
    {
        if (creepyQuotes == null || creepyQuotes.Length == 0)
            return;

        SayLine(
            creepyQuotes[Random.Range(0, creepyQuotes.Length)],
            quoteDisplayTime
        );
    }

    public void SayLine(string line, float duration)
    {
        if (dialoguePanel == null || quoteText == null)
            return;

        if (speakerNameText != null)
        {
            speakerNameText.text = "Monster";
            speakerNameText.color = Color.red;
        }

        quoteText.text = line;
        quoteText.color = Color.white;
        dialoguePanel.SetActive(true);

        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(HideDialogueAfterDelay(duration));
    }

    public void HideSpeech()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private IEnumerator HideDialogueAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        dialogueRoutine = null;
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere * patrolRadius;

        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit navHit,
            patrolRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    private IEnumerator CatchPlayerSequence()
    {
        isHandlingCatch = true;
        currentState = EnemyState.Caught;
        agent.isStopped = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        PlayerLifeManager lifeManager =
            FindFirstObjectByType<PlayerLifeManager>();

        if (lifeManager != null)
        {
            lifeManager.LoseLife();

            yield return shortWait;

            if (lifeManager.currentLives > 0)
            {
                if (initialSpawnPoint != null)
                {
                    CharacterController cc =
                        lifeManager.GetComponent<CharacterController>();

                    FPController fpController =
                        lifeManager.GetComponent<FPController>();

                    if (cc != null)
                        cc.enabled = false;

                    lifeManager.transform.SetPositionAndRotation(
                        initialSpawnPoint.position,
                        initialSpawnPoint.rotation
                    );

                    if (fpController != null)
                        fpController.ResetGravityVelocity();

                    if (cc != null)
                        cc.enabled = true;
                }

                currentState = EnemyState.Patrol;
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                SetRandomPatrolDestination();
                hasSpokenOnSight = false;
            }
            else
            {
                StartCoroutine(HandleGameOverSequence());
                yield break;
            }
        }

        yield return oneSecond;
        isHandlingCatch = false;
    }

    private IEnumerator HandleGameOverSequence()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameOverManager.Instance.TriggerGameOver();
        yield break;
    }

    // Called by DoorMovement or triggers when opening an encounter room door.
    public void TriggerRoomInvasion(DoorMovement door)
    {
        if (isRoomInvasionActive)
            return;

        hasPlayedSafeVoice = false;

        StartCoroutine(RoomInvasionRoutine());
    }

    // ------------------------------------------------
    // SCRIPTED ENCOUNTER & ENDING HELPERS
    // ------------------------------------------------

    public void FaceToFaceConfrontation(
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        StopAllCoroutines();

        if (agent != null)
        {
            agent.Warp(targetPosition);
            agent.isStopped = true;
        }

        transform.rotation = targetRotation;
        currentState = EnemyState.Dormant;
    }

    public void GoDormantAt(Transform point)
    {
        StopAllCoroutines();

        wanderRoutine = null;
        isHandlingCatch = false;
        hasSpokenOnSight = false;
        isRoomInvasionActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (warningPromptText != null)
            warningPromptText.gameObject.SetActive(false);

        if (point == null)
        {
            Debug.LogWarning(
                "Enemy.GoDormantAt() was called with no point assigned.",
                this
            );
        }
        else if (agent != null)
        {
            bool warped = agent.Warp(point.position);

            if (!warped)
            {
                Debug.LogWarning(
                    $"Enemy.GoDormantAt(): agent.Warp() to '{point.name}' FAILED.",
                    this
                );
            }

            transform.rotation = point.rotation;
            agent.isStopped = true;
        }

        currentState = EnemyState.Dormant;
    }

    public void RevealAt(Transform point)
    {
        StopWandering();

        if (point == null)
        {
            Debug.LogWarning(
                "Enemy.RevealAt(): no point assigned.",
                this
            );
        }
        else if (agent != null)
        {
            bool warped = agent.Warp(point.position);

            if (!warped)
            {
                Debug.LogWarning(
                    $"Enemy.RevealAt(): agent.Warp() to '{point.name}' FAILED.",
                    this
                );
            }

            transform.rotation = point.rotation;
            agent.isStopped = true;
        }

        currentState = EnemyState.Dormant;
    }

    public void RelocateAndWander(
        Transform entryPoint,
        Transform[] waypoints)
    {
        GoDormantAt(entryPoint);
        StartWandering(waypoints);
    }

    public void GoUpstairs()
    {
        RelocateAndWander(upstairsReturnPoint, upstairsWaypoints);
    }

    public void GoDownstairs()
    {
        RelocateAndWander(downstairsReturnPoint, downstairsWaypoints);
    }

    public void ReturnToUpstairsPatrol()
    {
        GoUpstairs();
    }

    public void StartWandering(Transform[] waypoints)
    {
        StopWandering();

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning(
                "Enemy.StartWandering(): no Search Waypoints assigned.",
                this
            );
            return;
        }

        if (agent == null) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                "Enemy.StartWandering(): agent is not on the NavMesh.",
                this
            );
            return;
        }

        agent.speed = wanderSpeed;
        agent.isStopped = false;

        wanderRoutine = StartCoroutine(WanderLoop(waypoints));
    }

    public void StopWandering()
    {
        if (wanderRoutine != null)
        {
            StopCoroutine(wanderRoutine);
            wanderRoutine = null;
        }

        if (agent != null)
            agent.isStopped = true;
    }

    private IEnumerator WanderLoop(Transform[] waypoints)
    {
        int index = 0;

        while (true)
        {
            Transform target = waypoints[index % waypoints.Length];

            if (target == null)
            {
                Debug.LogWarning(
                    $"Enemy.WanderLoop(): Search Waypoint {index % waypoints.Length} is empty."
                );
            }
            else if (!agent.SetDestination(target.position))
            {
                Debug.LogWarning(
                    $"Enemy.WanderLoop(): couldn't path to '{target.name}'."
                );
            }

            while (agent.pathPending ||
                   agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            index++;
        }
    }

    // ------------------------------------------------
    // PLAYER SAFE VOICE
    // ------------------------------------------------

    private void PlaySafeVoice()
    {
        Debug.Log("PLAY SAFE VOICE METHOD CALLED!", this);

        if (hasPlayedSafeVoice)
        {
            Debug.LogWarning("Safe voice has already played.", this);
            return;
        }

        if (playerVoiceAudioSource == null)
        {
            Debug.LogError(
                "Player Voice Audio Source is not assigned!",
                this
            );
            return;
        }

        if (safeAfterHidingVoice == null)
        {
            Debug.LogError(
                "Safe After Hiding Voice clip is not assigned!",
                this
            );
            return;
        }

        // Make sure the AudioSource can play.
        if (!playerVoiceAudioSource.gameObject.activeInHierarchy)
        {
            playerVoiceAudioSource.gameObject.SetActive(true);
        }

        playerVoiceAudioSource.enabled = true;
        playerVoiceAudioSource.mute = false;
        playerVoiceAudioSource.playOnAwake = false;
        playerVoiceAudioSource.loop = false;

        // Play the voice as 2D audio.
        playerVoiceAudioSource.spatialBlend = 0f;
        playerVoiceAudioSource.volume = safeVoiceVolume;

        // Stop any previous sound on this dedicated voice source.
        playerVoiceAudioSource.Stop();

        // Assign and play the voice directly.
        playerVoiceAudioSource.clip = safeAfterHidingVoice;
        playerVoiceAudioSource.Play();

        hasPlayedSafeVoice = true;

        Debug.Log(
            "SAFE VOICE AUDIO COMMAND SENT! Clip: " +
            safeAfterHidingVoice.name +
            " | AudioSource playing: " +
            playerVoiceAudioSource.isPlaying,
            this
        );
    }

    // ------------------------------------------------
    // ROOM INVASION AND PLAYER HIDING
    // ------------------------------------------------

    private IEnumerator RoomInvasionRoutine()
    {
        isRoomInvasionActive = true;

        Debug.Log("Room invasion started.", this);

        if (warningPromptText != null)
        {
            warningPromptText.gameObject.SetActive(true);
            warningPromptText.text =
                "WARNING: Enemy approaching the room!";
        }

        yield return oneAndHalfSecond;

        // Give the player 10 seconds to hide.
        float timer = hideCountdown;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (warningPromptText != null)
            {
                warningPromptText.text =
                    $"Hide quickly! Time remaining: {Mathf.Ceil(timer)}s";
            }

            yield return null;
        }

        if (warningPromptText != null)
            warningPromptText.gameObject.SetActive(false);

        // Enemy searches for 5 seconds.
        // Player must remain hidden for the entire search.
        float searchTimer = searchDuration;
        bool caughtPlayer = false;

        do
        {
            if (!IsPlayerHiding)
            {
                caughtPlayer = true;
                break;
            }

            searchTimer -= Time.deltaTime;

            yield return null;

        } while (searchTimer > 0f);

        if (caughtPlayer)
        {
            Debug.Log("Player was not hiding during the enemy search.", this);

            PlayerLifeManager lifeManager =
                FindFirstObjectByType<PlayerLifeManager>();

            if (lifeManager != null)
                lifeManager.LoseLife();
        }
        else
        {
            Debug.Log("Player survived the enemy search.", this);

            PlaySafeVoice();
        }

        isRoomInvasionActive = false;
    }
}
