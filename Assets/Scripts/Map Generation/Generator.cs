using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using Unity.AI.Navigation;
using UnityEngine.AI;
//using static UnityEditor.FilePathAttribute;

public class Generator : MonoBehaviour
{
    enum CellType {None, Room, Hallway}

    class Room {
        public RectInt bounds;

        public Room(Vector2Int location, Vector2Int size){
            bounds = new RectInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }
    }
    private Dictionary<Vector2Int, GameObject> structureInstances = new Dictionary<Vector2Int, GameObject>();
    private bool IsWinRoomPlaced = false;
    
    // Generator Settings
    [Header("Generator Settings")]
    [SerializeField] private int seed;
    [SerializeField] private Vector2Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private int EnemyRoomCount;
    [SerializeField] private int ItemRoomCount;
    [SerializeField] private Vector2Int roomMaxSize;
    [SerializeField] private NavMeshSurface surface;

    // Room Type Sizes
    [Header("Room Type Sizes")]
    [SerializeField] private Vector2Int StarterRoomSize = new Vector2Int(6, 6);
    [SerializeField] private Vector2Int WinRoomSize = new Vector2Int(6, 6);
    [SerializeField] private Vector2Int EnemyRoomSize = new Vector2Int(1, 1);
    [SerializeField] private Vector2Int ItemRoomSize = new Vector2Int(4, 4);

    // Prefabs for Room Types
    [Header("Room Type Prefabs")]
    [SerializeField] private GameObject StarterRoomPrefab;
    [SerializeField] private GameObject ItemRoomPrefab;
    [SerializeField] private GameObject EnemySpawnRoomPrefab;
    [SerializeField] private GameObject WinRoomPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private GameObject HallwayPrefab;

    // Prefabs for Interactive Elements
    [Header("Interactive Element Prefabs")]
    [SerializeField] private GameObject FoodPrefab;
    [SerializeField] private GameObject WaterPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject EnemyPrefab;

    // Premade Room Prefabs
    [Header("Premade Room Prefabs")]
    [SerializeField] private List<RoomPrefabEntry> PremadeRoomPrefabs = new List<RoomPrefabEntry>();

    [System.Serializable]
    public class RoomPrefabEntry
    {
        public Vector2Int RoomSize; // The size of the room (key)
        public GameObject Prefab;  // The prefab to use for this size (value)
    }



    private Dictionary<Vector2Int, GameObject> premadeRoomPrefabsLookup;

    Random random;
    Grid2D<CellType> grid;
    List<Room> rooms;
    Delaunay2D delaunay;
    HashSet<Prim.Edge> selectedEdges;


    // Start is called before the first frame update
    void Start()
    {
        if (GeneralSettings.Instance != null){

            Debug.Log($"Seed: {GeneralSettings.Instance.Seed}");
            Debug.Log($"Room Count: {GeneralSettings.Instance.RoomCount}");
            Debug.Log($"Enemy Room Count: {GeneralSettings.Instance.EnemyRoomCount}");
            Debug.Log($"Item Room Count: {GeneralSettings.Instance.ItemRoomCount}");

            seed = GeneralSettings.Instance.Seed;
            roomCount = GeneralSettings.Instance.RoomCount;
            EnemyRoomCount = GeneralSettings.Instance.EnemyRoomCount;
            ItemRoomCount = GeneralSettings.Instance.ItemRoomCount;
        }

        InitializeRoomPrefabs();
        var spawnInfo = Generate();
        // Bake NavMeshSurface after level is generated
        GenerateMesh();
        InstantiateCharacters(spawnInfo.Item1, spawnInfo.Item2);
    }

    private void InitializeRoomPrefabs()
    {
        premadeRoomPrefabsLookup = new Dictionary<Vector2Int, GameObject>();

        foreach (var entry in PremadeRoomPrefabs)
        {
            if (!premadeRoomPrefabsLookup.ContainsKey(entry.RoomSize))
            {
                premadeRoomPrefabsLookup.Add(entry.RoomSize, entry.Prefab);
            }
            else
            {
                Debug.LogWarning($"Duplicate entry for room size {entry.RoomSize} in PremadeRoomPrefabs.");
            }
        }
    }

    void GenerateMesh() {
        surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    ((Vector2Int, Vector2Int), (Vector2Int, Vector2Int)[]) Generate(){
        if (seed == 0) {
        seed = System.DateTime.Now.Millisecond;
        }
        random = new Random(seed);
        grid = new Grid2D<CellType>(size, Vector2Int.zero);
        rooms = new List<Room>();

        var pSpawnInfo = PlaceStarterRoom();
        PlaceWinRoom();
        var eSpawnInfo = PlaceEnemyRoom();
        PlaceItemRoom();
        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();

        return (pSpawnInfo, eSpawnInfo);
    }

    //Spawns all characters after the NavMesh is generated
    void InstantiateCharacters((Vector2Int, Vector2Int) pSpawnInfo, (Vector2Int, Vector2Int)[] eSpawnInfo)
    {
        //Spawns the player
        Instantiate(PlayerPrefab, new Vector3(pSpawnInfo.Item1.x + pSpawnInfo.Item2.x / 2, 0.5f, pSpawnInfo.Item1.y + pSpawnInfo.Item2.y / 2), Quaternion.identity);
        foreach (var item in eSpawnInfo)
        {
            // Spawns enemies in the enemy spawn room
            Vector3 spawnPosition = new Vector3(item.Item1.x + item.Item2.x / 2 + 0.5f, 0.1f, item.Item1.y + item.Item2.y / 2 + 0.5f);
            NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 10f, 1);
            Instantiate(EnemyPrefab, hit.position, Quaternion.identity); 
        }
    }

    //returns location and roomSize info to spawn the player after the NavMesh is generated
    (Vector2Int, Vector2Int) PlaceStarterRoom()
    {
        Vector2Int location = new Vector2Int(
            size.x / 2 - StarterRoomSize.x / 2,
            size.y / 2 - StarterRoomSize.y / 2
        );

        Room startingRoom = new Room(location, StarterRoomSize);
        rooms.Add(startingRoom);
        PlaceRoom(startingRoom.bounds.position, startingRoom.bounds.size, StarterRoomPrefab);

        foreach (var pos in startingRoom.bounds.allPositionsWithin)
        {
            grid[pos] = CellType.Room;
        }

        return (location, StarterRoomSize);
    }


    void PlaceRooms() {
        for (int i = 0; i < roomCount; i++) {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y) {
                add = false;
            }

            if (add) {
                rooms.Add(newRoom);
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size, RoomPrefab);

                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
            }
        }
    }
// The Win Room should only be placed once in a generation
// Same Logic to PlaceRooms except for IsWinRoomPlaced Bool
    void PlaceWinRoom()
    {
        while (!IsWinRoomPlaced)
        {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Room winRoom = new Room(location, WinRoomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), WinRoomSize + new Vector2Int(2, 2));

            bool add = true;
            foreach (var room in rooms)
            {
                if (Room.Intersect(room, buffer))
                {
                    add = false;
                    break;
                }
            }

            if (winRoom.bounds.xMin < 0 || winRoom.bounds.xMax >= size.x ||
                winRoom.bounds.yMin < 0 || winRoom.bounds.yMax >= size.y)
            {
                add = false;
            }

            if (add)
            {
                rooms.Add(winRoom);
                PlaceRoom(winRoom.bounds.position, winRoom.bounds.size, WinRoomPrefab);

                foreach (var pos in winRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                }

                IsWinRoomPlaced = true;
            }
        }
    }
//Uses user input to determine how many rooms
    void PlaceItemRoom()
    {
        int loopCounter = 0;
        while (loopCounter < ItemRoomCount)
        {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Room itemRoom = new Room(location, ItemRoomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), ItemRoomSize + new Vector2Int(2, 2));

            bool add = true;
            foreach (var room in rooms)
            {
                if (Room.Intersect(room, buffer))
                {
                    add = false;
                    break;
                }
            }

            if (itemRoom.bounds.xMin < 0 || itemRoom.bounds.xMax >= size.x ||
                itemRoom.bounds.yMin < 0 || itemRoom.bounds.yMax >= size.y)
            {
                add = false;
            }

            if (add)
            {
                rooms.Add(itemRoom);
                PlaceRoom(itemRoom.bounds.position, itemRoom.bounds.size, ItemRoomPrefab);

                GameObject randomPrefab = UnityEngine.Random.value > 0.5f ? FoodPrefab : WaterPrefab;
                Instantiate(randomPrefab, new Vector3(location.x + ItemRoomSize.x / 2 + 0.5f, 0.5f, location.y + ItemRoomSize.y / 2 + 0.5f), Quaternion.identity);

                foreach (var pos in itemRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                }

                loopCounter++;
            }
        }
    }


    //returns location and roomSize info to spawn the enemies after the NavMesh is generated
    (Vector2Int, Vector2Int)[] PlaceEnemyRoom()
    {
        int loopCounter = 0;
        (Vector2Int, Vector2Int)[] spawnInfo = new (Vector2Int, Vector2Int)[EnemyRoomCount];

        while (loopCounter < EnemyRoomCount)
        {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Room enemyRoom = new Room(location, EnemyRoomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), EnemyRoomSize + new Vector2Int(2, 2));

            bool add = true;
            foreach (var room in rooms)
            {
                if (Room.Intersect(room, buffer))
                {
                    add = false;
                    break;
                }
            }

            if (enemyRoom.bounds.xMin < 0 || enemyRoom.bounds.xMax >= size.x ||
                enemyRoom.bounds.yMin < 0 || enemyRoom.bounds.yMax >= size.y)
            {
                add = false;
            }

            if (add)
            {
                rooms.Add(enemyRoom);
                PlaceRoom(enemyRoom.bounds.position, enemyRoom.bounds.size, EnemySpawnRoomPrefab);

                spawnInfo[loopCounter] = (location, EnemyRoomSize);
                loopCounter++;
            }
        }

        return spawnInfo;
    } 

    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector2)room.bounds.position + ((Vector2)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay2D.Triangulate(vertices);
    }

    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < 0.125) {
                selectedEdges.Add(edge);
            }
        }
    }

    void PathfindHallways() {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (grid[b.Position] == CellType.Room) {
                    pathCost.cost += 10;
                } else if (grid[b.Position] == CellType.None) {
                    pathCost.cost += 5;
                } else if (grid[b.Position] == CellType.Hallway) {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                //Fixed walls appearing at hallway crossroads and hallway/room intersections here
                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway && !structureInstances.ContainsKey(pos)) {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

// Removes walls of structures that are adjacent to each other 
    void RemoveAdjacentWalls(){
        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var structure in structureInstances) {
            Vector2Int location = (Vector2Int)structure.Key;
            GameObject structureInstance = structure.Value;

            foreach (var direction in directions) {
                Vector2Int neighborPos = location + direction;
                
                //This is the part that removes the walls, and the walls of any neighboring cells, if you have issues, check RemoveWall()
                if (structureInstances.ContainsKey(neighborPos)) {
                    GameObject neighborStructure = structureInstances[neighborPos];
                    RemoveWall(structureInstance, direction);

                    Vector2Int oppositeDirection = new Vector2Int(-direction.x, -direction.y);
                    RemoveWall(neighborStructure, oppositeDirection);
                }
            }
        }
    }

    void RemoveWall(GameObject structureInstance, Vector2Int direction) {
        // XWall refers to the Positive X direction, NWall is the Negative X direction, same logic applies to Z di - use this naming convention plz
        string wallName = direction == new Vector2Int(1, 0) ? "XWall" :
                        direction == new Vector2Int(-1, 0) ? "NXWall" :
                        direction == new Vector2Int(0, 1) ? "ZWall" :
                        direction == new Vector2Int(0, -1) ? "NZWall" :
                        null;
        // Important that the walls are tagged "Walls", if not the code wont find em' and you'll have walls
        // If there are walls where you don't want em', go into the editor click on the prefab, check the inspection panel for the tags and tag the walls 
        if (wallName != null) {
            Transform wallsTransform = structureInstance.transform.Find("Walls");
            if (wallsTransform != null) {
                Transform wallTransform = wallsTransform.Find(wallName);
                if (wallTransform != null) {
                    wallTransform.gameObject.SetActive(false);
                } 
                else {
                    Debug.LogWarning($"Wall {wallName} not found in {structureInstance.name}"); //hail mary
                }
            } 
            else {
                Debug.LogWarning($"Walls container not found in {structureInstance.name}"); //Phantom Walls beware
            }
        }
    }


// This is what makes the rooms modular, if not for this, the rooms would be stretched and distorted prefabs, this makes it so that one prefab is used over and over to populate the space of the room based on the original user input
// If rooms are not working correctly, 50/50 chance this here is the culprit, the other 50% is somewhere in PlaceRooms()
    void PlaceRoom(Vector2Int location, Vector2Int size, GameObject defaultRoomPrefab)
    {
        // Check if there is a premade prefab for the given size
        if (premadeRoomPrefabsLookup.TryGetValue(size, out GameObject premadePrefab))
        {
            // Instantiate the prefab
            GameObject roomInstance = Instantiate(premadePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);

            // Iterate through each sub-room in the prefab
            foreach (Transform subRoom in roomInstance.transform)
            {
                if (subRoom.name.StartsWith("Room"))
                {
                    // Calculate the grid position for the sub-room
                    Vector2Int subRoomPos = new Vector2Int(
                        Mathf.RoundToInt(subRoom.position.x),
                        Mathf.RoundToInt(subRoom.position.z)
                    );

                    // Treat the sub-room as a separate room
                    structureInstances[subRoomPos] = subRoom.gameObject;
                    grid[subRoomPos] = CellType.Room;
                }
            }

            // Remove walls for modular sub-rooms
            RemoveAdjacentWalls();
        }
        else
        {
            // Default modular generation
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int offset = new Vector2Int(i, j);
                    Vector2Int prefabPosition = location + offset;
                    GameObject roomInstance = Instantiate(defaultRoomPrefab, new Vector3(prefabPosition.x, 0, prefabPosition.y), Quaternion.identity);
                    structureInstances[prefabPosition] = roomInstance;
                    grid[prefabPosition] = CellType.Room;
                }
            }

            // Remove walls for modular room
            RemoveAdjacentWalls();
        }
    }


    void PlaceHallway(Vector2Int location) {
        GameObject hallwayInstance = Instantiate(HallwayPrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        structureInstances[location] = hallwayInstance;
        RemoveAdjacentWalls();
    }
}