using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Caught, Dormant }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Drag Encounter Settings")]
    [Tooltip("Assign the BoxCollider where the enemy drags the player back to across all rooms.")]
    [SerializeField] private BoxCollider dragDestinationCollider;
    [SerializeField] private float dragSpeed = 3.5f;
    [SerializeField] private float approachPlayerSpeed = 4.0f;

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
    [SerializeField] private Animator animator;

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

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

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
        UpdateAnimation();

        if (isRoomInvasionActive)
            return;

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

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        bool isMoving = !agent.isStopped && agent.velocity.sqrMagnitude > 0.05f;
        animator.SetBool("isWalking", isMoving);
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

    // Called by DoorMovement when opening an encounter room door.
    public void TriggerRoomInvasion(DoorMovement door)
    {
        if (isRoomInvasionActive)
            return;

        hasPlayedSafeVoice = false;
        StopWandering();

        StartCoroutine(DragPlayerEncounterRoutine());
    }

    // ------------------------------------------------
    // FREEZE-AND-DRAG ROOM ENCOUNTER (ALL ROOMS)
    // ------------------------------------------------

    private IEnumerator DragPlayerEncounterRoutine()
    {
        isRoomInvasionActive = true;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (playerController == null && player != null)
            playerController = player.GetComponent<FPController>();

        if (playerController != null)
            playerController.LockPlayerControl();

        if (warningPromptText != null)
        {
            warningPromptText.gameObject.SetActive(true);
            warningPromptText.text = "Something is coming...";
        }

        TriggerCreepyQuote();

        // 1. Enemy moves to the stationary player
        if (agent != null && player != null)
        {
            agent.isStopped = false;
            agent.speed = approachPlayerSpeed;

            while (Vector3.Distance(transform.position, player.position) > 1.35f)
            {
                agent.SetDestination(player.position);
                yield return null;
            }

            agent.isStopped = true;
        }

        if (warningPromptText != null)
            warningPromptText.gameObject.SetActive(false);

        // 2. Drag the locked player back to dragDestinationCollider
        if (player != null && dragDestinationCollider != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            if (animator != null)
                animator.SetBool("isWalking", true);

            Vector3 targetCenter = dragDestinationCollider.bounds.center;

            while (!dragDestinationCollider.bounds.Contains(player.position))
            {
                Vector3 stepTarget = new Vector3(targetCenter.x, transform.position.y, targetCenter.z);
                transform.position = Vector3.MoveTowards(transform.position, stepTarget, dragSpeed * Time.deltaTime);

                Vector3 moveDir = (stepTarget - transform.position).normalized;
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(moveDir),
                        Time.deltaTime * 8f
                    );
                }

                // Pull player behind the enemy
                Vector3 draggedPlayerPos = transform.position - transform.forward * 1.15f;
                draggedPlayerPos.y = player.position.y;
                player.position = draggedPlayerPos;

                // Keep player facing the enemy during the drag
                Vector3 lookAtEnemy = transform.position - player.position;
                lookAtEnemy.y = 0f;
                if (lookAtEnemy.sqrMagnitude > 0.001f)
                {
                    player.rotation = Quaternion.LookRotation(lookAtEnemy);
                }

                // Snap into the box collider once the enemy reaches it
                if (Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.z),
                        new Vector2(targetCenter.x, targetCenter.z)) < 0.3f)
                {
                    player.position = new Vector3(targetCenter.x, player.position.y, targetCenter.z);
                    break;
                }

                yield return null;
            }

            if (animator != null)
                animator.SetBool("isWalking", false);

            if (cc != null) cc.enabled = true;

            // Sync NavMeshAgent to the enemy's new world position
            if (agent != null)
            {
                agent.Warp(transform.position);
            }
        }
        else
        {
            Debug.LogWarning("Enemy.DragPlayerEncounterRoutine(): Assign 'Drag Destination Collider' on Enemy!", this);
        }

        // 3. Only now that the player is inside the BoxCollider, restore player control
        if (playerController != null)
            playerController.UnlockPlayerControl();

        isRoomInvasionActive = false;

        // Return enemy to patrol/dormant behavior after releasing the player
        if (currentState != EnemyState.Dormant && agent != null)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            SetRandomPatrolDestination();
        }
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
        if (hasPlayedSafeVoice) return;
        if (playerVoiceAudioSource == null || safeAfterHidingVoice == null) return;

        if (!playerVoiceAudioSource.gameObject.activeInHierarchy)
            playerVoiceAudioSource.gameObject.SetActive(true);

        playerVoiceAudioSource.enabled = true;
        playerVoiceAudioSource.mute = false;
        playerVoiceAudioSource.playOnAwake = false;
        playerVoiceAudioSource.loop = false;
        playerVoiceAudioSource.spatialBlend = 0f;
        playerVoiceAudioSource.volume = safeVoiceVolume;

        playerVoiceAudioSource.Stop();
        playerVoiceAudioSource.clip = safeAfterHidingVoice;
        playerVoiceAudioSource.Play();

        hasPlayedSafeVoice = true;
    }
}