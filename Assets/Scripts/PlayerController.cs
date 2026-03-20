using UnityEngine;

// Название класса совпадает с названием файла
public class PlayerController : MonoBehaviour 
{
    // Настройки скорости вынесены в инспектор
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    // Метод Start (PascalCase) вызывается один раз в начале
    private void Start()
    {
        // Инициализация компонента физики
        rb = GetComponent<Rigidbody2D>();
    }

    // Update вызывается каждый кадр
    private void Update()
    {
        // Считываем ввод пользователя (WASD или стрелочки)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    // FixedUpdate используется для физики (обеспечивает плавность)
    private void FixedUpdate()
    {
        // Вычисляем новую позицию и плавно перемещаем персонажа
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}