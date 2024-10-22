using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using Unity.AI.Navigation;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;

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
    
    [SerializeField]
    int seed;
    [SerializeField]
    GameObject PlayerPrefab;
    [SerializeField]
    GameObject EnemyPrefab;
    [SerializeField]
    GameObject StarterRoomPrefab;
    [SerializeField]
    GameObject ItemRoomPrefab;
    [SerializeField]
    GameObject EnemySpawnRoomPrefab;
    [SerializeField]
    GameObject WinRoomPrefab;
    [SerializeField]
    GameObject RoomPrefab;
    [SerializeField]
    GameObject HallwayPrefab;
    [SerializeField]
    Vector2Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    int EnemyRoomCount;
    [SerializeField]
    int ItemRoomCount;
    [SerializeField]
    Vector2Int roomMaxSize;
    [SerializeField]
    NavMeshSurface surface;


    Random random;
    Grid2D<CellType> grid;
    List<Room> rooms;
    Delaunay2D delaunay;
    HashSet<Prim.Edge> selectedEdges;


    // Start is called before the first frame update
    void Start()
    {
        var spawnInfo = Generate();
        // Bake NavMeshSurface after level is generated
        GenerateMesh();
        InstantiateCharacters(spawnInfo.Item1, spawnInfo.Item2);

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
    (Vector2Int, Vector2Int) PlaceStarterRoom(){
        Vector2Int location = new Vector2Int(
        size.x / 2 - roomMaxSize.x / 2, 
        size.y / 2 - roomMaxSize.y / 2
        );

        Vector2Int roomSize = new Vector2Int(6,6);

        Room startingRoom = new Room(location, roomSize);
        rooms.Add(startingRoom);
        PlaceRoom(startingRoom.bounds.position, startingRoom.bounds.size, StarterRoomPrefab);

        foreach (var pos in startingRoom.bounds.allPositionsWithin) {
            grid[pos] = CellType.Room;
        }

        return (location, roomSize);
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
    void PlaceWinRoom(){
        while (IsWinRoomPlaced == false) {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(6,6);

            bool add = true;
            Room WinRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (WinRoom.bounds.xMin < 0 || WinRoom.bounds.xMax >= size.x
                || WinRoom.bounds.yMin < 0 || WinRoom.bounds.yMax >= size.y) {
                add = false;
            }

            if (add) {
                rooms.Add(WinRoom);
                PlaceRoom(WinRoom.bounds.position, WinRoom.bounds.size, WinRoomPrefab);

                foreach (var pos in WinRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
                IsWinRoomPlaced = true;
            }
        }
    }
//Uses user input to determine how many rooms
    void PlaceItemRoom(){
        int LoopCounter = 0;
        while (LoopCounter < ItemRoomCount) {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(4,4);

            bool add = true;
            Room ItemRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (ItemRoom.bounds.xMin < 0 || ItemRoom.bounds.xMax >= size.x
                || ItemRoom.bounds.yMin < 0 || ItemRoom.bounds.yMax >= size.y) {
                add = false;
            }

            if (add) {
                rooms.Add(ItemRoom);
                PlaceRoom(ItemRoom.bounds.position, ItemRoom.bounds.size, ItemRoomPrefab);

                foreach (var pos in ItemRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
                LoopCounter ++;
            }
        }
    }

    //returns location and roomSize info to spawn the enemies after the NavMesh is generated
    (Vector2Int, Vector2Int)[] PlaceEnemyRoom(){
        int LoopCounter = 0;
        (Vector2Int, Vector2Int)[] spawnInfo = new (Vector2Int, Vector2Int)[EnemyRoomCount];
        while (LoopCounter < EnemyRoomCount) {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(1,1);

            bool add = true;
            Room EnemySpawnRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (EnemySpawnRoom.bounds.xMin < 0 || EnemySpawnRoom.bounds.xMax >= size.x
                || EnemySpawnRoom.bounds.yMin < 0 || EnemySpawnRoom.bounds.yMax >= size.y) {
                add = false;
            }

            if (add) {
                rooms.Add(EnemySpawnRoom);
                PlaceRoom(EnemySpawnRoom.bounds.position, EnemySpawnRoom.bounds.size, EnemySpawnRoomPrefab);
                foreach (var pos in EnemySpawnRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
                spawnInfo[LoopCounter] = (location, roomSize);
                LoopCounter ++;
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
    void PlaceRoom(Vector2Int location, Vector2Int size, GameObject RoomPrefab) {
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                Vector2Int offset = new Vector2Int(i, j);
                Vector2Int prefabPosition = location + offset;
                GameObject roomInstance = Instantiate(RoomPrefab, new Vector3(prefabPosition.x, 0, prefabPosition.y), Quaternion.identity);
                structureInstances[prefabPosition] = roomInstance;
                // If walls are populating the room, check RemoveAdjacentWall()
                RemoveAdjacentWalls();
            }
        }
    }

    void PlaceHallway(Vector2Int location) {
        GameObject hallwayInstance = Instantiate(HallwayPrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        structureInstances[location] = hallwayInstance;
        RemoveAdjacentWalls();
    }
}
