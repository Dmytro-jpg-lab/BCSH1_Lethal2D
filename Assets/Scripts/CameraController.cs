using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Цель, за которой следим (наш игрок)
    [SerializeField] private Transform target;

    // Скорость сглаживания (чем меньше, тем плавнее)
    [SerializeField] private float smoothSpeed = 5f;

    // В 2D камера ВСЕГДА должна висеть в координате Z = -10, иначе она войдет в текстуры
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    // LateUpdate вызывается после того, как игрок уже сделал шаг в FixedUpdate
    private void LateUpdate()
    {
        if (target == null) return;

        // Вычисляем идеальную точку, где должна быть камера
        Vector3 desiredPosition = target.position + offset;

        // Lerp плавно двигает камеру из текущей точки в идеальную
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
