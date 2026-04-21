using UnityEngine;
using TMPro; // Обязательная библиотека для работы с новым текстом!

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;

    [Header("UI Настройки")]
    [SerializeField] private GameObject interactUI;

    private TextMeshProUGUI promptText; // Переменная для самого текста

    private void Start()
    {
        // При старте игры находим текстовый компонент внутри твоего выключенного объекта
        if (interactUI != null)
        {
            promptText = interactUI.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        CheckInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void CheckInteractable()
    {
        if (interactUI == null || promptText == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);
        DoorController foundDoor = null;

        // Ищем дверь вокруг нас
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Door"))
            {
                foundDoor = col.GetComponentInParent<DoorController>();
                break;
            }
        }

        // Если нашли дверь
        if (foundDoor != null)
        {
            interactUI.SetActive(true); // Показываем текст

            // МАГИЯ ЗДЕСЬ: Меняем текст в зависимости от состояния
            if (foundDoor.isOpen)
            {
                promptText.text = "[E] Zavřít"; // Если открыта - пишем Закрыть
            }
            else
            {
                promptText.text = "[E] Otevřít"; // Если закрыта - пишем Открыть
            }
        }
        else
        {
            // Если ушли от двери - прячем текст
            interactUI.SetActive(false);
        }
    }

    private void TryInteract()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Door"))
            {
                DoorController door = col.GetComponentInParent<DoorController>();
                if (door != null)
                {
                    door.ToggleDoor();
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}