using Pathfinding;
using UnityEngine;

public class BlindHearerAI : MonoBehaviour
{
    [Header("Speed Settings")]
    public float patrolSpeed = 1.5f;
    public float investigateSpeed = 3.5f;
    public float chaseSpeed = 4.2f;

    [Header("Hearing Settings")]
    public float sprintHearingRadius = 15f;
    public float noiseInvestigateDuration = 3f;

    [Header("Взаимодействие с дверьми")]
    public LayerMask doorLayer; // Слой Door

    [Header("Attack Settings")]
    public float attackDistance = 1.2f; // Дистанция удара (настрой так, чтобы доставал вплотную)
    public float attackCooldown = 1.0f; // Перерыв между ударами (1 секунда)
    private float lastAttackTime = 0f;

    private enum State { Patrol, Investigate, Chase }
    private State currentState = State.Patrol;

    private Transform player;
    private PlayerController playerController;
    private AIPath ai;

    private float waitTimer = 0f;

    public static BlindHearerAI Instance;

    // Добавь это куда-нибудь наверх, к остальным переменным
    public float maxPatrolTime = 7f;
    private float stuckTimer = 0f;

    private void Awake() { Instance = this; }

    private void Start()
    {
        ai = GetComponent<AIPath>();
        ai.canMove = true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerController = p.GetComponent<PlayerController>();
        }

        PickNewPatrolPoint();
    }

    private void Update()
    {
        if (player == null) return;
        ListenForPlayer();
        HandleStatesLogic();
        ForceTouchDoors();
        TryAttackPlayer();
    }

    // --- НОВАЯ, ЖЕЛЕЗОБЕТОННАЯ ЛОГИКА ДВЕРЕЙ ---
    // Срабатывает автоматически, когда коллайдер монстра касается другого коллайдера
    private void ForceTouchDoors()
    {
        // 1. Ищем ВСЕ объекты на слое Door в радиусе 2 метров
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.0f, doorLayer);

        foreach (var hit in hitColliders)
        {
            // Ищем скрипт двери. Если он на родителе — GetComponentInParent поможет
            DoorController door = hit.GetComponent<DoorController>();

            if (door == null)
                door = hit.GetComponentInParent<DoorController>();

            if (door != null)
            {
                // ТОТ САМЫЙ ЭФФЕКТ "НАЖАТОЙ Е":
                // Если дверь закрыта — принудительно открываем
                if (!door.isOpen)
                {
                    Debug.Log("МОНСТР: Вижу закрытую дверь " + hit.gameObject.name + ". Открываю!");
                    door.MonsterOpenDoor();
                    return; // Нашли одну дверь, открыли — выходим
                }
            }
        }
    }
    // -------------------------------------------

    private void ListenForPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (playerController.isSprinting && distanceToPlayer <= sprintHearingRadius)
        {
            currentState = State.Chase;
            ai.destination = player.position;
        }
        else if (currentState == State.Chase && !playerController.isSprinting)
        {
            currentState = State.Investigate;
            waitTimer = noiseInvestigateDuration;
        }
    }

    private void HandleStatesLogic()
    {
        switch (currentState)
        {
            case State.Chase:
                ai.maxSpeed = chaseSpeed;
                ai.destination = player.position;
                break;

            case State.Investigate:
                ai.maxSpeed = investigateSpeed;
                if (ai.reachedDestination)
                {
                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0) PickNewPatrolPoint();
                }
                break;

            case State.Patrol:
                ai.maxSpeed = patrolSpeed;
                stuckTimer += Time.deltaTime; // Таймер тикает, пока он идет

                // Если плагин говорит, что дошли ИЛИ таймер истек (застрял)
                if (ai.reachedDestination || stuckTimer >= maxPatrolTime)
                {
                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0)
                    {
                        PickNewPatrolPoint();
                        stuckTimer = 0f; // Сбрасываем таймер для новой точки
                    }
                }
                break;
        }
    }

    private void PickNewPatrolPoint()
    {
        stuckTimer = 0f;
        Vector2 randomDir = Random.insideUnitCircle * Random.Range(4f, 8f);
        Vector3 randomPoint = transform.position + new Vector3(randomDir.x, randomDir.y, 0);

        GraphNode node = AstarPath.active.GetNearest(randomPoint, NNConstraint.Default).node;

        if (node != null)
        {
            ai.destination = (Vector3)node.position;
        }

        currentState = State.Patrol;
        waitTimer = Random.Range(1f, 2.5f);
    }

    public static void MakeNoise(Vector2 noisePosition, float volumeRadius)
    {
        if (Instance == null || Instance.currentState == State.Chase) return;

        float distance = Vector2.Distance(Instance.transform.position, noisePosition);
        if (distance <= volumeRadius)
        {
            Instance.currentState = State.Investigate;
            Instance.ai.destination = noisePosition;
            Instance.waitTimer = Instance.noiseInvestigateDuration;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }

    private void TryAttackPlayer()
    {
        if (player == null || playerController == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Если подошли достаточно близко И кулдаун атаки прошел
        if (distanceToPlayer <= attackDistance && Time.time >= lastAttackTime + attackCooldown)
        {
            // Сносим 25 ХП (4 удара = 100 ХП)
            playerController.TakeDamage(25f);
            lastAttackTime = Time.time;
        }
    }


}