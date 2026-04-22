using System.Collections;
using UnityEngine;
using Pathfinding; // ПОДКЛЮЧАЕМ ПЛАГИН A*

public class DoorController : MonoBehaviour
{
    [Header("Nastavení (Konstruktor)")]
    public Transform openStateMarker;
    public float openSpeed = 3f;

    [Header("Настройки для Монстра")]
    public int health = 2; // Количество ударов до поломки

    public bool isOpen = false;
    private bool isAnimating = false;

    private Vector3 closedPos;
    private Quaternion closedRot;
    private Vector3 openPos;
    private Quaternion openRot;

    private bool isSwingDoor = false;
    private Vector2 phantomHinge;
    private float totalSwingAngle;

    private Collider2D col; // Ссылка на коллайдер двери

    private void Start()
    {
        col = GetComponent<Collider2D>();

        closedPos = transform.position;
        closedRot = transform.rotation;

        if (openStateMarker != null)
        {
            openPos = openStateMarker.position;
            openRot = openStateMarker.rotation;

            SpriteRenderer redSR = GetComponent<SpriteRenderer>();
            SpriteRenderer greenSR = openStateMarker.GetComponent<SpriteRenderer>();

            if (redSR != null && greenSR != null)
            {
                Bounds b1 = redSR.bounds;
                Bounds b2 = greenSR.bounds;

                if (b1.Intersects(b2))
                {
                    isSwingDoor = true;

                    float xMin = Mathf.Max(b1.min.x, b2.min.x);
                    float xMax = Mathf.Min(b1.max.x, b2.max.x);
                    float yMin = Mathf.Max(b1.min.y, b2.min.y);
                    float yMax = Mathf.Min(b1.max.y, b2.max.y);

                    phantomHinge = new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);

                    Vector2 toStart = (Vector2)closedPos - phantomHinge;
                    Vector2 toEnd = (Vector2)openPos - phantomHinge;

                    totalSwingAngle = Vector2.SignedAngle(toStart, toEnd);
                }
            }

            if (greenSR != null) greenSR.enabled = false;
        }
    }

    // Твой оригинальный метод, который вызывает игрок
    public void ToggleDoor()
    {
        if (isAnimating || openStateMarker == null) return;
        StartCoroutine(AnimateDoor());
    }

    // НОВЫЙ МЕТОД: Монстр бьет дверь
    public void TakeDamage()
    {
        health--;
        Debug.Log("Монстр бьет дверь! Осталось прочности: " + health);

        if (health <= 0)
        {
            BreakDoor();
        }
    }

    // НОВЫЙ МЕТОД: Дверь ломается
    private void BreakDoor()
    {
        // Запоминаем зону, где стояла дверь
        Bounds bounds = col.bounds;

        Destroy(gameObject); // Уничтожаем объект

        // Говорим сетке: "Перерисуй синие квадраты, тут больше нет преграды!"
        AstarPath.active.UpdateGraphs(bounds);
    }

    private IEnumerator AnimateDoor()
    {
        isAnimating = true;
        float progress = 0;

        // Запоминаем изначальные границы до начала движения
        Bounds startBounds = col.bounds;

        Vector3 targetPos = isOpen ? closedPos : openPos;
        Quaternion targetRot = isOpen ? closedRot : openRot;

        float orbitAngle = isOpen ? -totalSwingAngle : totalSwingAngle;
        Vector3 orbitStartPos = isOpen ? openPos : closedPos;
        Quaternion orbitStartRot = isOpen ? openRot : closedRot;

        while (progress < 1f)
        {
            progress += Time.deltaTime * openSpeed;
            float eased = progress * progress * (3f - 2f * progress);

            if (isSwingDoor)
            {
                float currentAngle = Mathf.Lerp(0, orbitAngle, eased);
                Quaternion rotationOffset = Quaternion.Euler(0, 0, currentAngle);

                Vector3 relativeStart = orbitStartPos - (Vector3)phantomHinge;
                transform.position = (Vector3)phantomHinge + rotationOffset * relativeStart;
                transform.rotation = rotationOffset * orbitStartRot;
            }
            else
            {
                transform.position = Vector3.Lerp(isOpen ? openPos : closedPos, targetPos, eased);
                transform.rotation = Quaternion.Lerp(isOpen ? openRot : closedRot, targetRot, eased);
            }

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        isOpen = !isOpen;
        isAnimating = false;

        // --- ЖЕЛЕЗОБЕТОННОЕ ОБНОВЛЕНИЕ СЕТКИ ---
        Bounds updateBounds = startBounds;
        updateBounds.Encapsulate(col.bounds); // Берем старое и новое место
        updateBounds.Expand(2f); // Увеличиваем зону сканирования с запасом

        // Ждем ровно один кадр, чтобы физика Unity обновила позицию коллайдера в пространстве
        yield return new WaitForEndOfFrame();

        // Просим плагин пересчитать эту зону
        AstarPath.active.UpdateGraphs(updateBounds);

    }

    // Метод СПЕЦИАЛЬНО для монстра: только открывает, никогда не закрывает
    public void MonsterOpenDoor()
    {
        if (!isOpen && !isAnimating)
        {
            ToggleDoor();
        }
    }
}