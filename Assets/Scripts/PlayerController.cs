using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePosition; // Переменная для хранения позиции мыши

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 1. Считываем кнопки WASD
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. Считываем позицию мыши и переводим её из пикселей экрана в координаты игрового мира
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void FixedUpdate()
    {
        // 3. Двигаем игрока (как раньше)
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        // 4. Вычисляем направление от игрока к мышке
        Vector2 lookDir = mousePosition - rb.position;

        // 5. Математика: вычисляем угол поворота в градусах
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        // 6. Поворачиваем физическое тело игрока
        rb.rotation = angle;
    }
}