using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;

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

    [SerializeField]
    int seed;
    [SerializeField]
    GameObject RoomPrefab;
    [SerializeField]
    GameObject HallwayPrefab;
    [SerializeField]
    Vector2Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector2Int roomMaxSize;
    [SerializeField]
    

    Random random;
    Grid2D<CellType> grid;
    List<Room> rooms;
    Delaunay2D delaunay;
    HashSet<Prim.Edge> selectedEdges;


    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    void Generate(){
            if (seed == 0) {
        seed = System.DateTime.Now.Millisecond;
        }
        random = new Random(seed);
        grid = new Grid2D<CellType>(size, Vector2Int.zero);
        rooms = new List<Room>();

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
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
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
            }
        }
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

                //Fixed walls appearing where they should not be
                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway && !structureInstances.ContainsKey(pos)) {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

// Gib those walls
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
                
                //This is the part that removes the walls, and the walls of any neighboring cells
                if (structureInstances.ContainsKey(neighborPos)) {
                    GameObject neighborStructure = structureInstances[neighborPos];
                    RemoveWall(structureInstance, direction);

                    Vector2Int oppositeDirection = new Vector2Int(-direction.x, -direction.y);
                    RemoveWall(neighborStructure, oppositeDirection);
                }
            }
        }
    }

//Gib that specific wall
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
    void PlaceRoom(Vector2Int location, Vector2Int size) {
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
