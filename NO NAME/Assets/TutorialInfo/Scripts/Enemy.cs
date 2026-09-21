using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Caught }
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
    private WaitForSeconds quoteDisplayWfs;

    private FPController playerController;
    private Coroutine dialogueRoutine;

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

        quoteDisplayWfs = new WaitForSeconds(quoteDisplayTime);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        SetRandomPatrolDestination();
    }

    private void Update()
    {
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

        string quote = creepyQuotes[Random.Range(0, creepyQuotes.Length)];

        if (dialoguePanel != null && quoteText != null)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = "Monster";
                speakerNameText.color = Color.red;
            }

            quoteText.text = quote;
            quoteText.color = Color.white;

            dialoguePanel.SetActive(true);

            // Only restart the dialogue timer. StopAllCoroutines() would also kill
            // the catch sequence and room invasion coroutines.
            if (dialogueRoutine != null) StopCoroutine(dialogueRoutine);
            dialogueRoutine = StartCoroutine(HideDialogueAfterDelay());
        }
    }

    private IEnumerator HideDialogueAfterDelay()
    {
        yield return quoteDisplayWfs;
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