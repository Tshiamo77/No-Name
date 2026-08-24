using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverDelay = 3f;

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        SetRandomPatrolDestination();
    }

    private void Update()
    {
        // New Input System check for testing room invasion with 'T' key
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            DoorMovement testDoor = FindAnyObjectByType<DoorMovement>();
            if (testDoor != null)
            {
                StartRoomInvasion(testDoor);
            }
        }

        if (player == null) return;

        FPController fpController = player.GetComponent<FPController>();
        if (fpController != null && fpController.isHiding && currentState == EnemyState.Chase)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
            SetRandomPatrolDestination();
            hasSpokenOnSight = false;
        }

        if (currentState == EnemyState.Caught || isHandlingCatch) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= catchDistance)
        {
            StartCoroutine(CatchPlayerSequence());
            return;
        }

        if (CanSeePlayer())
        {
            currentState = EnemyState.Chase;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);

            if (!hasSpokenOnSight)
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

        FPController fp = player.GetComponent<FPController>();
        if (fp != null && fp.isHiding)
        {
            return false;
        }

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
            StopAllCoroutines();
            StartCoroutine(HideDialogueAfterDelay());
        }
    }

    private IEnumerator HideDialogueAfterDelay()
    {
        yield return new WaitForSeconds(quoteDisplayTime);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
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

            yield return new WaitForSeconds(0.1f);

            if (lifeManager.currentLives > 0)
            {
                if (initialSpawnPoint != null)
                {
                    CharacterController cc = lifeManager.GetComponent<CharacterController>();
                    FPController fpController = lifeManager.GetComponent<FPController>();

                    if (cc != null) cc.enabled = false;

                    lifeManager.transform.position = initialSpawnPoint.position;
                    lifeManager.transform.rotation = initialSpawnPoint.rotation;

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
                StartCoroutine(HandleGameOverSequence());
                yield break;
            }
        }

        yield return new WaitForSeconds(1.0f);
        isHandlingCatch = false;
    }

    public void StartRoomInvasion(DoorMovement targetDoor)
    {
        StartCoroutine(RoomInvasionRoutine(targetDoor));
    }

    private IEnumerator RoomInvasionRoutine(DoorMovement targetDoor)
    {
        if (targetDoor != null)
        {
            targetDoor.ToggleDoor();
        }

        if (warningPromptText != null)
        {
            warningPromptText.gameObject.SetActive(true);
            warningPromptText.text = "WARNING: Enemy approaching the room!";
        }

        yield return new WaitForSeconds(1.5f);

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

        float searchTimer = searchDuration;
        bool caughtPlayer = false;

        while (searchTimer > 0f)
        {
            searchTimer -= Time.deltaTime;

            FPController playerController = FindFirstObjectByType<FPController>();
            if (playerController != null)
            {
                if (playerController.isHiding)
                {
                    break;
                }
                else
                {
                    caughtPlayer = true;
                    break;
                }
            }
            yield return null;
        }

        if (caughtPlayer)
        {
            PlayerLifeManager lifeManager = FindFirstObjectByType<PlayerLifeManager>();
            if (lifeManager != null)
            {
                lifeManager.LoseLife();
            }
        }
    }

    public void TriggerRoomInvasion(DoorMovement door)
    {
        StartCoroutine(RoomInvasionRoutine(door));
    }

    private IEnumerator HandleGameOverSequence()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene("MAIN_MENU");
    }
}