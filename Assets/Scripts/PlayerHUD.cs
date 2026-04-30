using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerInventory playerInventory;

    public Image hpLine;
    public Image staminaLine;
    public TextMeshProUGUI weightText;

    private void Update()
    {
        if (playerController == null || playerInventory == null) return;

        // Линии здоровья и стамины
        hpLine.fillAmount = playerController.currentHealth / playerController.maxHealth;
        staminaLine.fillAmount = playerController.currentStamina / playerController.maxStamina;

        // ЧИСТЫЙ ТЕКСТ: Вес и Деньги
        float currentWeight = playerInventory.totalWeight;
        int totalValue = playerInventory.totalScrapValue;
        int bank = playerInventory.bankScore;

        weightText.text = $"LOAD: {currentWeight:F1} KG     VALUE: $ {totalValue}\nBANK: $ {bank}";
    }
}