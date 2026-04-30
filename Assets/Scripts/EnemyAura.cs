using System.Collections;
using UnityEngine;

public class EnemyAura : MonoBehaviour
{
    [Header("Aura Settings")]
    public SpriteRenderer auraRenderer;

    // ДОБАВИЛИ ЭТУ СТРОЧКУ: Теперь время можно менять в Инспекторе
    [SerializeField] private float blinkDuration = 2f;

    private void Start()
    {
        if (auraRenderer != null) auraRenderer.gameObject.SetActive(false);
    }

    public void RevealMonster()
    {
        if (auraRenderer == null) return;

        StopAllCoroutines();
        StartCoroutine(ShowAura());
    }

    private IEnumerator ShowAura()
    {
        auraRenderer.gameObject.SetActive(true);
        float timer = 0;

        // ТЕПЕРЬ ТУТ СТОИТ НАША ПЕРЕМЕННАЯ
        while (timer < blinkDuration)
        {
            timer += Time.deltaTime;
            float alpha = 0.5f + Mathf.Sin(Time.time * 10f) * 0.2f;
            Color c = auraRenderer.color;
            c.a = alpha;
            auraRenderer.color = c;
            yield return null;
        }

        auraRenderer.gameObject.SetActive(false);
    }
}