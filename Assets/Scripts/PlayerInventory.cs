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

    private void DropSelectedScrap()
    {
        // Проверяем, есть ли что-то в ВЫБРАННОМ слоте
        if (!slots[selectedSlotIndex].isEmpty)
        {
            // Берем данные
            string dropName = slots[selectedSlotIndex].name;
            int dropValue = slots[selectedSlotIndex].value;
            float dropWeight = slots[selectedSlotIndex].weight;

            // Очищаем слот
            slots[selectedSlotIndex].isEmpty = true;
            CalculateTotals();
            // Очищаем слот
            slots[selectedSlotIndex].isEmpty = true;
            CalculateTotals();

            // --- НОВАЯ СТРОЧКА: Создаем звук падения радиусом 8 метров ---
            BlindHearerAI.MakeNoise(transform.position, 8f);

            // Создаем предмет перед игроком
            if (scrapPrefab != null)
            {
                Vector3 dropPos = transform.position + transform.up * dropOffset;
                GameObject droppedItem = Instantiate(scrapPrefab, dropPos, Quaternion.identity);

                Scrap scrapScript = droppedItem.GetComponent<Scrap>();
                if (scrapScript != null)
                {
                    // Передаем ему его старые данные!
                    scrapScript.itemName = dropName;
                    scrapScript.scrapValue = dropValue;
                    scrapScript.scrapWeight = dropWeight;
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