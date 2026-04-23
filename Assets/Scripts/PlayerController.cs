using UnityEngine;
using UnityEngine.SceneManagement;

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
    private PlayerInteract interact; // Ссылка на скрипт сканера/взаимодействия

    // Состояния штрафа
    private bool isExhausted = false;
    private float exhaustionTimer = 0f;
    private const float penaltyDuration = 3f;

    [Header("Player State")]
    public float maxHealth = 100f; // У тебя 100 хп, значит монстр должен бить по 25
    public float currentHealth;
    public bool isSprinting = false;
    public bool isDead = false; // Флаг смерти

    [Header("UI Смерти")]
    public GameObject gameOverUI; // Сюда кидай черную панель "ВЫ ПОГИБЛИ"

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<PlayerInventory>();
        interact = GetComponent<PlayerInteract>(); // Находим скрипт радара

        currentStamina = maxStamina;
        currentHealth = maxHealth;

        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    private void Update()
    {
        // Если мертв - ждем только кнопку R
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return; // Выходим из Update, чтобы не бегать
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        CalculateSpeedAndStamina();
    }

    private void FixedUpdate()
    {
        if (isDead) return; // Мертвые не двигаются

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

        if (currentStamina <= 0 && !isExhausted)
        {
            isExhausted = true;
            exhaustionTimer = penaltyDuration;
        }

        if (isExhausted)
        {
            exhaustionTimer -= Time.deltaTime;
            currentMoveSpeed = minMoveSpeed;
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (exhaustionTimer <= 0) isExhausted = false;
        }
        else
        {
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
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Монстр кусает! Здоровье: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("ИГРОК МЕРТВ!");

        // 1. Вырубаем радар и возможность открывать двери
        if (interact != null) interact.enabled = false;

        // 2. Включаем UI смерти
        if (gameOverUI != null) gameOverUI.SetActive(true);

        // 3. Красим игрока в цвет крови/тлена
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.red;

        // 4. Останавливаем физику
        rb.linearVelocity = Vector2.zero;
    }
}