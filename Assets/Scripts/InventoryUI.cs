using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Связи")]
    public PlayerInventory playerInventory;

    [Header("Нижние Слоты (4 шт)")]
    public RectTransform[] slotRects; // Для изменения размера
    public Image[] slotImages;        // Для изменения прозрачности/цвета

    [Header("Правый Верхний Угол (Активный предмет)")]
    public TextMeshProUGUI activeItemText;

    [Header("Визуал")]
    public Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.3f); // Почти прозрачный черный
    public Color filledColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Более плотный
    public Color highlightColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Цвет рамки выбранного

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerInventory == null) return;

        // 1. ОБНОВЛЯЕМ НИЖНИЕ СЛОТЫ
        for (int i = 0; i < playerInventory.maxSlots; i++)
        {
            bool isSelected = (i == playerInventory.selectedSlotIndex);
            bool isEmpty = playerInventory.slots[i].isEmpty;

            // Изменяем масштаб: выбранный = 1.2, обычный = 1.0
            float scale = isSelected ? 1.2f : 1.0f;
            slotRects[i].localScale = new Vector3(scale, scale, 1f);

            // Настраиваем цвет слота
            if (isEmpty)
            {
                slotImages[i].color = emptyColor;
            }
            else
            {
                slotImages[i].color = isSelected ? highlightColor : filledColor;
            }

            // В Lethal Company в самих слотах нет текста, только иконка.
            // Так как у нас пока нет иконок, можешь оставить слот пустым или добавить текст цены.
        }

        // 2. ОБНОВЛЯЕМ ТЕКСТ В ПРАВОМ ВЕРХНЕМ УГЛУ
        InventoryItem activeItem = playerInventory.slots[playerInventory.selectedSlotIndex];

        if (activeItem.isEmpty)
        {
            activeItemText.text = ""; // Скрываем текст, если в руках пусто
        }
        else
        {
            // Показываем Имя, Цену и Вес в столбик
            activeItemText.text = $"{activeItem.name}\n${activeItem.value}\n{activeItem.weight} kg";
        }
    }
}