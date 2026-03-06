using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

public class PrinterBehaviour : MonoBehaviour
{
    public static PrinterBehaviour Instance { get; private set; }

    public string levelFilename; // filename of the json to be loaded

    // All basic construction prefabs
    public GameObject wallPrefab;
    public GameObject tilesPrefab;
    public GameObject warpPrefab;

    // Each SQUARE GameObject with its code.
    // If an element takes two cells, there are two codes and two go's
    public int[] codes;
    public GameObject[] gos;

    // Rooms are kept as in the maze behaviour
    private Room[] _rooms;

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
        Read();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Read()
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            levelFilename
        );

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Debug.Log("JSON cargado...");

            JObject obj = JObject.Parse(json);

            int roomsCount = (int)obj["rooms_n"];
            int frame = (int)obj["frame_size"];
            int distancing = (int)obj["distancing"];

            _rooms = new Room[roomsCount];

            JArray roomsArray = (JArray)obj["rooms"];
            int count = 0;
            foreach (JToken room in roomsArray)
            {
                int roomNumber = (int)room["room_number"];

                int horizontalMovement = distancing * (roomNumber - 1) + frame * (roomNumber - 1);
                // All Rooms are placed to the right of the one before
                // The grid starts (a frame + a distance) away from the following one 

                GameObject rObject = new GameObject("Room" + roomNumber.ToString()); // Room object is created to contain everything
                Room r = new Room(); // The class instance Room is created to store information: namely cell grid and gateways
                r.SetGrid(frame, roomNumber, rObject); // The room and all it's info are initiated
                _rooms[count] = r; // The room class instance is added to the array 

                JArray cellsArray = (JArray)room["cells"];
                int xPosition = 0;
                int yPosition = 0;
                foreach (JToken cellJson in cellsArray)
                {
                    int status = (int)cellJson["status"];
                    if (status == 1) // If the cell is being used in the grid
                    {
                        int tile = (int)cellJson["tile"];
                        int elementCode = (int)cellJson["elementCode"];
                        int elementOrientation = (int)cellJson["elementOrientation"];

                        JArray gatesJArray = (JArray)cellJson["nextGateway"];
                        int[] neighbouringGates = gatesJArray.Select(g => (int)g).ToArray();

                        Cell cell = new Cell(rObject, xPosition, yPosition, roomNumber); // Cell instance is created with frame positions and a given room
                        cell.InitializeCell(xPosition + horizontalMovement, yPosition, true, tile, tilesPrefab); // Creation of the GameObject
                        r.SetCell(cell, xPosition, yPosition); // Cell is added to the instance room's grid
                        
                        // Cell descriptors are set
                        cell.nextToGatewayT = neighbouringGates[0] == 1;
                        cell.nextToGatewayB = neighbouringGates[1] == 1;
                        cell.nextToGatewayR = neighbouringGates[2] == 1;
                        cell.nextToGatewayL = neighbouringGates[3] == 1;

                        if (elementCode != 0) // If there are elements in that cell
                        {
                            int index = System.Array.IndexOf(codes, elementCode);
                            GameObject goPrefab = gos[index];

                            GameObject elementObject = UnityEngine.Object.Instantiate(
                                goPrefab,
                                new Vector2(xPosition + horizontalMovement, yPosition),
                                Quaternion.Euler(0, 0, elementOrientation),
                                rObject.transform
                            );
                        }
                    }

                    xPosition++;
                    if (xPosition == frame) // Building the grid
                    {
                        xPosition = 0;
                        yPosition++;
                    }
                }

                r.SetRoomBorders(); // Once all the rooms are set, the barriers are placed.

                // Next step is placing the GateWay objects
                JArray gatesArray = (JArray)room["gates"];

                foreach (JToken gateJson in gatesArray)
                {
                    JArray coordsJArray = (JArray)gateJson["coordsPosition"]; // Where they are placed
                    int[] coordsPosition = coordsJArray.Select(g => (int)g).ToArray();

                    JArray destinationJArray = (JArray)gateJson["coordsDestination"]; // Where they lead
                    int[] coordsDestination = destinationJArray.Select(g => (int)g).ToArray();

                    int toRoom = (int)gateJson["toRoom"];

                    GameObject gateObject = UnityEngine.Object.Instantiate(
                        warpPrefab,
                        new Vector2(coordsPosition[0], coordsPosition[1]),
                        Quaternion.identity,
                        rObject.transform
                    );
                    GateWay gateBehaviour = gateObject.GetComponent<GateWay>();
                    gateBehaviour.SetToScreenPositions(coordsDestination[0], coordsDestination[1], toRoom); // Only absolute positions are being used currently
                    r.AddGatePrinter(gateObject); // Rooms know their gates
                }
                count++;
            }
        }
        else
        {
            Debug.LogWarning("Archivo no encontrado: " + path);
        }
    }
}
