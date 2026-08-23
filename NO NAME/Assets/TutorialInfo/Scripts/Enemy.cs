using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Caught }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolWaitTime = 2f;
    private float waitTimer;

    [Header("Vision Settings")]
    [SerializeField] private float visionDistance = 15f;
    [SerializeField][Range(0, 180)] private float visionAngle = 90f;

    [Header("Catch Settings")]
    [SerializeField] private float catchDistance = 2.0f;
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
        if (player == null || currentState == EnemyState.Caught || isHandlingCatch) return;

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
        Debug.Log($"<color=red><b>ENEMY SEES YOU:</b></color> \"{quote}\"");

        if (dialoguePanel != null && quoteText != null)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = "Monster";
                speakerNameText.color = Color.red;
            }

            quoteText.text = quote;
            quoteText.color = Color.white; // Forces text color to bright white so it's visible on screen

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

        Debug.Log("<color=red><b>[CAUGHT!] The enemy has grabbed you!</b></color>");

        PlayerLifeManager lifeManager = FindFirstObjectByType<PlayerLifeManager>();
        if (lifeManager != null)
        {
            lifeManager.LoseLife();

            yield return new WaitForSeconds(0.1f); // Brief pause to secure life decrement

            if (lifeManager.currentLives > 0)
            {
                if (initialSpawnPoint != null)
                {
                    CharacterController cc = lifeManager.GetComponent<CharacterController>();
                    FPController fpController = lifeManager.GetComponent<FPController>();

                    // 1. Disable character controller to move safely
                    if (cc != null) cc.enabled = false;

                    // 2. Teleport player to the spawn point
                    lifeManager.transform.position = initialSpawnPoint.position;
                    lifeManager.transform.rotation = initialSpawnPoint.rotation;

                    // 3. Clear all falling velocity so player doesn't fly away
                    if (fpController != null)
                    {
                        fpController.ResetGravityVelocity();
                    }

                    // 4. Re-enable character controller
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

    private IEnumerator HandleGameOverSequence()
    {
        Debug.Log("<color=red>[GAME OVER] Lives reached 0. Initiating Game Over sequence...</color>");

        // 1. Hide dialogue immediately
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 2. Unlock and show the mouse cursor so players can interact with menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Safely turn on the Game Over panel if it exists
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[WARNING] Game Over Panel is not assigned in the Inspector, but proceeding to load Main Menu.");
        }

        // 4. Wait for the designated delay time
        yield return new WaitForSeconds(gameOverDelay);

        // 5. Force load the main menu scene
        Debug.Log("[GAME OVER] Loading scene: MAIN_MENU");
        SceneManager.LoadScene("MAIN_MENU");
    }


}

