using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Взаимодействие (Двери и Лут)")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private GameObject interactUI;

    [Header("Радар лута (Сканнер)")]
    [SerializeField] private float scanRadius = 10f;
    [SerializeField][Range(10f, 360f)] private float scanAngle = 90f;
    [SerializeField] private GameObject scanWavePrefab;
    [SerializeField] private float waveDuration = 0.6f;

    [Header("Физика")]
    [SerializeField] private LayerMask obstacleLayer;

    private TextMeshProUGUI promptText;

    private void Start()
    {
        if (interactUI != null) promptText = interactUI.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        CheckInteractable();
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        if (Input.GetMouseButtonDown(1)) ScanForLoot();
    }

    private void ScanForLoot()
    {
        if (scanWavePrefab != null) StartCoroutine(AnimateScanWave());

        // Ищем вообще ВСЕ коллайдеры в радиусе (без фильтра по слоям)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, scanRadius);
        Debug.Log($"--- НОВЫЙ СКАН --- Найдено объектов в радиусе: {colliders.Length}");
        foreach (Collider2D c in colliders)
        {
            Debug.Log($"Радар нащупал: {c.gameObject.name} (Слой: {LayerMask.LayerToName(c.gameObject.layer)})");
        }
        // --------------------------------------------
        foreach (Collider2D col in colliders)
        {
            // 1. Ищем скрипт лута (на всякий случай проверяем и родителей)
            Scrap scrapItem = col.GetComponent<Scrap>();
            if (scrapItem == null) scrapItem = col.GetComponentInParent<Scrap>();

            // 2. Бронебойный поиск ауры монстра (проверяем всё дерево объекта)
            EnemyAura enemyAura = col.GetComponent<EnemyAura>();
            if (enemyAura == null) enemyAura = col.GetComponentInParent<EnemyAura>();
            if (enemyAura == null) enemyAura = col.GetComponentInChildren<EnemyAura>();

            // Если это обычная стена или пол - пропускаем
            if (scrapItem == null && enemyAura == null) continue;

            if (enemyAura != null) Debug.Log($"[РАДАР] Зацепил коллайдер монстра: {col.gameObject.name}. Проверяю угол...");

            // ЦЕЛИМСЯ В ЦЕНТР КОЛЛАЙДЕРА, а не в точку Pivot (чтобы луч не ушел в пол)
            Vector2 directionToTarget = (col.bounds.center - transform.position).normalized;
            Vector2 playerForward = transform.up;
            float angleToTarget = Vector2.Angle(playerForward, directionToTarget);

            if (angleToTarget < scanAngle / 2f)
            {
                if (enemyAura != null) Debug.Log("[РАДАР] Монстр в зоне видимости! Пускаю луч-проверку на стены...");

                float distanceToTarget = Vector2.Distance(transform.position, col.bounds.center);
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, directionToTarget, distanceToTarget, obstacleLayer);
                bool pathBlocked = false;

                foreach (RaycastHit2D hit in hits)
                {
                    // ПРОБИВАЕМ САМУ ЦЕЛЬ: Если луч задел любую часть монстра (или дочерний триггер) - игнорим
                    if (hit.collider.transform.root == col.transform.root) continue;

                    // Игнорируем самого игрока (чтобы луч не застрял в нас самих)
                    if (hit.collider.transform.root == transform.root) continue;

                    DoorController hitDoor = hit.collider.GetComponentInParent<DoorController>();
                    if (hitDoor != null && hitDoor.isOpen) continue;

                    pathBlocked = true;
                    if (enemyAura != null) Debug.Log($"[РАДАР] БЛОКИРОВКА! Луч врезался в: {hit.collider.gameObject.name}");
                    break;
                }

                if (!pathBlocked)
                {
                    if (scrapItem != null) scrapItem.RevealScrap();
                    if (enemyAura != null)
                    {
                        Debug.Log("[РАДАР] ПУТЬ ЧИСТ! Включаю красное свечение!");
                        enemyAura.RevealMonster();
                    }
                }
            }
        }
    }

    private IEnumerator AnimateScanWave()
    {
        GameObject wave = Instantiate(scanWavePrefab, transform.position, transform.rotation);
        SpriteRenderer sr = wave.GetComponent<SpriteRenderer>();
        float timer = 0f;
        float targetScale = scanRadius * 2f;

        while (timer < waveDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / waveDuration;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            wave.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(targetScale, targetScale, 1f), easedProgress);
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(0.8f, 0f, easedProgress);
                sr.color = c;
            }
            yield return null;
        }
        Destroy(wave);
    }

    // --- ОБНОВЛЕНО: Ищет и двери, и лут. Лут в приоритете ---
    private void CheckInteractable()
    {
        // 1. Защита от "тихого залипания"
        if (interactUI == null)
        {
            Debug.LogWarning("ВНИМАНИЕ: Слот interactUI пустой в инспекторе Игрока!");
            return;
        }

        // Надежно ищем текст даже внутри дочерних объектов
        if (promptText == null)
        {
            promptText = interactUI.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText == null) return;
        }

        // 2. Ищем объекты вокруг (без лучей, просто по радиусу)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);
        DoorController foundDoor = null;
        Scrap foundScrap = null;

        foreach (Collider2D col in colliders)
        {
            if (col.GetComponent<Scrap>() != null)
            {
                foundScrap = col.GetComponent<Scrap>();
            }
            else if (col.CompareTag("Door"))
            {
                foundDoor = col.GetComponentInParent<DoorController>();
            }
        }

        // 3. Показываем или прячем интерфейс
        if (foundScrap != null)
        {
            interactUI.SetActive(true);
            promptText.text = $"[E] Sebrat (${foundScrap.scrapValue})";
        }
        else if (foundDoor != null)
        {
            interactUI.SetActive(true);
            promptText.text = "[E] Otevřít / Zavřít";
        }
        else
        {
            // ЖЕЛЕЗНО ВЫКЛЮЧАЕМ, если рядом ничего нет
            interactUI.SetActive(false);
        }
    }



    
    // --- ОБНОВЛЕНО: Поднимает лут. Если лута нет - открывает дверь СКВОЗЬ СТЕНЫ ---
    private void TryInteract()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);

        // 1. Сначала пытаемся подобрать лут (тут оставляем проверку на стены, чтобы сквозь них не пылесосить)
        foreach (Collider2D col in colliders)
        {
            Scrap scrap = col.GetComponent<Scrap>();
            if (scrap != null)
            {
                Vector2 dir = (col.transform.position - transform.position).normalized;
                float dist = Vector2.Distance(transform.position, col.transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);

                // Если между нами и лутом нет препятствий
                if (hit.collider == null || hit.collider.gameObject == col.gameObject)
                {
                    scrap.Collect();
                    return; // Выходим из функции
                }
            }
        }

        // 2. Если лута нет, пытаемся открыть дверь (БЕЗ ПРОВЕРКИ ЛУЧОМ)
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Door"))
            {
                DoorController door = col.GetComponentInParent<DoorController>();
                if (door != null)
                {
                    // Дверь в радиусе interactRange? Просто открываем, плевать на стены!
                    door.ToggleDoor();
                    return;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
        Vector3 rightDirection = Quaternion.Euler(0, 0, -scanAngle / 2f) * transform.up;
        Vector3 leftDirection = Quaternion.Euler(0, 0, scanAngle / 2f) * transform.up;
        Gizmos.DrawRay(transform.position, rightDirection * scanRadius);
        Gizmos.DrawRay(transform.position, leftDirection * scanRadius);
    }
}

