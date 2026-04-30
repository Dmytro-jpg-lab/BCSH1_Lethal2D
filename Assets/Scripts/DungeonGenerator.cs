using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Магическая Палочка")]
    public Transform wallMerger; // Сюда мы кинем наш котел

    [Header("Префабы")]
    public Room startRoomPrefab;
    public List<GameObject> doorPrefabs;
    public GameObject sealPrefab;
    public List<Room> roomPrefabs;

    [Header("Спавн Монстров")]
    // Теперь это список НАСТРОЕК для каждого монстра
    public List<MonsterSpawnConfig> monsterSpawns;

    [Header("Система Лута (Lethal Style)")]
    public List<GameObject> lootPrefabs; // Список предметов (Scrap и т.д.)

    [Range(0, 100)] public float emptyRoomChance = 40f;   // Шанс пустой комнаты (40%)
    [Range(0, 100)] public float jackpotRoomChance = 10f; // Шанс джекпота (10%)

    public int minNormalLoot = 1;
    public int maxNormalLoot = 3;

    public int minJackpotLoot = 5;
    public int maxJackpotLoot = 8;

    [Header("Настройки")]
    public int maxRooms = 10;
    public LayerMask roomLayer; // Слой RoomTrigger

    private List<Room> spawnedRooms = new List<Room>();
    private List<RoomConnector> availableConnectors = new List<RoomConnector>();

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        // 1. Создаем стартовую комнату
        Room startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        startRoom.name = "StartRoom";
        spawnedRooms.Add(startRoom);
        availableConnectors.AddRange(startRoom.connectors);

        // 2. Цикл постройки
        StartCoroutine(BuildRoutine());
    }

    private IEnumerator BuildRoutine()
    {
        int iterations = 0;

        while (spawnedRooms.Count < maxRooms && iterations < 100)
        {
            iterations++;

            // Берем случайную свободную розетку на уровне
            int randomConnIndex = Random.Range(0, availableConnectors.Count);
            RoomConnector targetConnector = availableConnectors[randomConnIndex];

            // Пробуем прилепить новую комнату
            yield return TryPlaceRoom(targetConnector);
        }
        SealRemainingConnectors();
        ApplyMagicWand();
        DistributeLoot();
        SpawnMonsters();

        Debug.Log("Генерация завершена! Построено комнат: " + spawnedRooms.Count);
    }



    private void ApplyMagicWand()
    {
        if (wallMerger == null) return;

        // 1. Ставим котел на правильный слой, чтобы свет об него "спотыкался"
        wallMerger.gameObject.layer = LayerMask.NameToLayer("Obstacle");

        var compositeCollider = wallMerger.GetComponent<CompositeCollider2D>();
        if (compositeCollider != null) compositeCollider.usedByEffector = false;

        BoxCollider2D[] allColliders = FindObjectsOfType<BoxCollider2D>();
        int wallsMerged = 0;

        foreach (BoxCollider2D col in allColliders)
        {
            // Берем только стены
            if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                col.transform.SetParent(wallMerger);
                col.usedByComposite = true;
                wallsMerged++;
            }
        }

        // 2. Свариваем физику
        if (compositeCollider != null) compositeCollider.GenerateGeometry();

        // 3. ПРАВИЛЬНЫЙ РУБИЛЬНИК ТЕНЕЙ
        // Выключаем и включаем котел, чтобы он "съел" новые стены и построил монолитную тень
        var compShadow = wallMerger.GetComponent<UnityEngine.Rendering.Universal.CompositeShadowCaster2D>();
        if (compShadow != null)
        {
            compShadow.enabled = false;
            compShadow.enabled = true;
        }

        Debug.Log("Стены слиты: " + wallsMerged);
        StartCoroutine(ScanGridAfterMerge());
    }



    // Этот метод запустит финальное сканирование для монстра
    private IEnumerator ScanGridAfterMerge()
    {
        yield return new WaitForEndOfFrame(); // Ждем конца кадра
        AstarPath.active.Scan(); // Плагин A* сканирует новый монолитный контур
        Debug.Log("Сетка путей обновлена!");
    }
    private void SealRemainingConnectors()
    {
        foreach (var connector in availableConnectors)
        {
            if (!connector.isConnected)
            {
                // Ставим заглушку в позицию коннектора с его поворотом
                Instantiate(sealPrefab, connector.transform.position, connector.transform.rotation);
            }
        }

        availableConnectors.Clear();
    }
    private void SpawnMonsters()
    {
        // Проверяем, есть ли комнаты (игнорируем стартовую) и настроены ли монстры
        if (monsterSpawns == null || monsterSpawns.Count == 0 || spawnedRooms.Count <= 1) return;

        int totalSpawned = 0;

        // Перебираем каждую настройку монстра из нашего списка
        foreach (var config in monsterSpawns)
        {
            if (config.prefab == null) continue; // Если забыл вставить префаб - пропускаем

            // Бросаем кубик специально для ЭТОГО типа монстра
            int countToSpawn = Random.Range(config.minCount, config.maxCount + 1);

            for (int i = 0; i < countToSpawn; i++)
            {
                // Выбираем случайную комнату (игнорируя Старт под индексом 0)
                int randomRoomIndex = Random.Range(1, spawnedRooms.Count);
                Room room = spawnedRooms[randomRoomIndex];

                BoxCollider2D roomBounds = room.GetComponent<BoxCollider2D>();
                if (roomBounds == null) continue;

                // Считаем случайную точку в границах комнаты
                float rx = Random.Range(roomBounds.bounds.min.x + 1.5f, roomBounds.bounds.max.x - 1.5f);
                float ry = Random.Range(roomBounds.bounds.min.y + 1.5f, roomBounds.bounds.max.y - 1.5f);
                Vector3 spawnPos = new Vector3(rx, ry, 0);

                // Спавним конкретного монстра
                Instantiate(config.prefab, spawnPos, Quaternion.identity);
                totalSpawned++;
            }
        }

        Debug.Log($"Бестиарий заполнился! Всего заспавнено монстров: {totalSpawned}");
    }
    private void DistributeLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Count == 0) return;

        // Начинаем цикл с i = 1, чтобы ПРОПУСТИТЬ стартовую комнату (spawnedRooms[0]).
        // В первой комнате должно быть безопасно и пусто.
        for (int i = 1; i < spawnedRooms.Count; i++)
        {
            Room room = spawnedRooms[i];

            // Бросаем кубик от 0 до 100
            float roll = Random.Range(0f, 100f);
            int itemsToSpawn = 0;

            if (roll < emptyRoomChance)
            {
                // Попали в нижние 40% - комната пустая
                itemsToSpawn = 0;
            }
            else if (roll > 100f - jackpotRoomChance)
            {
                // Попали в верхние 10% - ДЖЕКПОТ!
                itemsToSpawn = Random.Range(minJackpotLoot, maxJackpotLoot + 1);
            }
            else
            {
                // Всё остальное (50%) - обычная комната
                itemsToSpawn = Random.Range(minNormalLoot, maxNormalLoot + 1);
            }

            // Берем коллайдер комнаты, чтобы понять её размеры
            BoxCollider2D roomBounds = room.GetComponent<BoxCollider2D>();
            if (roomBounds == null) continue;

            // Спавним нужное количество предметов
            for (int j = 0; j < itemsToSpawn; j++)
            {
                GameObject randomLoot = lootPrefabs[Random.Range(0, lootPrefabs.Count)];

                // Высчитываем случайную точку внутри комнаты.
                // Отступаем по 1.5 метра от краев, чтобы предметы не спавнились в стенах
                float rx = Random.Range(roomBounds.bounds.min.x + 1.5f, roomBounds.bounds.max.x - 1.5f);
                float ry = Random.Range(roomBounds.bounds.min.y + 1.5f, roomBounds.bounds.max.y - 1.5f);

                Vector3 spawnPos = new Vector3(rx, ry, 0);

                // Спавним предмет и делаем его дочерним к комнате (для порядка в Иерархии)
                Instantiate(randomLoot, spawnPos, Quaternion.identity, room.transform);
            }
        }

        Debug.Log("Предметы раскиданы по комнатам!");
    }



    private IEnumerator TryPlaceRoom(RoomConnector targetConnector)
    {
        // Выбираем случайный префаб
        Room prefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
        Room newRoom = Instantiate(prefab);

        // Берем случайную розетку У НОВОЙ комнаты
        RoomConnector newRoomConnector = newRoom.connectors[Random.Range(0, newRoom.connectors.Count)];

        // --- 1. МАГИЯ ВРАЩЕНИЯ ---
        // Узнаем, куда смотрят розетки в мировых координатах
        Vector2 targetDir = targetConnector.transform.TransformDirection(targetConnector.direction);
        Vector2 newDir = newRoomConnector.transform.TransformDirection(newRoomConnector.direction);

        // Вычисляем нужный угол поворота (чтобы розетки смотрели в противоположные стороны)
        float angle = Vector2.SignedAngle(newDir, -targetDir);
        newRoom.transform.Rotate(0, 0, angle);

        // --- 2. МАГИЯ СМЕЩЕНИЯ ---
        // Двигаем новую комнату так, чтобы её розетка точно совпала с целевой
        Vector3 offset = targetConnector.transform.position - newRoomConnector.transform.position;
        newRoom.transform.position += offset;

        // Даем физике Unity обновить координаты
        yield return new WaitForFixedUpdate();

        // --- 3. ПРОВЕРКА НАЛОЖЕНИЯ ---
        // Используем 0.95f, так как наш BoxCollider теперь обхватывает только пол!
        Bounds bounds = newRoom.roomBounds.bounds;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(bounds.center, bounds.size * 0.95f, 0, roomLayer);

        bool isOverlapping = false;
        foreach (var col in colliders)
        {
            // Если нашли чужую комнату (пол залез на чужой пол) - отменяем стройку
            if (col.gameObject != newRoom.gameObject && col.transform.IsChildOf(newRoom.transform) == false)
            {
                isOverlapping = true;
                break;
            }
        }

        if (isOverlapping)
        {
            // Место занято, удаляем и алгоритм попробует снова
            Destroy(newRoom.gameObject);
        }
        else
        {
            // Успех! Комната встала идеально.
            spawnedRooms.Add(newRoom);

            // Ставим дверь
            // Ставим умную дверь
            // Ставим случайную умную дверь
            if (doorPrefabs != null && doorPrefabs.Count > 0)
            {
                // Выбираем случайную дверь из списка
                GameObject randomDoorPrefab = doorPrefabs[Random.Range(0, doorPrefabs.Count)];

                float doorAngle = 0f;
                if (Mathf.Abs(targetDir.y) > 0.5f)
                {
                    doorAngle = 90f;
                }

                Quaternion properRotation = Quaternion.Euler(0, 0, doorAngle);
                Instantiate(randomDoorPrefab, targetConnector.transform.position, properRotation);
            }

            // Помечаем розетки как занятые
            targetConnector.isConnected = true;
            newRoomConnector.isConnected = true;

            availableConnectors.Remove(targetConnector);

            // Добавляем свободные розетки новой комнаты в общий котел
            foreach (var conn in newRoom.connectors)
            {
                if (!conn.isConnected) availableConnectors.Add(conn);
            }
        }
    }
}
// Добавляем этот класс ВНЕ класса DungeonGenerator (в самом низу файла)
[System.Serializable]
public class MonsterSpawnConfig
{
    public string note = "Имя монстра"; // Просто чтобы тебе было удобно подписывать их в Инспекторе
    public GameObject prefab;
    public int minCount = 1;
    public int maxCount = 2;
}