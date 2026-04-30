using Pathfinding;
using UnityEngine;

public class CoilHeadAI : MonoBehaviour
{
    [Header("Настройки скорости")]
    public float chaseSpeed = 8.0f; // Он должен быть ОЧЕНЬ быстрым

    [Header("Взаимодействие с дверьми")]
    public LayerMask doorLayer; // Слой Door

    [Header("Настройки зрения (SCP механика)")]
    public float agroRadius = 15f;  // Со скольки метров он вообще начинает охоту
    public float loseRadius = 25f;  // На каком расстоянии он отстанет, если убежать
    [Range(10f, 360f)]
    public float playerFOV = 110f;  // Угол зрения игрока (110 - отлично для бокового зрения)
    public LayerMask obstacleLayer; // Слой стен (чтобы прятаться за ними)

    [Header("Настройки атаки")]
    public float attackDistance = 1.2f;
    public float attackCooldown = 1.0f;
    private float lastAttackTime = 0f;

    private Transform player;
    private PlayerController playerController;
    private IAstarAI ai; // Используем интерфейс, чтобы работал и AIPath, и AILerp

    private bool isChasing = false;

    private void Start()
    {
        ai = GetComponent<IAstarAI>();
        ai.maxSpeed = chaseSpeed;
        ai.canMove = false; // Изначально стоит как статуя

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerController = p.GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. Проверяем, смотрит ли игрок на монстра
        bool isBeingWatched = CheckIfPlayerIsLooking(distanceToPlayer);

        // 2. Логика поведения
        if (isBeingWatched)
        {
            // Игрок смотрит прямо на него (и нет стен) - ЗАМИРАЕМ
            ai.canMove = false;
        }
        else
        {
            // Игрок отвернулся или зашел за стену
            if (distanceToPlayer <= agroRadius)
            {
                isChasing = true; // Триггерим агрессию
            }
            else if (distanceToPlayer > loseRadius)
            {
                isChasing = false; // Игрок убежал слишком далеко, успокаиваемся
            }

            // Если он в состоянии погони и на него не смотрят - БЕЖИМ
            if (isChasing)
            {
                ai.canMove = true;
                ai.destination = player.position;

                TryAttackPlayer(distanceToPlayer);
                ForceTouchDoors();
            }
            else
            {
                // Не в погоне (просто стоит далеко)
                ai.canMove = false;
            }
        }
    }

    // Тот самый украденный код из твоего радара
    private bool CheckIfPlayerIsLooking(float distanceToPlayer)
    {
        // Если монстр слишком далеко, считаем, что игрок его не видит в темноте
        if (distanceToPlayer > loseRadius) return false;

        Vector2 dirToMonster = (transform.position - player.position).normalized;
        Vector2 playerForward = player.up; // Направление взгляда игрока

        float angleToMonster = Vector2.Angle(playerForward, dirToMonster);

        // Попадает ли монстр в конус зрения игрока?
        if (angleToMonster < playerFOV / 2f)
        {
            // Пускаем луч от игрока к монстру, чтобы проверить стены
            RaycastHit2D hit = Physics2D.Raycast(player.position, dirToMonster, distanceToPlayer, obstacleLayer);

            // Если луч НЕ столкнулся со стеной, значит игрок видит монстра!
            if (hit.collider == null)
            {
                return true;
            }
        }

        return false; // Или отвернулся, или между ними стена
    }

    private void TryAttackPlayer(float distance)
    {
        if (playerController == null) return;

        // Монстр может бить ТОЛЬКО если он может двигаться (когда на него не смотрят)
        if (distance <= attackDistance && Time.time >= lastAttackTime + attackCooldown)
        {
            playerController.TakeDamage(25f);
            lastAttackTime = Time.time;
            Debug.Log("SCP УДАРИЛ ИГРОКА!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, agroRadius);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
    private void ForceTouchDoors()
    {
        // Ищем двери в радиусе 2 метров
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.0f, doorLayer);

        foreach (var hit in hitColliders)
        {
            DoorController door = hit.GetComponent<DoorController>();
            if (door == null) door = hit.GetComponentInParent<DoorController>();

            if (door != null && !door.isOpen)
            {
                door.MonsterOpenDoor();
                return;
            }
        }
    }
}