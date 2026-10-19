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

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float wanderSpeed = 1f; // used while searching a room during a scripted encounter

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

    [Header("Creepy Quotes")]
    [SerializeField] private float quoteTriggerDistance = 6f; // Enemy only speaks when this close to the player
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

    // Cached WaitForSeconds instances to avoid allocations
    private static readonly WaitForSeconds oneSecond = new WaitForSeconds(1.0f);
    private static readonly WaitForSeconds oneAndHalfSecond = new WaitForSeconds(1.5f);
    private static readonly WaitForSeconds shortWait = new WaitForSeconds(0.1f);

    private FPController playerController;
    private Coroutine dialogueRoutine;
    private Coroutine wanderRoutine;

    private bool IsPlayerHiding => playerController != null && playerController.isHiding;

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Cache the player's controller once instead of looking it up every frame
        if (player != null) playerController = player.GetComponent<FPController>();
        if (playerController == null) playerController = FindFirstObjectByType<FPController>();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (currentState == EnemyState.Dormant)
        {
            if (agent != null) agent.isStopped = true;
        }
        else
        {
            SetRandomPatrolDestination();
        }
    }

    private void Update()
    {
        // Completely inert until a scripted event wakes it up (GoDormantAt/RevealAt leave it
        // Dormant on purpose; only an outside script changing currentState resumes normal behaviour)
        if (currentState == EnemyState.Dormant) return;

        if (player == null) return;

        // Player hid while being chased: the enemy loses them
        if (IsPlayerHiding && currentState == EnemyState.Chase)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
            SetRandomPatrolDestination();
            hasSpokenOnSight = false;
        }

        if (currentState == EnemyState.Caught || isHandlingCatch) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // A hiding player can never be caught, no matter how close the enemy is
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

            // Only speak once per chase, and only when the enemy is actually close
            if (!hasSpokenOnSight && distanceToPlayer <= quoteTriggerDistance)
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
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
            float angleBetweenEnemyAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleBetweenEnemyAndPlayer <= visionAngle / 2f)
            {
                return true;
            }
        }
        return false;
    }

    private void TriggerCreepyQuote()
    {
        if (creepyQuotes == null || creepyQuotes.Length == 0) return;
        SayLine(creepyQuotes[Random.Range(0, creepyQuotes.Length)], quoteDisplayTime);
    }

    /// <summary>
    /// Makes the enemy speak a custom line through its own dialogue panel (separate from the
    /// player's own thoughts). Used for ambient creepy quotes and for scripted encounter dialogue
    /// like "I know you are in here".
    /// </summary>
    public void SayLine(string line, float duration)
    {
        if (dialoguePanel == null || quoteText == null) return;

        if (speakerNameText != null)
        {
            speakerNameText.text = "Monster";
            speakerNameText.color = Color.red;
        }

        quoteText.text = line;
        quoteText.color = Color.white;
        dialoguePanel.SetActive(true);

        // Only restart the dialogue timer. StopAllCoroutines() would also kill
        // the catch sequence and room invasion coroutines.
        if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
        dialogueRoutine = StartCoroutine(HideDialogueAfterDelay(duration));
    }

    /// <summary>Immediately hides the enemy's speech, cancelling its auto-hide timer if one is running.</summary>
    public void HideSpeech()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private IEnumerator HideDialogueAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        dialogueRoutine = null;
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    private IEnumerator CatchPlayerSequence()
    {
        isHandlingCatch = true;
        currentState = EnemyState.Caught;
        agent.isStopped = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        PlayerLifeManager lifeManager = FindFirstObjectByType<PlayerLifeManager>();
        if (lifeManager != null)
        {
            lifeManager.LoseLife();

            yield return shortWait;

            if (lifeManager.currentLives > 0)
            {
                if (initialSpawnPoint != null)
                {
                    CharacterController cc = lifeManager.GetComponent<CharacterController>();
                    FPController fpController = lifeManager.GetComponent<FPController>();

                    if (cc != null) cc.enabled = false;

                    lifeManager.transform.SetPositionAndRotation(initialSpawnPoint.position, initialSpawnPoint.rotation);
                    if (fpController != null)
                    {
                        fpController.ResetGravityVelocity();
                    }

                    if (cc != null) cc.enabled = true;
                }

                currentState = EnemyState.Patrol;
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                SetRandomPatrolDestination();
                hasSpokenOnSight = false;
            }
            else
            {
                // Triggers the blood drip and returns to Main Menu when lives hit 0
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
        {
            dialoguePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameOverManager.Instance.TriggerGameOver();
        yield break;
    }

    // Called by DoorMovement the first time the door is opened.
    // The door is already open at this point, so we must NOT toggle it again here.
    public void TriggerRoomInvasion(DoorMovement door)
    {
        StartCoroutine(RoomInvasionRoutine());
    }

    // --- SCRIPTED ENCOUNTER HELPERS (used by EnemyEncounterTrigger / EnemyRelocateZone) ---

    /// <summary>
    /// Instantly warps the enemy to a position and leaves it fully stationary and inert
    /// (ignores vision, chase, patrol) until something explicitly wakes it again.
    /// </summary>
    public void GoDormantAt(Transform point)
    {
        StopAllCoroutines(); // cancel any in-progress invasion/catch/wander routine tied to this enemy
        wanderRoutine = null;
        isHandlingCatch = false;
        hasSpokenOnSight = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (warningPromptText != null) warningPromptText.gameObject.SetActive(false);

        if (point == null)
        {
            Debug.LogWarning("Enemy.GoDormantAt() was called with no point assigned - the enemy will stay wherever it currently is.", this);
        }
        else if (agent != null)
        {
            bool warped = agent.Warp(point.position);
            if (!warped)
            {
                Debug.LogWarning($"Enemy.GoDormantAt(): agent.Warp() to '{point.name}' FAILED. That point is probably not on the baked NavMesh (check Window > AI > Navigation and re-bake, or move the point onto the floor).", this);
            }
            transform.rotation = point.rotation;
            agent.isStopped = true;
        }

        currentState = EnemyState.Dormant;
    }

    /// <summary>
    /// Instantly warps the enemy to a position so the player can see it, without chasing.
    /// Used for the brief "glimpse" beat and for the enemy "entering" a room in a scripted encounter.
    /// </summary>
    public void RevealAt(Transform point)
    {
        if (point == null)
        {
            Debug.LogWarning("Enemy.RevealAt() was called with no point assigned - the enemy will stay wherever it currently is.", this);
        }
        else if (agent != null)
        {
            bool warped = agent.Warp(point.position);
            if (!warped)
            {
                Debug.LogWarning($"Enemy.RevealAt(): agent.Warp() to '{point.name}' FAILED. That point is probably not on the baked NavMesh (check Window > AI > Navigation and re-bake, or move the point onto the floor).", this);
            }
            transform.rotation = point.rotation;
            agent.isStopped = true;
        }

        currentState = EnemyState.Dormant; // stays inert; the encounter script controls timing from here
    }

    /// <summary>
    /// Makes the enemy wander between the given points, one at a time, pausing briefly at each,
    /// looping until StopWandering() or GoDormantAt() is called. Used during a scripted search so
    /// the enemy visibly moves around the room instead of standing still.
    /// </summary>
    public void StartWandering(Transform[] waypoints)
    {
        StopWandering();

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("Enemy.StartWandering() was called with no Search Waypoints assigned - the enemy will just stand still instead of wandering. Assign some Transforms to Search Waypoints on EnemyEncounterTrigger.", this);
            return;
        }

        if (agent == null) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Enemy.StartWandering(): the agent is not currently on the NavMesh (its last Warp() probably failed), so it can't move.", this);
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
        if (agent != null) agent.isStopped = true;
    }

    private IEnumerator WanderLoop(Transform[] waypoints)
    {
        int index = 0;
        while (true)
        {
            Transform target = waypoints[index % waypoints.Length];
            if (target == null)
            {
                Debug.LogWarning($"Enemy.WanderLoop(): Search Waypoints element {index % waypoints.Length} is empty, skipping it.", this);
            }
            else if (!agent.SetDestination(target.position))
            {
                Debug.LogWarning($"Enemy.WanderLoop(): couldn't path to '{target.name}'. It's probably not on the NavMesh or unreachable from here.", this);
            }

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f); // brief pause at each point before moving on
            index++;
        }
    }

    private IEnumerator RoomInvasionRoutine()
    {
        if (warningPromptText != null)
        {
            warningPromptText.gameObject.SetActive(true);
            warningPromptText.text = "WARNING: Enemy approaching the room!";
        }

        yield return oneAndHalfSecond;

        float timer = hideCountdown;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (warningPromptText != null)
            {
                warningPromptText.text = $"Hide quickly! Time remaining: {Mathf.Ceil(timer)}s";
            }
            yield return null;
        }

        if (warningPromptText != null)
        {
            warningPromptText.gameObject.SetActive(false);
        }

        // Search phase: the player must be hiding when the search starts and stay hidden until it ends.
        // (The check always runs at least once, even if searchDuration is 0.)
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
        }
        while (searchTimer > 0f);

        if (caughtPlayer)
        {
            PlayerLifeManager lifeManager = FindFirstObjectByType<PlayerLifeManager>();
            if (lifeManager != null)
            {
                lifeManager.LoseLife();
            }
        }
    }
}