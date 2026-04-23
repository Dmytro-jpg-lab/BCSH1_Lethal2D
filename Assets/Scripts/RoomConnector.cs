using UnityEngine;

public class RoomConnector : MonoBehaviour
{
    // Куда смотрит этот выход? (Поможет алгоритму правильно вращать комнаты)
    public Vector2 direction;

    // Сюда мы потом запишем, какая комната пристыковалась к этой розетке
    [HideInInspector] public bool isConnected = false;

    private void OnDrawGizmos()
    {
        // Рисуем красную линию в редакторе, чтобы ты сам видел, куда смотрит розетка
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}