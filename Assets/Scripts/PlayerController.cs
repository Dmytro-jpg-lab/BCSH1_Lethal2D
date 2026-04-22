using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseMoveSpeed = 5f;
    public float minMoveSpeed = 1.5f;
    public float sprintMultiplier = 1.6f;

    [Header("Weight Settings")]
    public float weightPenaltyLimit = 50f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    [SerializeField] private float baseStaminaDrain = 15f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float weightDrainPenalty = 0.5f;

    private float currentMoveSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePosition;
    private PlayerInventory inventory;

    // Состояния штрафа
    private bool isExhausted = false;
    private float exhaustionTimer = 0f;
    private const float penaltyDuration = 3f; // Длительность штрафа

    [Header("Player State")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isSprinting = false; 

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<PlayerInventory>();
        currentStamina = maxStamina;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        CalculateSpeedAndStamina();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * currentMoveSpeed * Time.fixedDeltaTime);

        Vector2 lookDir = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    private void CalculateSpeedAndStamina()
    {
        if (inventory == null) return;

        float currentWeight = inventory.totalWeight;
        float speedReduction = (currentWeight / weightPenaltyLimit) * baseMoveSpeed;
        float speedAfterWeight = Mathf.Clamp(baseMoveSpeed - speedReduction, minMoveSpeed, baseMoveSpeed);

        bool isMoving = movement.magnitude > 0;

        // Если стамина упала в 0 — ловим одышку
        if (currentStamina <= 0 && !isExhausted)
        {
            isExhausted = true;
            exhaustionTimer = penaltyDuration;
        }

        if (isExhausted)
        {
            // Таймер штрафа тикает
            exhaustionTimer -= Time.deltaTime;

            // Жестко ставим минимальную скорость
            currentMoveSpeed = minMoveSpeed;

            // ИСПРАВЛЕНИЕ: Восстанавливаем стамину даже пока мы в одышке!
            currentStamina += staminaRegenRate * Time.deltaTime;

            // Когда 3 секунды прошли — снимаем одышку
            if (exhaustionTimer <= 0)
            {
                isExhausted = false;
            }
        }
        else
        {
            // Обычный бег или спринт
            isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && isMoving;

            if (isSprinting)
            {
                currentMoveSpeed = speedAfterWeight * sprintMultiplier;
                float totalDrainRate = baseStaminaDrain + (currentWeight * weightDrainPenalty);
                currentStamina -= totalDrainRate * Time.deltaTime;
            }
            else
            {
                currentMoveSpeed = speedAfterWeight;
                // Восстанавливаем стамину при ходьбе/остановке
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        // Не даем стамине выйти за пределы 0-100
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    // --- НОВЫЙ БЛОК: ПОЛУЧЕНИЕ УРОНА ---
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        // Чтобы здоровье не ушло в минуса
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Монстр кусает! Здоровье: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("ИГРОК МЕРТВ!");
        // Позже мы добавим сюда экран Game Over или перезагрузку уровня
    }
}