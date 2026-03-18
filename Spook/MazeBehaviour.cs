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
    private HashSet<GateWay> _setGateWays; // all gateways for set rooms
    GameObject _setGateWaysObject; // To store set gates

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
        _setGateWays = new HashSet<GateWay>(); // Gates from the set rooms

        _setGateWaysObject = new GameObject("SetGates");

        PlaceSetGateWays1(_rooms); // Entrance and Exit
        PlaceGateWays(_rooms); // Primary gateways are instantiated in all rooms in a horizontal sequence
        //PlaceSetGateWays2(_rooms); // Set rooms are positionated. They are stored in the Maze Manager's component RoomDisposition. Spares first and last
        //PlaceSecondaryGateWays(_rooms); // Random gates are placed all over the maze


        PlaceAllBorders(); // Colliders are instantiated in all map
        //commented as there is no need to load them in the maze creator

        //PlaceAllBarriers(); // Inner Barriers are placed
        //PlaceAllElements(); // Scenery Elements are placed

        MazeSaving.CreateJson(frameSize, _rooms, _distancing, roomNumber, levelName, _setGateWays.ToArray()); // Saves the rooms in a json file

    }

    private void PlaceSetGateWays1(Room[] rooms)
    {
        // From disposition you get the screen coordinates of the Set Rooms
        RoomDisposition disposition = GetComponent<RoomDisposition>(); // All info on Room disposition is loaded 
        int[] xPositions = disposition.gatewaysPositionX; // Position of the gate
        int[] yPositions = disposition.gatewaysPositionY; // Position of the gate
        string[] gatewaysOrientation = disposition.gatewaysOrientation; // Dispositions: when to turn when returning

        // Firstly, data on the entrance Room => first in the array
        int entrancePositionX = xPositions[0]; // Position of the gateway
        int entrancePositionY = yPositions[0]; // Position of the gateway
        string entranceOrientation = gatewaysOrientation[0]; // Where to turn to when returning

        //Debug.Log("entranceOrientation " + entranceOrientation);

        // Secondly, data on the exit Room => last in the array
        int exitPositionX = xPositions[xPositions.Length - 1];
        int exitPositionY = yPositions[yPositions.Length - 1];
        string exitOrientation = gatewaysOrientation[gatewaysOrientation.Length - 1];

        //Debug.Log("exitOrientation " + exitOrientation);

        // The rooms that will be linked
        Room entranceRoom = rooms[0];
        Room exitRoom = rooms[rooms.Length - 1];

        // Now that we have the info on the SET Gates, we get the position and orientation of the normal rooms.
        // This are the orientations the player turn to when crossing the set gates
        string primaryEntranceOrientation = null;
        string primaryExitOrientation = null;

        // The disposition is shuffled both times so we get a variety of orientations in our created gates 
        Room.Shuffle(gateDispositionOptions);
        Cell enterCell = null;
        for (int i = 0; i < 4; i++) // All the orientations
        {
            primaryEntranceOrientation = gateDispositionOptions[i]; // randomly ordered orientations
            Cell[] enterRoomArray = GateDispositionCells(primaryEntranceOrientation, entranceRoom); // Valid gate positions within the grid, in the normal room
            enterCell = entranceRoom.GetRandomCell(new HashSet<Cell>(new HashSet<Cell>(enterRoomArray))); // A random cell in that set of gates
            if (enterCell != null) // Once you find one
            {
                break;
            }
        }
        Room.Shuffle(gateDispositionOptions);
        Cell exitCell = null;
        for (int i = 0; i < 4; i++)
        {
            primaryExitOrientation = gateDispositionOptions[i];
            Cell[] exitRoomArray = GateDispositionCells(primaryExitOrientation, exitRoom);
            exitCell = exitRoom.GetRandomCell(new HashSet<Cell>(new HashSet<Cell>(exitRoomArray)));
            if (exitCell != null)
            {
                break;
            }
        }

        //Debug.Log("EnterCell orientation " + primaryEntranceOrientation);
        //Debug.Log("ExitCell orientation " + primaryExitOrientation);

        // Now we get the information on the receiving cells and their modifier (sets where the player is set, in this case, using the orientations we just got)
        int[] entranceScreenCoords = enterCell.ScreenCoordinates();
        int[] entranceModifier = dispositionsModifier[primaryEntranceOrientation];

        int[] exitScreenCoords = exitCell.ScreenCoordinates();
        int[] exitModifier = dispositionsModifier[primaryExitOrientation];


        // ----------------------To create a gateway that leads from the set to the regular rooms -------------------------------

        // Gate from First Set Room to Regular Room
        GameObject entranceGateObject = UnityEngine.Object.Instantiate(
            MazeManager.Instance.warpPrefab,
            new Vector2(entrancePositionX, entrancePositionY),
            Quaternion.identity,
            _setGateWaysObject.transform
        );
        GateWay entranceGate = entranceGateObject.GetComponent<GateWay>();
        int[] entranceCoords = enterCell.ScreenCoordinates();
        entranceGate.SetToScreenPositions(entranceCoords[0], entranceCoords[1], 1); // When crossing the door, leads to cell
        entranceGate.SetDestinationOrientation(primaryEntranceOrientation); // The orientation of a gate is where they lead to

        // Gate from Last Set Room to Regular Room
        GameObject exitGateObject = UnityEngine.Object.Instantiate(
            MazeManager.Instance.warpPrefab,
            new Vector2(exitPositionX, exitPositionY),
            Quaternion.identity,
            _setGateWaysObject.transform
        );
        GateWay exitGate = exitGateObject.GetComponent<GateWay>();
        int[] exitCoords = exitCell.ScreenCoordinates();
        exitGate.SetToScreenPositions(exitCoords[0], exitCoords[1], rooms.Length);
        exitGate.SetDestinationOrientation(primaryExitOrientation);

        // Special gates are stored separatedly from room gates
        _setGateWays.Add(entranceGate);
        _setGateWays.Add(exitGate);

        // ----------------------To create a gateway that leads from the regular rooms to the entrance and exit rooms -------------------------------

        GateWay gate1 = entranceRoom.SetRoomGates(enterCell, primaryEntranceOrientation).GetComponent<GateWay>();
        GateWay gate2 = exitRoom.SetRoomGates(exitCell, primaryExitOrientation).GetComponent<GateWay>();

        // To get the correct position in set rooms, dispositionMofidifier is applied
        // The correct coords are set to the gateways
        // Note that this gates are stored in the room's gate set and will be stored as such in the json
        int[] entranceDispositionMod = dispositionsModifier[entranceOrientation];
        gate1.SetToScreenPositions(entrancePositionX + entranceDispositionMod[0], entrancePositionY + entranceDispositionMod[1], -1);
        gate1.SetDestinationOrientation(entranceOrientation);

        int[] exitDispositionMod = dispositionsModifier[exitOrientation];
        gate2.SetToScreenPositions(exitPositionX + exitDispositionMod[0], exitPositionY + exitDispositionMod[1], (gatewaysOrientation.Length) * -1);
        gate2.SetDestinationOrientation(exitOrientation);
    }

    // Runs through a series of given rooms and connects them.
    // It takes into account that most of the rooms' space is free, as it is the second run of placing gates
    private void PlaceGateWays(Room[] rooms) // Rooms are in order 
    {

        for (int r = 0; r < rooms.Length; r++)
        {
            if (r < rooms.Length - 1) // Stops at preivous to last
            {
                //Debug.Log("ROOMS " + r.ToString() + " - " + (r+1).ToString());
                Room room1 = rooms[r]; // Joining current room with the next
                Room room2 = rooms[r + 1];

                string room1EntranceOrientation = null;
                string room2EntranceOrientation = null;

                // The disposition is shuffled both times so we get a variety of orientations in our created gates 
                Room.Shuffle(gateDispositionOptions);
                Cell cell1 = null;
                for (int i = 0; i < 4; i++) // All the orientations
                {
                    room1EntranceOrientation = gateDispositionOptions[i]; // randomly ordered orientations
                    Cell[] roomArray1 = GateDispositionCells(room1EntranceOrientation, room1); // Valid gate positions within the grid, in the normal room
                    cell1 = room1.GetRandomCell(new HashSet<Cell>(new HashSet<Cell>(roomArray1))); // A random cell in that set of gates
                    if (cell1 != null) // Once you find one
                    {
                        break;
                    }
                }
                Room.Shuffle(gateDispositionOptions);
                Cell cell2 = null;
                for (int i = 0; i < 4; i++)
                {
                    room2EntranceOrientation = gateDispositionOptions[i];
                    Cell[] roomArray2 = GateDispositionCells(room2EntranceOrientation, room2);
                    cell2 = room2.GetRandomCell(new HashSet<Cell>(new HashSet<Cell>(roomArray2)));
                    if (cell2 != null)
                    {
                        break;
                    }
                }

                //Debug.Log("room1 " + room1EntranceOrientation);
                //Debug.Log("room2 " + room2EntranceOrientation);

                // Here the normal orientation is used to get the gates' location, while when set they are inverted

                // GameObjects are created and stored in the room's gates set
                GameObject gate1 = room1.SetRoomGates(cell1, room1EntranceOrientation);
                GameObject gate2 = room2.SetRoomGates(cell2, room2EntranceOrientation);

                // GateBehaviour is get and set 
                GateWay gateBehaviour1 = gate1.GetComponent<GateWay>();
                int[] warpCoord1 = gateBehaviour1.GetFromScreenPositions();
                int[] warpCoord1ToModifier = dispositionsModifier[room1EntranceOrientation];

                GateWay gateBehaviour2 = gate2.GetComponent<GateWay>();
                int[] warpCoord2 = gateBehaviour2.GetFromScreenPositions();
                int[] warpCoord2ToModifier = dispositionsModifier[room2EntranceOrientation];

                // The current gate leads to the position of the next gate, with applied modifier
                gateBehaviour1.SetToScreenPositions(warpCoord2[0] + warpCoord2ToModifier[0], warpCoord2[1] + warpCoord2ToModifier[1], r + 2);
                gateBehaviour1.SetDestinationOrientation(room2EntranceOrientation);
                // The second gate leads to the position of the current gate with applied modifier
                gateBehaviour2.SetToScreenPositions(warpCoord1[0] + warpCoord1ToModifier[0], warpCoord1[1] + warpCoord1ToModifier[1], r + 1);
                gateBehaviour2.SetDestinationOrientation(room1EntranceOrientation);

                // Gates are appended to the maze's set (though this is not yet used)
                _gateWays.Add(gate1);
                _gateWays.Add(gate2);

            }
            else
            {
                // If last room, do nothing since it is already linked to the finishing room that was Set (previous method)
            }

        }
    }

    private void PlaceSetGateWays2(Room[] rooms)
    {

    }

    private void PlaceSecondaryGateWays(Room[] roomsFrom, Room[] roomsTo)
    {

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

    //private void CreateJson() // Loads all the scene and stores it in a JSON file
    //{
    //    Debug.Log("Creating json...");
    //    Debug.Log(Application.persistentDataPath);

    //    string path = Path.Combine(
    //        Application.persistentDataPath,
    //        levelName + ".json"
    //    );

    //    //if (File.Exists(path))
    //    //    return;

    //    string[] roomJsonArray = new string[_rooms.Length];
    //    for (int r = 0; r < _rooms.Length; r++) // Each room is stored separatedly
    //    {
    //        string roomJson = "";
    //        Cell[][] grid = _rooms[r].GetGrid();
    //        string[] cellsArray = new string[frameSize * frameSize]; // The cells are a one dimensional array, with a given frame height/width to read

    //        int count = 0;
    //        for (int y = 0; y < frameSize; y++) 
    //        {
    //            for (int x = 0; x < frameSize; x++)
    //            {
    //                Cell cell = grid[x][y];
    //                if (cell == null)
    //                {
    //                    string cellJson = "{\"status\": 0}"; // Status 0 means no part of the maze/No Cells
    //                    cellsArray[count] = cellJson;
    //                }
    //                else
    //                {
    //                    CellBehaviour cellBehaviour = cell.GetCellObject().GetComponent<CellBehaviour>();
    //                    int tileCode = cellBehaviour.code;
    //                    int elementCode = cellBehaviour.elementCode;
    //                    int elementOrientation = cellBehaviour.elementOrientation;

    //                    int nextGT = cell.nextToGatewayT ? 1 : 0;
    //                    int nextGB = cell.nextToGatewayB ? 1 : 0;
    //                    int nextGR = cell.nextToGatewayR ? 1 : 0;
    //                    int nextGL = cell.nextToGatewayL ? 1 : 0;
    //                    int[] nextToGatewayArray = { nextGT, nextGB, nextGR, nextGL };
    //                    string nextToGatewayStr = $"[{string.Join(",", nextToGatewayArray)}]";

    //                    string cellJson = "{\"status\": 1, " +
    //                                        "\"tile\": " + tileCode.ToString() + ", " +
    //                                        "\"elementCode\": " + elementCode.ToString() + ", " +
    //                                        "\"elementOrientation\": " + elementOrientation.ToString() + ", " +
    //                                        "\"nextGateway\":" + nextToGatewayStr + "}";
    //                    cellsArray[count] = cellJson;
    //                }
    //                count++;
    //            }
    //        }
    //        // Each room has cells and gateways
    //        GameObject[] gatewaysArray = _rooms[r].GetGateways().ToArray();
    //        string[] gatesJsonArray = new string[gatewaysArray.Length];
    //        for (int g = 0; g < gatewaysArray.Length; g++)
    //        {
    //            GameObject gateObject = gatewaysArray[g];
    //            GateWay gate = gateObject.GetComponent<GateWay>();
    //            string gateJson = "{\"coordsPosition\":" + $"[{string.Join(",", gate.GetFromScreenPositions())}], " + //Position is where they are placed
    //                                "\"coordsDestination\":" + $"[{string.Join(",", gate.GetToScreenPositions())}], " + //Destination where they lead
    //                                "\"toRoom\":" + gate.toRoomID + "}"; //The room they lead to

    //            gatesJsonArray[g] = gateJson;
    //        }

    //        roomJson =
    //                            $@"{{ 
    //                                ""room_number"": {r + 1},
    //                                ""cells"": [{string.Join(",", cellsArray)}],
    //                                ""gates"": [{string.Join(",", gatesJsonArray)}]
    //                              }}";
    //        roomJsonArray[r] = roomJson;
    //    }

    //    // General information: number of rooms, frame, distancing between frames, info on room Array
    //    string roomsJson = "[" + string.Join(",", roomJsonArray) + "]";
    //    string json = "{\"rooms_n\": " + roomNumber.ToString() + ", " +
    //                    "\"frame_size\": " + frameSize.ToString() + ", " +
    //                    "\"distancing\":" + _distancing.ToString() + ", " +
    //                    "\"rooms\": " + roomsJson + " }";

    //    File.WriteAllText(path, json);
    //    // After the json file is created, the game should go to the next scene and keep creating the rest of the maze
    //    // So it is all loaded by the time the player starts playing
    //} 
}
