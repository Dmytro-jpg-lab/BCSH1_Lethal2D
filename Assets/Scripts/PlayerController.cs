using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки скорости")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float minMoveSpeed = 1.5f;
    [SerializeField] private float sprintMultiplier = 1.6f;

    [Header("Настройки веса")]
    [SerializeField] public float weightPenaltyLimit = 50f;

    [Header("Настройки выносливости (Спринт)")]
    public float maxStamina = 100f;
    public float currentStamina;
    [SerializeField] private float baseStaminaDrain = 15f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float weightDrainPenalty = 0.5f;

    [Header("Состояние игрока")]
    public float maxHealth = 100f;
    public float currentHealth;

    private float currentMoveSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePosition;

    private PlayerInventory inventory;

    // НОВАЯ ПЕРЕМЕННАЯ: Флаг одышки
    private bool isExhausted = false;

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
        float speedAfterWeight = baseMoveSpeed - speedReduction;
        speedAfterWeight = Mathf.Clamp(speedAfterWeight, minMoveSpeed, baseMoveSpeed);

        bool isMoving = movement.magnitude > 0;

        // --- ИСПРАВЛЕННАЯ ЛОГИКА ОДЫШКИ ---

        // Если стамина кончилась — вешаем статус одышки
        if (currentStamina <= 0)
        {
            isExhausted = true;
        }

        // Если игрок отпустил Shift и у него есть хоть немного стамины — снимаем одышку
        if (!Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            isExhausted = false;
        }

        // Теперь для спринта нужно, чтобы игрок НЕ был в состоянии одышки
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isExhausted && currentStamina > 0 && isMoving;

        if (isSprinting)
        {
            currentMoveSpeed = speedAfterWeight * sprintMultiplier;
            float totalDrainRate = baseStaminaDrain + (currentWeight * weightDrainPenalty);
            currentStamina -= totalDrainRate * Time.deltaTime;
        }
        else
        {
            currentMoveSpeed = speedAfterWeight;

            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    // Добавь этот метод в любое место класса (например, в самый конец)
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"Мерта получил урон! Осталось HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Мерта погиб...");
            // Здесь потом добавим логику смерти (рестарт или экран смерти)
        }
    }
}