using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Nastavení inventáře (Настройки инвентаря)
    [SerializeField] private int maxSlots = 4;
    private int currentScrapCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kontrola, zda má objekt tag "Scrap"
        if (other.CompareTag("Scrap"))
        {
            // Kontrola kapacity inventáře
            if (currentScrapCount < maxSlots)
            {
                currentScrapCount++; // Přidání předmětu

                // Výpis do konzole v češtině
                Debug.Log("Sebrán šrot: " + currentScrapCount + " z " + maxSlots);

                // Zničení předmětu na scéně
                Destroy(other.gameObject);
            }
            else
            {
                // Zpráva o plném inventáři
                Debug.Log("Inventář je plný! Musíš odnést kořist na loď.");
            }
        }
    }
}