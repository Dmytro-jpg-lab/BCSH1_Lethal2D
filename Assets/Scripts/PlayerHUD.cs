using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player References")]
    public PlayerController playerController;
    public PlayerInventory playerInventory;

    [Header("UI Lines")]
    public Image hpLine;
    public Image staminaLine;

    [Header("UI Text")]
    public TextMeshProUGUI weightText;

    [Header("Style Settings")]
    public Color normalRed = new Color(0.8f, 0.2f, 0.2f, 1f);
    public float blinkSpeed = 10f;

    private void Update()
    {
        if (playerController == null || playerInventory == null) return;

        UpdateHP();
        UpdateStamina();
        UpdateTerminalText();
    }

    void UpdateHP()
    {
        float hpRatio = playerController.currentHealth / playerController.maxHealth;
        hpLine.fillAmount = hpRatio;

        // Мигание, если HP < 25%
        if (hpRatio < 0.25f)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            hpLine.color = new Color(normalRed.r, normalRed.g, normalRed.b, alpha);
        }
        else
        {
            hpLine.color = normalRed;
        }
    }

    void UpdateStamina()
    {
        staminaLine.fillAmount = playerController.currentStamina / playerController.maxStamina;
    }

    void UpdateTerminalText()
    {
        float currentWeight = playerInventory.totalWeight;
        float limit = playerController.weightPenaltyLimit;

        // БЕРЕМ ПРАВИЛЬНУЮ ПЕРЕМЕННУЮ ИЗ ТВОЕГО ИНВЕНТАРЯ:
        int totalValue = playerInventory.totalScrapValue;

        string bar = GenerateBar(currentWeight, limit, 10);
        string status = currentWeight > limit ? "<color=red>[OVERLOAD]</color>" : "NORMAL";

        // Выводим вес, статус, цену и шкалу
        weightText.text = $"LOAD : {currentWeight:F1} / {limit:F0} KG  {status}\nVALUE: ${totalValue}\n{bar}";
    }

    string GenerateBar(float current, float max, int length)
    {
        string res = "[";
        float fill = Mathf.Clamp01(current / max);
        int filledCount = Mathf.RoundToInt(fill * length);

        for (int i = 0; i < length; i++)
        {
            if (i < filledCount) res += "█";
            else res += "░";
        }
        return res + "]";
    }
}