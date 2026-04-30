using System.Collections;
using UnityEngine;
using TMPro;

public class Scrap : MonoBehaviour
{
    [Header("Loot Settings")]
    public string itemName;
    public int scrapValue; // Цена
    public float scrapWeight; // Вес
    public bool isSold = false; 

    public GameObject priceUI;
    public SpriteRenderer outlineRenderer;

    [Header("Aura Settings")]
    [Range(0.1f, 1f)] public float darkenMultiplier = 0.6f;

    private SpriteRenderer myRenderer;
    private TextMeshProUGUI textComponent;

    // Список названий
    private string[] possibleNames = { "Monitor", "Big bolt", "Toolbox", "Box of Cables", "Clock", "Bottles", "Tactical vest", "Metal Sheet", "Mother's buddy", "Dildo" };

    private void Start()
    {
        // Генерируем данные, только если это НОВЫЙ предмет (если имя пустое)
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = possibleNames[Random.Range(0, possibleNames.Length)];
            scrapValue = Random.Range(15, 85);
            scrapWeight = Random.Range(1f, 15f);

            // Округляем вес до 1 знака после запятой (например, 14.5)
            scrapWeight = Mathf.Round(scrapWeight * 10f) / 10f;
        }

        myRenderer = GetComponent<SpriteRenderer>();

        if (priceUI != null)
        {
            textComponent = priceUI.GetComponentInChildren<TextMeshProUGUI>(true);
            if (textComponent != null)
            {
                // Показываем ИМЯ, а на следующей строчке (\n) ЦЕНУ
                textComponent.text = $"{itemName}\n${scrapValue}";
            }
            priceUI.SetActive(false);
        }

        if (outlineRenderer != null)
        {
            Color baseColor = myRenderer.color;
            outlineRenderer.color = new Color(baseColor.r * darkenMultiplier, baseColor.g * darkenMultiplier, baseColor.b * darkenMultiplier, 1f);
            outlineRenderer.gameObject.SetActive(false);
        }
    }

    public void RevealScrap()
    {
        StopAllCoroutines();
        StartCoroutine(ShowEffect());
    }

    private IEnumerator ShowEffect()
    {
        if (priceUI != null) priceUI.SetActive(true);
        if (outlineRenderer != null) outlineRenderer.gameObject.SetActive(true);

        float timer = 0;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            float alpha = 0.5f + Mathf.Sin(Time.time * 10f) * 0.2f;

            if (outlineRenderer != null)
            {
                Color c = outlineRenderer.color;
                c.a = alpha;
                outlineRenderer.color = c;
            }
            yield return null;
        }

        if (priceUI != null) priceUI.SetActive(false);
        if (outlineRenderer != null) outlineRenderer.gameObject.SetActive(false);
    }

    // --- ОБНОВЛЕННАЯ ФУНКЦИЯ ПОДБОРА ---
    public void Collect()
    {
        if (isSold) return;
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            // Пытаемся положить в инвентарь и передаем ему Имя, Цену и Вес
            bool wasPickedUp = inventory.AddScrap(itemName, scrapValue, scrapWeight);

            // Если место было и предмет забрали - уничтожаем его со сцены
            if (wasPickedUp)
            {
                Destroy(gameObject);
            }
        }
    }
}