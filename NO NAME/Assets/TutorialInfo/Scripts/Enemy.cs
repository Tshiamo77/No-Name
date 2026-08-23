using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

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
    [SerializeField] private float visionDistance = 12f;
    [SerializeField][Range(0, 180)] private float visionAngle = 60f;

    [Header("Catch Settings")]
    [SerializeField] private float catchDistance = 1.8f;
    [SerializeField] private Transform initialSpawnPoint;

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
    [SerializeField] private GameObject dialoguePanel; // Drag your DialoguePanel here
    [SerializeField] private TextMeshProUGUI speakerNameText; // Drag your SpeakerNameText here ("Monster")
    [SerializeField] private TextMeshProUGUI quoteText; // Drag your QuoteText here
    [SerializeField] private float quoteDisplayTime = 3f;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverDelay = 3f;

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Hide dialogue boxes at start
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        SetRandomPatrolDestination();
    }

    private void Update()
    {
        if (player == null || currentState == EnemyState.Caught) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
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
                if (Physics.Raycast(transform.position + Vector3.up * 1f, directionToPlayer.normalized, out RaycastHit hit, visionDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        return true;
                    }
                }
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
            if (speakerNameText != null) speakerNameText.text = "Monster";
            quoteText.text = quote;

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

    private void CatchPlayer()
    {
        currentState = EnemyState.Caught;
        agent.isStopped = true;

        // Hide dialogue if caught
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        Debug.Log("<color=red><b>[CAUGHT!] The enemy has grabbed you!</b></color>");

        PlayerLifeManager lifeManager = FindFirstObjectByType<PlayerLifeManager>();
        if (lifeManager != null)
        {
            lifeManager.LoseLife();

            if (lifeManager.currentLives > 0)
            {
                if (initialSpawnPoint != null)
                {
                    CharacterController cc = lifeManager.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    lifeManager.transform.position = initialSpawnPoint.position;

                    if (cc != null) cc.enabled = true;
                }

                currentState = EnemyState.Patrol;
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                SetRandomPatrolDestination();
                hasSpokenOnSight = false; // Fixed typo here
            }
            else
            {
                StartCoroutine(HandleGameOverSequence());
            }
        }
    }

    private IEnumerator HandleGameOverSequence()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("<color=black><b>GAME OVER: You died! You let the monster consume you.</b></color>");

        yield return new WaitForSeconds(gameOverDelay);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

