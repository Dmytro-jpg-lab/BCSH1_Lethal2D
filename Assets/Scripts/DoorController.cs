using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Nastavení (Konstruktor)")]
    public Transform openStateMarker;
    public float openSpeed = 3f;

    private bool isOpen = false;
    private bool isAnimating = false;

    private Vector3 closedPos;
    private Quaternion closedRot;
    private Vector3 openPos;
    private Quaternion openRot;

    private bool isSwingDoor = false;
    private Vector2 phantomHinge;
    private float totalSwingAngle;

    private void Start()
    {
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

                // Если красная и зеленая физически накладываются (как на твоем рисунке)
                if (b1.Intersects(b2))
                {
                    isSwingDoor = true;

                    // Находим центр квадрата пересечения - это наша точка P (петля)
                    float xMin = Mathf.Max(b1.min.x, b2.min.x);
                    float xMax = Mathf.Min(b1.max.x, b2.max.x);
                    float yMin = Mathf.Max(b1.min.y, b2.min.y);
                    float yMax = Mathf.Min(b1.max.y, b2.max.y);

                    phantomHinge = new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);

                    // Вычисляем угол прерывистой траектории от красной к зеленой
                    Vector2 toStart = (Vector2)closedPos - phantomHinge;
                    Vector2 toEnd = (Vector2)openPos - phantomHinge;

                    // SignedAngle дает точный угол (например, -90 или +90), учитывая, где лежит h1
                    totalSwingAngle = Vector2.SignedAngle(toStart, toEnd);
                }
            }

            // Прячем зеленую метку
            if (greenSR != null) greenSR.enabled = false;
        }
    }

    public void ToggleDoor()
    {
        if (isAnimating || openStateMarker == null) return;
        StartCoroutine(AnimateDoor());
    }

    private IEnumerator AnimateDoor()
    {
        isAnimating = true;
        float progress = 0;

        // Определяем, куда едем
        Vector3 targetPos = isOpen ? closedPos : openPos;
        Quaternion targetRot = isOpen ? closedRot : openRot;

        // Параметры для дуги (в зависимости от того, открываем или закрываем)
        float orbitAngle = isOpen ? -totalSwingAngle : totalSwingAngle;
        Vector3 orbitStartPos = isOpen ? openPos : closedPos;
        Quaternion orbitStartRot = isOpen ? openRot : closedRot;

        while (progress < 1f)
        {
            progress += Time.deltaTime * openSpeed;
            float eased = progress * progress * (3f - 2f * progress);

            if (isSwingDoor)
            {
                // Идем точно по прерывистой линии с твоего чертежа
                float currentAngle = Mathf.Lerp(0, orbitAngle, eased);
                Quaternion rotationOffset = Quaternion.Euler(0, 0, currentAngle);

                Vector3 relativeStart = orbitStartPos - (Vector3)phantomHinge;
                transform.position = (Vector3)phantomHinge + rotationOffset * relativeStart;

                // ИСПРАВЛЕНИЕ: Жестко привязываем поворот спрайта к повороту дуги
                transform.rotation = rotationOffset * orbitStartRot;
            }
            else
            {
                // Если двери не пересекаются — это просто Слайд-бункер
                transform.position = Vector3.Lerp(isOpen ? openPos : closedPos, targetPos, eased);
                transform.rotation = Quaternion.Lerp(isOpen ? openRot : closedRot, targetRot, eased);
            }

            yield return null;
        }

        // Жесткая фиксация в конце, чтобы не было микро-зазоров
        transform.position = targetPos;
        transform.rotation = targetRot;

        isOpen = !isOpen;
        isAnimating = false;
    }
}