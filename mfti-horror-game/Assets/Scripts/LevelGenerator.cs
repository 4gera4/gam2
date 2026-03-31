using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject[] corridorPrefabs;
    public GameObject[] classroomPrefabs;
    public GameObject[] lectureHallPrefabs;
    public GameObject[] specialRoomPrefabs;
    
    [Header("Level Settings")]
    public int levelWidth = 10;
    public int levelHeight = 10;
    public float roomSize = 10f;
    public int roomCount = 15;
    
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public List<Transform> teacherSpawnPoints = new List<Transform>();
    public List<Transform> integralSpawnPoints = new List<Transform>();
    
    [Header("Decorations")]
    public GameObject[] deskPrefabs;
    public GameObject[] boardPrefabs;
    public GameObject[] decorationPrefabs;
    public int decorationsPerRoom = 3;
    
    private bool[,] grid;
    private List<Vector2Int> roomPositions = new List<Vector2Int>();
    private List<GameObject> spawnedRooms = new List<GameObject>();
    
    [System.Serializable]
    public class Room
    {
        public Vector2Int position;
        public RoomType type;
        public GameObject instance;
        public bool hasNorthDoor;
        public bool hasSouthDoor;
        public bool hasEastDoor;
        public bool hasWestDoor;
    }
    
    public enum RoomType
    {
        Corridor,
        Classroom,
        LectureHall,
        Library,
        Cafeteria,
        DeanOffice
    }
    
    private List<Room> rooms = new List<Room>();
    
    void Start()
    {
        GenerateLevel();
    }
    
    public void GenerateLevel()
    {
        ClearLevel();
        
        grid = new bool[levelWidth, levelHeight];
        
        // Начинаем с центра
        Vector2Int startPos = new Vector2Int(levelWidth / 2, levelHeight / 2);
        CreateRoom(startPos, RoomType.Classroom);
        
        // Генерируем комнаты
        for (int i = 0; i < roomCount - 1; i++)
        {
            Vector2Int newPos = GetValidAdjacentPosition();
            if (newPos.x >= 0)
            {
                RoomType type = GetRandomRoomType();
                CreateRoom(newPos, type);
            }
        }
        
        // Соединяем комнаты дверями
        ConnectRooms();
        
        // Добавляем декорации
        DecorateRooms();
        
        // Создаем точки спавна
        CreateSpawnPoints();
        
        Debug.Log($"Сгенерирован уровень с {rooms.Count} комнатами");
    }
    
    void CreateRoom(Vector2Int pos, RoomType type)
    {
        grid[pos.x, pos.y] = true;
        
        Vector3 worldPos = new Vector3(pos.x * roomSize, 0, pos.y * roomSize);
        
        GameObject prefab = GetPrefabForType(type);
        GameObject roomObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        
        Room room = new Room
        {
            position = pos,
            type = type,
            instance = roomObj
        };
        
        rooms.Add(room);
        roomPositions.Add(pos);
        spawnedRooms.Add(roomObj);
    }
    
    GameObject GetPrefabForType(RoomType type)
    {
        switch (type)
        {
            case RoomType.Corridor:
                return corridorPrefabs[Random.Range(0, corridorPrefabs.Length)];
            case RoomType.Classroom:
                return classroomPrefabs[Random.Range(0, classroomPrefabs.Length)];
            case RoomType.LectureHall:
                return lectureHallPrefabs[Random.Range(0, lectureHallPrefabs.Length)];
            default:
                return specialRoomPrefabs[Random.Range(0, specialRoomPrefabs.Length)];
        }
    }
    
    RoomType GetRandomRoomType()
    {
        float rand = Random.value;
        if (rand < 0.4f) return RoomType.Corridor;
        if (rand < 0.7f) return RoomType.Classroom;
        if (rand < 0.9f) return RoomType.LectureHall;
        return RoomType.Library;
    }
    
    Vector2Int GetValidAdjacentPosition()
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        
        foreach (Vector2Int roomPos in roomPositions)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = roomPos + dir;
                
                if (IsValidPosition(newPos) && !grid[newPos.x, newPos.y])
                {
                    possiblePositions.Add(newPos);
                }
            }
        }
        
        if (possiblePositions.Count > 0)
        {
            return possiblePositions[Random.Range(0, possiblePositions.Count)];
        }
        
        return new Vector2Int(-1, -1);
    }
    
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < levelWidth && pos.y >= 0 && pos.y < levelHeight;
    }
    
    void ConnectRooms()
    {
        foreach (Room room in rooms)
        {
            Vector2Int pos = room.position;
            
            // Проверяем соседей
            room.hasNorthDoor = HasRoomAt(pos + Vector2Int.up);
            room.hasSouthDoor = HasRoomAt(pos + Vector2Int.down);
            room.hasEastDoor = HasRoomAt(pos + Vector2Int.right);
            room.hasWestDoor = HasRoomAt(pos + Vector2Int.left);
            
            // Создаем двери
            CreateDoors(room);
        }
    }
    
    bool HasRoomAt(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return false;
        return grid[pos.x, pos.y];
    }
    
    void CreateDoors(Room room)
    {
        // Здесь можно создать двери в нужных направлениях
        // Например, активировать/деактивировать дверные объекты в префабе
    }
    
    void DecorateRooms()
    {
        foreach (Room room in rooms)
        {
            if (room.type == RoomType.Classroom || room.type == RoomType.LectureHall)
            {
                // Добавляем парты
                int deskCount = room.type == RoomType.LectureHall ? 10 : 5;
                for (int i = 0; i < deskCount; i++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-roomSize * 0.4f, roomSize * 0.4f),
                        0,
                        Random.Range(-roomSize * 0.4f, roomSize * 0.4f)
                    );
                    
                    Vector3 deskPos = room.instance.transform.position + offset;
                    GameObject desk = Instantiate(deskPrefabs[Random.Range(0, deskPrefabs.Length)], 
                        deskPos, Quaternion.Euler(0, Random.Range(0, 4) * 90, 0), room.instance.transform);
                }
                
                // Добавляем доску
                if (boardPrefabs.Length > 0)
                {
                    Vector3 boardPos = room.instance.transform.position + new Vector3(0, 2, -roomSize * 0.4f);
                    Instantiate(boardPrefabs[Random.Range(0, boardPrefabs.Length)], 
                        boardPos, Quaternion.identity, room.instance.transform);
                }
            }
            
            // Случайные декорации
            for (int i = 0; i < decorationsPerRoom; i++)
            {
                if (decorationPrefabs.Length > 0)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-roomSize * 0.45f, roomSize * 0.45f),
                        0,
                        Random.Range(-roomSize * 0.45f, roomSize * 0.45f)
                    );
                    
                    Vector3 decPos = room.instance.transform.position + offset;
                    Instantiate(decorationPrefabs[Random.Range(0, decorationPrefabs.Length)], 
                        decPos, Quaternion.Euler(0, Random.Range(0, 360), 0), room.instance.transform);
                }
            }
        }
    }
    
    void CreateSpawnPoints()
    {
        // Создаем точку спавна игрока в первой комнате
        if (rooms.Count > 0)
        {
            GameObject playerSpawn = new GameObject("PlayerSpawn");
            playerSpawn.transform.position = rooms[0].instance.transform.position + Vector3.up;
            playerSpawnPoint = playerSpawn.transform;
        }
        
        // Создаем точки спавна преподавателей
        for (int i = 1; i < rooms.Count && i < 8; i++)
        {
            GameObject teacherSpawn = new GameObject($"TeacherSpawn_{i}");
            teacherSpawn.transform.position = rooms[i].instance.transform.position + Vector3.up;
            teacherSpawnPoints.Add(teacherSpawn.transform);
        }
        
        // Создаем точки спавна интегралов
        for (int i = 2; i < rooms.Count && i < 7; i++)
        {
            GameObject integralSpawn = new GameObject($"IntegralSpawn_{i}");
            integralSpawn.transform.position = rooms[i].instance.transform.position + Vector3.up * 2;
            integralSpawnPoints.Add(integralSpawn.transform);
        }
    }
    
    void ClearLevel()
    {
        foreach (GameObject room in spawnedRooms)
        {
            if (room != null)
            {
                Destroy(room);
            }
        }
        
        spawnedRooms.Clear();
        rooms.Clear();
        roomPositions.Clear();
        teacherSpawnPoints.Clear();
        integralSpawnPoints.Clear();
    }
    
    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                Vector3 pos = new Vector3(x * roomSize, 0, y * roomSize);
                Gizmos.DrawWireCube(pos, new Vector3(roomSize, 5, roomSize));
            }
        }
    }
}
