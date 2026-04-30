using UnityEngine;

// Класс для хранения данных о предмете в кармане
[System.Serializable]
public class InventoryItem
{
    public string name;
    public int value;
    public float weight;
    public bool isEmpty = true; // Пустой ли слот?
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Настройки")]
    public int maxSlots = 4;
    public GameObject scrapPrefab;
    public float dropOffset = 1.2f;


    // Массив из 4 слотов
    public InventoryItem[] slots;

    // Текущий выбранный слот (от 0 до 3)
    public int selectedSlotIndex = 0;

    // Общие статы для UI и физики
    public int totalScrapValue = 0;
    public float totalWeight = 0f;
    public int bankScore = 0;

    private void Awake()
    {
        // Создаем 4 пустых слота при старте
        slots = new InventoryItem[maxSlots];
        for (int i = 0; i < maxSlots; i++)
        {
            slots[i] = new InventoryItem();
        }
    }

    private void Update()
    {
        HandleScrollInput();

        if (Input.GetKeyDown(KeyCode.G))
        {
            DropSelectedScrap();
        }
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // Крутим вверх
        {
            selectedSlotIndex--;
            if (selectedSlotIndex < 0) selectedSlotIndex = maxSlots - 1; // Возврат к последнему
        }
        else if (scroll < 0f) // Крутим вниз
        {
            selectedSlotIndex++;
            if (selectedSlotIndex >= maxSlots) selectedSlotIndex = 0; // Возврат к первому
        }
    }

    // Вызывается из PlayerInteract, когда жмем [E]
    public bool AddScrap(string itemName, int value, float weight)
    {
        // Ищем ПЕРВЫЙ свободный слот (начиная с выбранного или с нуля)
        for (int i = 0; i < maxSlots; i++)
        {
            if (slots[i].isEmpty)
            {
                slots[i].name = itemName;
                slots[i].value = value;
                slots[i].weight = weight;
                slots[i].isEmpty = false;

                CalculateTotals(); // Пересчитываем общую сумму и вес
                return true; // Предмет успешно взят
            }
        }
        Debug.Log("Inventář je plný!");
        return false;
    }

    // Допустим, dropPosition - это координаты твоей мышки
    public LayerMask obstacleLayer; // Назначь сюда слой стен в Инспекторе

    public void DropItem(Vector2 dropPosition)
    {
        // Рисуем невидимый кружок радиусом 0.2 в точке броска.
        // Если он касается стены - отменяем бросок!
        Collider2D wallCheck = Physics2D.OverlapCircle(dropPosition, 0.2f, obstacleLayer);

        if (wallCheck != null)
        {
            Debug.Log("Don't throw it against the wall!");
            return; // Прерываем функцию, предмет остается в инвентаре
        }

        // Если стена не найдена — код идет дальше и предмет падает
        // Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        // BlindHearerAI.MakeNoise(...);
    }

    private void DropSelectedScrap()
    {
        // Проверяем, есть ли что-то в ВЫБРАННОМ слоте
        if (!slots[selectedSlotIndex].isEmpty)
        {
            // 1. Сначала вычисляем, КУДА именно должен упасть предмет
            Vector3 dropPos = transform.position + transform.up * dropOffset;

            // 2. ПРОВЕРКА НА СТЕНУ
            // Кидаем невидимый кружок радиусом 0.2f в точку будущего падения
            Collider2D wallCheck = Physics2D.OverlapCircle(dropPos, 0.2f, obstacleLayer);

            if (wallCheck != null)
            {
                // Если там стена - отменяем выброс! 
                Debug.Log("There's a wall in front of you; the object hasn't been thrown away!");
                return; // Функция прерывается, предмет остается в инвентаре
            }

            // 3. Если стены нет - продолжаем выброс как обычно
            string dropName = slots[selectedSlotIndex].name;
            int dropValue = slots[selectedSlotIndex].value;
            float dropWeight = slots[selectedSlotIndex].weight;

            // Очищаем слот (я убрал дублирование, которое у тебя тут было)
            slots[selectedSlotIndex].isEmpty = true;
            CalculateTotals();

            // Создаем звук падения
            BlindHearerAI.MakeNoise(transform.position, 15f);

            // Создаем сам предмет
            if (scrapPrefab != null)
            {
                GameObject droppedItem = Instantiate(scrapPrefab, dropPos, Quaternion.identity);

                Scrap scrapScript = droppedItem.GetComponent<Scrap>();
                if (scrapScript != null)
                {
                    scrapScript.itemName = dropName;
                    scrapScript.scrapValue = dropValue;
                    scrapScript.scrapWeight = dropWeight;

                    if (Mathf.Abs(transform.position.x) <= 5f && Mathf.Abs(transform.position.y) <= 5f)
                    {
                        scrapScript.isSold = true; // Делаем предмет декорацией
                        bankScore += dropValue;    // Начисляем деньги в банк
                        Debug.Log($"ПРОДАНО! +${dropValue}. В банке: ${bankScore}");
                    }
                }
            }
        }
    }

    private void CalculateTotals()
    {
        totalScrapValue = 0;
        totalWeight = 0f;
        foreach (var slot in slots)
        {
            if (!slot.isEmpty)
            {
                totalScrapValue += slot.value;
                totalWeight += slot.weight;
            }
        }
    }
}