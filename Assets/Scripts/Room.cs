using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Розетки этой комнаты")]
    public List<RoomConnector> connectors = new List<RoomConnector>();

    [Header("Границы комнаты (для проверки наложений)")]
    public BoxCollider2D roomBounds;

    // В будущем тут можно добавить списки точек спавна мобов или лута
}