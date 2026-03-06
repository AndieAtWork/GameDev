using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class MazeManager : MonoBehaviour
{

    public static MazeManager Instance { get; private set; }

    // The kind of cell the game will use as floor
    public GameObject cellTypePrefab;
    // The gateways between rooms
    public GameObject warpPrefab;
    // Room colliders
    public GameObject wallPrefab;

    public int roomNumber; // Number of rooms in the maze
    public int frameSize; // The max square size of the rooms 

    public int minBoxNumber; // How many rectangles make up each room
    public int maxBoxNumber;

    public int roomMinWidth; // The sizes. They can be inverted
    public int roomMaxWidth;
    public int roomMinHeigth;
    public int roomMaxHeigth;

    public int compression; // Sets how close together the different rectangles are to eachother

    private int _distancing = 10; // The distance between frames so that the player can't see other rooms
    private Room[] _rooms; // The array of rooms in the maze
    private HashSet<GameObject> _gateWays; // all gateways from every room

    public GameObject[] elementsPrefab;
    public int[] elementsChance; // Chance of there being an elemet
    public int[] elementsDice; // How many times that chance is played (also max times it can appear

    public string nextScene; // Where the last room leads
    public string levelName; // Name of the JSON file

    // Top is an opening from the top of the room
    private static readonly string[] gateDispositionOptions = { "T", "R", "B", "L" };
    // A Top door is matched with a bottom door
    private static readonly Dictionary<string, string> reversedDispositions = new Dictionary<string, string>
    {
        { "T", "B" },
        { "B", "T" },
        { "R", "L" },
        { "L", "R" }
    };
    // The player doesn't appear in the opposite gate, but a square away so they dont keep teleporting
    private static readonly Dictionary<string, int[]> dispositionsModifier = new Dictionary<string, int[]>
    {
        { "T", new int[] { 0, -1 } },
        { "R", new int[] { -1, 0 } },
        { "B", new int[] { 0, 1 } },
        { "L", new int[] { 1,  0 } }
    };


    private void Awake()
    {
        // The Maze is a singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Main function: MAP IS CREATED AND STORED INTO A JSON
    void CreateMap()
    {
        _rooms = new Room[roomNumber];
        int xIndex = 0;
        int yIndex = 0;
        for (int i = 0; i < roomNumber; i++)
        {
            _rooms[i] = new Room();
            _rooms[i].CreateFrame();
            _rooms[i].PaintAllCells(xIndex, yIndex); // Cells are instantiated

            xIndex = xIndex + frameSize + _distancing; // Se van a imprimir en fila por el eje x
        }

        _gateWays = new HashSet<GameObject>(); // All gates

        PlaceGateWays(_rooms); // Primary gateways are instantiated in all rooms
        //PlaceAllBorders(); // Colliders are instantiated in all map
        //commented as there is no need to load them in the maze creator

        PlaceAllBarriers(); // Inner Barriers are placed
        PlaceAllElements(); // Scenery Elements are placed

        CreateJson(); // Saves the rooms in a json file

    }

    // Runs through a series of given rooms and connects them
    private void PlaceGateWays(Room[] rooms) // Rooms are in order 
    {

        for (int i = 0; i < rooms.Length; i++)
        {
            Cell[] enterRoom1Array;
            Cell[] exitRoom2Array;
            Room room1;
            Room room2;
            GameObject gate1;
            GameObject gate2;

            var shuffledOrientations = gateDispositionOptions.OrderBy(_ => Random.value).ToArray();
            // In each room, orientations are shuffled

            if (i < rooms.Length - 1) // Not last room
            {
                int index = 0; // Switches orientations so they are all picked
                string orientation1;
                string orientation2;

                bool notFound = false;
                do
                {
                    room1 = rooms[i];
                    room2 = rooms[i + 1];

                    orientation1 = shuffledOrientations[index];
                    orientation2 = reversedDispositions[orientation1];

                    enterRoom1Array = GateDispositionCells(orientation1, room1);
                    exitRoom2Array = GateDispositionCells(orientation2, room2);

                    index = index + 1;

                }
                while (enterRoom1Array.Length == 0 || exitRoom2Array.Length == 0);
                index = 0;

                if (notFound)
                {
                    continue;
                }

                // Selects random cells within the possible spots for a gate
                Cell enterCell = room1.GetRandomCell(new HashSet<Cell>(enterRoom1Array));
                Cell exitCell = room2.GetRandomCell(new HashSet<Cell>(exitRoom2Array));

                // GameObjects are created
                gate1 = room1.SetRoomGates(enterCell, orientation1);
                gate2 = room2.SetRoomGates(exitCell, orientation2);

                // GateBehaviour is set 
                GateWay gateBehaviour1 = gate1.GetComponent<GateWay>();
                int[] warpCoord1 = gateBehaviour1.GetFromScreenPositions();
                int[] warpCoord1ToModifier = dispositionsModifier[orientation1];

                GateWay gateBehaviour2 = gate2.GetComponent<GateWay>();
                int[] warpCoord2 = gateBehaviour2.GetFromScreenPositions();
                int[] warpCoord2ToModifier = dispositionsModifier[orientation2];

                // The current gate leads to the position of the next gate, with applied modifier
                gateBehaviour1.SetToScreenPositions(warpCoord2[0] + warpCoord2ToModifier[0], warpCoord2[1] + warpCoord2ToModifier[1], i + 2);
                // The second gate leads to the position of the current gate with applied modifier
                gateBehaviour2.SetToScreenPositions(warpCoord1[0] + warpCoord1ToModifier[0], warpCoord1[1] + warpCoord1ToModifier[1], i + 1);

                // Gates are appended to the maze's set
                _gateWays.Add(gate1);
                _gateWays.Add(gate2);
            }
            else // Last room
            {
                string orientation; // Orientarion is shuffled too
                int index = 0;
                do
                {
                    orientation = shuffledOrientations[index];
                    room2 = rooms[i];
                    exitRoom2Array = GateDispositionCells(orientation, room2);

                    index = index + 1;
                } while (exitRoom2Array.Length == 0);

                Cell exitCell = room2.GetRandomCell(new HashSet<Cell>(exitRoom2Array));
                gate2 = room2.SetRoomGates(exitCell, orientation);
                GateWay gateBehaviour2 = gate2.GetComponent<GateWay>();
                gateBehaviour2.SetToScreenPositions(0, 0, 0, nextScene);

                _gateWays.Add(gate2);

            }

        }
    }

    // Within a room, get the valid positions for a gateway
    private Cell[] GateDispositionCells(string orientation, Room room)
    {
        return room.GetAllGateBorders(orientation).ToArray();
    }

    // Instantiates de borders (including separatedly the corners)
    public void PlaceAllBorders()
    {
        foreach (Room room in _rooms)
        {
            room.SetRoomBorders();
        }
    }

    // Barriers are both walls and rivers
    private void PlaceAllBarriers()
    {
        BarrierDisposition disposition = GetComponent<BarrierDisposition>();
        // BarrierDisposition has all the data on how to implement the barrier of the Maze
        int barrierDice = disposition.barrierDice; // how many times the chances are played; Also max amount of barriers
        int barrierChance = disposition.barrierChance; // chance of there being a barrier for each dice
        GameObject barrierPrefab = disposition.barrierPrefab;

        for (int i = 0; i < _rooms.Length; i++)
        {
            int happenings = 0; // How many barriers
            for (int dice = 0; dice < barrierDice; dice++)
            {
                int numero = UnityEngine.Random.Range(1, 101);
                if (numero <= barrierChance)
                {
                    happenings += 1;
                }
            }
            for (int h = 0; h < happenings; h++)
            {
                Barrier barrier = barrierPrefab.GetComponent<Barrier>();
                barrier.PlaceObject(_rooms[i]);
            }
        }

    }

    // Scenery elements are places around the rooms
    private void PlaceAllElements()
    {
        for (int i = 0; i < _rooms.Length; i++)
        {
            List<GameObject> sceneryList = new List<GameObject>();

            for (int e = 0; e < elementsPrefab.Length; e++)
            {
                // Each element has a prefab, a number of dice, and a chance of happening
                // Same logic as the method above
                GameObject elementPrefab = elementsPrefab[e];
                int elementChance = elementsChance[e];
                int elementDice = elementsDice[e];

                int happenings = 0;
                for (int dice = 0; dice < elementDice; dice++)
                {
                    int numero = UnityEngine.Random.Range(1, 101);
                    if (numero <= elementChance)
                    {
                        happenings += 1;
                    }
                }
                for (int h = 0; h < happenings; h++)
                {
                    sceneryList.Add(elementPrefab);
                }
            }

            GameObject[] sceneryArray = sceneryList.ToArray(); 
            // The list is turned into an array and disordered
            // This way no element has an advantage of fitting into the scene
            Room.Shuffle(sceneryArray);

            for (int x = 0; x < sceneryArray.Length; x++)
            {
                Debug.Log("SCENERY IS PLACED");
                SceneryElement scenery = sceneryArray[x].GetComponent<SceneryElement>();
                scenery.PlaceObject(_rooms[i]);
            }

        }
    }

    private void CreateJson() // Loads all the scene and stores it in a JSON file
    {
        Debug.Log("Creating json...");
        Debug.Log(Application.persistentDataPath);

        string path = Path.Combine(
            Application.persistentDataPath,
            levelName + ".json"
        );

        //if (File.Exists(path))
        //    return;

        string[] roomJsonArray = new string[_rooms.Length];
        for (int r = 0; r < _rooms.Length; r++) // Each room is stored separatedly
        {
            string roomJson = "";
            Cell[][] grid = _rooms[r].GetGrid();
            string[] cellsArray = new string[frameSize * frameSize]; // The cells are a one dimensional array, with a given frame height/width to read

            int count = 0;
            for (int y = 0; y < frameSize; y++) 
            {
                for (int x = 0; x < frameSize; x++)
                {
                    Cell cell = grid[x][y];
                    if (cell == null)
                    {
                        string cellJson = "{\"status\": 0}"; // Status 0 means no part of the maze/No Cells
                        cellsArray[count] = cellJson;
                    }
                    else
                    {
                        CellBehaviour cellBehaviour = cell.GetCellObject().GetComponent<CellBehaviour>();
                        int tileCode = cellBehaviour.code;
                        int elementCode = cellBehaviour.elementCode;
                        int elementOrientation = cellBehaviour.elementOrientation;

                        int nextGT = cell.nextToGatewayT ? 1 : 0;
                        int nextGB = cell.nextToGatewayB ? 1 : 0;
                        int nextGR = cell.nextToGatewayR ? 1 : 0;
                        int nextGL = cell.nextToGatewayL ? 1 : 0;
                        int[] nextToGatewayArray = { nextGT, nextGB, nextGR, nextGL };
                        string nextToGatewayStr = $"[{string.Join(",", nextToGatewayArray)}]";

                        string cellJson = "{\"status\": 1, " +
                                            "\"tile\": " + tileCode.ToString() + ", " +
                                            "\"elementCode\": " + elementCode.ToString() + ", " +
                                            "\"elementOrientation\": " + elementOrientation.ToString() + ", " +
                                            "\"nextGateway\":" + nextToGatewayStr + "}";
                        cellsArray[count] = cellJson;
                    }
                    count++;
                }
            }
            // Each room has cells and gateways
            GameObject[] gatewaysArray = _rooms[r].GetGateways().ToArray();
            string[] gatesJsonArray = new string[gatewaysArray.Length];
            for (int g = 0; g < gatewaysArray.Length; g++)
            {
                GameObject gateObject = gatewaysArray[g];
                GateWay gate = gateObject.GetComponent<GateWay>();
                string gateJson = "{\"coordsPosition\":" + $"[{string.Join(",", gate.GetFromScreenPositions())}], " + //Position is where they are placed
                                    "\"coordsDestination\":" + $"[{string.Join(",", gate.GetToScreenPositions())}], " + //Destination where they lead
                                    "\"toRoom\":" + gate.toRoomID + "}"; //The room they lead to

                gatesJsonArray[g] = gateJson;
            }

            roomJson =
                                $@"{{ 
                                    ""room_number"": {r + 1},
                                    ""cells"": [{string.Join(",", cellsArray)}],
                                    ""gates"": [{string.Join(",", gatesJsonArray)}]
                                  }}";
            roomJsonArray[r] = roomJson;
        }

        // General information: number of rooms, frame, distancing between frames, info on room Array
        string roomsJson = "[" + string.Join(",", roomJsonArray) + "]";
        string json = "{\"rooms_n\": " + roomNumber.ToString() + ", " +
                        "\"frame_size\": " + frameSize.ToString() + ", " +
                        "\"distancing\":" + _distancing.ToString() + ", " +
                        "\"rooms\": " + roomsJson + " }";

        File.WriteAllText(path, json);
        // After the json file is created, the game should go to the next scene and keep creating the rest of the maze
        // So it is all loaded by the time the player starts playing
    } 
}
