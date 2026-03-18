using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class MazeSaving
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void CreateJson(int frameSize, Room[] _rooms, int _distancing, int roomNumber, string levelName, GateWay[] setGateWays) // Loads all the scene and stores it in a JSON file
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
                                    "\"toRoom\":" + gate.toRoomID + ", " +
                                    "\"orientation\": \"" + gate.orientation + "\"}";
                //The room they lead to
                //The orientation the player is set to when used

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

        // Set Gates
        string[] setGatesJsonArray = new string[setGateWays.Length];
        for (int g = 0; g < setGateWays.Length; g++)
        {
            GateWay gate = setGateWays[g];
            string setGate = "{\"coordsPosition\":" + $"[{string.Join(",", gate.GetFromScreenPositions())}], " +
                              "\"coordsDestination\":" + $"[{string.Join(",", gate.GetToScreenPositions())}], " +
                              "\"toRoom\":" + gate.toRoomID + ", " +
                              "\"orientation\": \"" + gate.orientation + "\"}";
            setGatesJsonArray[g] = setGate;
        }
        string setGatesJson = string.Join(",", setGatesJsonArray);

        // General information: number of rooms, frame, distancing between frames, info on room Array
        string roomsJson = "[" + string.Join(",", roomJsonArray) + "]";
        string json = "{\"rooms_n\": " + roomNumber.ToString() + ", " +
                        "\"frame_size\": " + frameSize.ToString() + ", " +
                        "\"distancing\":" + _distancing.ToString() + ", " +
                        "\"rooms\": " + roomsJson + "," +
                        "\"gates\": [" + setGatesJson + "]}";

        File.WriteAllText(path, json);
        // After the json file is created, the game should go to the next scene and keep creating the rest of the maze
        // So it is all loaded by the time the player starts playing
    }
}
