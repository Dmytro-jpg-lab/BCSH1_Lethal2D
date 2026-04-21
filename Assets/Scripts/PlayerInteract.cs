using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f; // Радиус взаимодействия

    private void Update()
    {
        // Проверяем, нажал ли игрок клавишу E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // Создаем невидимый круг вокруг игрока и собираем всё, что в него попало
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach (Collider2D col in colliders)
        {
            // Если в круге оказался объект с Тегом "Door"
            if (col.CompareTag("Door"))
            {
                Debug.Log("Dveře nalezeny, hledám ovladač...");

                // --- ИСПРАВЛЕНИЕ: Ищем скрипт на родительском объекте (DoorPivot) ---
                // Раньше было col.GetComponent<DoorController>();
                DoorController door = col.GetComponentInParent<DoorController>();
                // -------------------------------------------------------------------

                if (door != null)
                {
                    Debug.Log("Ovladač nalezen, přepínám dveře!");
                    door.ToggleDoor();

                    // Выходим из цикла, чтобы не открыть случайно две двери сразу
                    break;
                }
                else
                {
                    Debug.LogError("Chyba: Objekt má tag 'Door', ale na DoorPivot chybí DoorController!");
                }
            }
        }
    }

    // Это визуальная фишка: рисует желтый круг взаимодействия в редакторе Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}