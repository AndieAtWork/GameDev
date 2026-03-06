using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class InnerWall : Barrier
{
    public GameObject wallPrefab;
    public GameObject closedWallPrefab;

    public int[] codes;
    public int minWidthPassage;
    public int maxWidthPassage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void PlaceObject(Room room)
    {
        GameObject mazeObject = MazeManager.Instance.gameObject; // Only mazeManager in the scene, (Singleton)
        BarrierDisposition disposition = mazeObject.GetComponent<BarrierDisposition>();
        int passageMinWidth = disposition.passageMinWidth;
        int passageMaxWidth = disposition.passageMaxWidth;

        int distanceFromWalls = disposition.distanceFromWalls; // How much space do the walls need on each side

        string[] orientations = { "L", "B" }; // There are only two orientations Vertical and Horizontal
        Room.Shuffle(orientations); // The first one to be tested is random

        Cell[][] grid = room.GetGrid(); // Room's grid
        bool objectPlaced = false; // Check if the try was successful

        for (int i = 0; i < 2; i++) // both orientations are tested
        {
            string orientation = orientations[i];
            Cell[] borderCells = room.GetAllBorders(orientation).ToArray();
            Room.Shuffle(borderCells); // All vertical or horizontal border cells, in disorder

            for (int c = 0; c < borderCells.Length; c++) // Each cell is tested
            {
                Cell cell = borderCells[c];
                int[] frameCoords = cell.FrameCoordinates(); // To check in grid
                int[] screenCoords = cell.ScreenCoordinates(); // To place on screen
                int barrierLength = 0;

                if (orientation == "L") // Horizontal Barrier
                {
                    bool correctPlacement = true;

                    if (frameCoords[1] == 0 || frameCoords[1] == grid[0].Length - 1)
                    {
                        // If it's a border, it can't have a wall
                        continue;
                    }

                    // Checks for max length of possible barrier. Must be matched with the row over and below it
                    for (int coordX = frameCoords[0]; coordX < grid[0].Length; coordX++)
                    {
                        if (grid[coordX][frameCoords[1]] != null)
                        {
                            if (grid[coordX][frameCoords[1]].PresentsGateway()) // A wall cannot hit a gateway's entrance
                            {
                                correctPlacement = false;
                                break;
                            }
                            if (grid[coordX][frameCoords[1]].Available())
                            {
                                // A cell is available only when it's not next to a gate and has no elementCode assigned to it
                                barrierLength++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (barrierLength < 2)
                    {
                        continue;
                    }

                    // SPACE: given horizontal inner wall, space moves vertically to ensure there is a distanceFromWalls
                    // Starts in -distance and reaches +distance
                    for (int space = 0 - distanceFromWalls; space < distanceFromWalls + 1; space++)
                    {
                        // Checks above and bellow
                        for (int coordX = frameCoords[0]; coordX < frameCoords[0] + barrierLength; coordX++)
                        {
                            // Position does not ensure the set space, thus incorrect placement
                            if (frameCoords[1] + space < 0 || frameCoords[1] + space > grid[0].Length - 1)
                            {
                                correctPlacement = false;
                                break;
                            }
                            else // Position is still valid
                            {
                                if (space == 0)
                                {
                                    // Was already checked to get barrierLength
                                }
                                else if (space == -1 || space == 1) // Right above and bellow, Available function
                                {
                                    if (grid[coordX][frameCoords[1] + space] != null && grid[coordX][frameCoords[1] + space].Available()) { }
                                    else
                                    {
                                        correctPlacement = false;
                                        break;
                                    }
                                }
                                else // Further than one cell in distance, AvailableForBarrier function
                                {
                                    if (grid[coordX][frameCoords[1] + space] != null && grid[coordX][frameCoords[1] + space].AvailableForBarrier(orientation)) { }
                                    else
                                    {
                                        correctPlacement = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (!correctPlacement)
                    {
                        continue;
                    }

                    // A passage without a barrier placed on it. Passage is the starting point, getPassageWidth adjusts
                    int passage = Random.Range(0, barrierLength);
                    HashSet<int> wholePassage = GetPassageWidth(barrierLength, passage, passageMinWidth, passageMaxWidth);
                    int beforePassage = wholePassage.Min() - 1;
                    int afterPassage = wholePassage.Max() + 1;

                    // Each block is placed in order
                    for (int block = 0; block < barrierLength; block++)
                    {
                        if (wholePassage.Contains(block))
                        {
                            continue;
                        }

                        GameObject prefab = wallPrefab;
                        Quaternion rotation = Quaternion.identity;
                        int objectCode = codes[0];
                        int objectOrientation = 0;
                        if (block == beforePassage)
                        {
                            prefab = closedWallPrefab;
                            objectCode = codes[1];
                            // default rotation 0
                            // default objectOrientation 0
                        }
                        else if (block == afterPassage)
                        {
                            prefab = closedWallPrefab;
                            rotation = Quaternion.Euler(0, 0, 180);
                            objectCode = codes[1];
                            objectOrientation = 180;
                        }
                        GameObject innerWallObject = UnityEngine.Object.Instantiate(
                            prefab,
                            new Vector2(screenCoords[0] + block, screenCoords[1]),
                            rotation, // Rotatio so it is horizontal
                            room.GetRoom().transform
                        );
                        grid[frameCoords[0] + block][frameCoords[1]].PlaceElement(objectCode, objectOrientation);
                    }
                    if (barrierLength > 1)
                    {
                        objectPlaced = true;
                        break;
                    }
                }
                else if (orientation == "B") // Vertical Barrier
                {
                    bool correctPlacement = true;

                    if (frameCoords[0] == 0 || frameCoords[0] == grid[0].Length - 1)
                    {
                        // If it's a border, it can't have a wall
                        continue;
                    }

                    // Checks for max length of possible barrier. Must be matched with the row over and below it
                    for (int coordY = frameCoords[1]; coordY < grid[0].Length; coordY++)
                    {
                        if (grid[frameCoords[0]][coordY] != null)
                        {
                            if (grid[frameCoords[0]][coordY].PresentsGateway()) // A wall cannot hit a gateway's entrance
                            {
                                correctPlacement = false;
                                break;
                            }
                            if (grid[frameCoords[0]][coordY].Available())
                            {
                                // A cell is available only when it's not next to a gate and has no elementCode assigned to it
                                barrierLength++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (barrierLength < 2)
                    {
                        continue;
                    }

                    for (int space = 0 - distanceFromWalls; space < distanceFromWalls + 1; space++)
                    {
                        // Checks right and bellow
                        for (int coordY = frameCoords[1]; coordY < frameCoords[1] + barrierLength; coordY++)
                        {
                            // Position does not ensure the set space, thus incorrect placement
                            if (frameCoords[0] + space < 0 || frameCoords[0] + space > grid[0].Length - 1)
                            {
                                correctPlacement = false;
                                break;
                            }
                            else // Position is still valid
                            {
                                if (space == 0)
                                {
                                    // Was already checked to get barrierLength
                                }
                                else if (space == -1 || space == 1) // Left and right, Available function
                                {
                                    if (grid[frameCoords[0] + space][coordY] != null && grid[frameCoords[0] + space][coordY].Available()) { }
                                    else
                                    {
                                        correctPlacement = false;
                                        break;
                                    }
                                }
                                else // Further than one cell in distance, AvailableForBarrier function
                                {
                                    if (grid[frameCoords[0] + space][coordY] != null && grid[frameCoords[0] + space][coordY].AvailableForBarrier(orientation)) { }
                                    else
                                    {
                                        correctPlacement = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (!correctPlacement)
                    {
                        continue;
                    }

                    // A passage without a barrier placed on it. Passage is the starting point, getPassageWidth adjusts
                    int passage = Random.Range(0, barrierLength);
                    HashSet<int> wholePassage = GetPassageWidth(barrierLength, passage, passageMinWidth, passageMaxWidth);
                    int beforePassage = wholePassage.Min() - 1;
                    int afterPassage = wholePassage.Max() + 1;

                    // Each block is placed in order
                    for (int block = 0; block < barrierLength; block++)
                    {
                        if (wholePassage.Contains(block))
                        {
                            continue;
                        }

                        GameObject prefab = wallPrefab;
                        Quaternion rotation = Quaternion.Euler(0, 0, 90);
                        int objectCode = codes[0];
                        int objectOrientation = 90;
                        if (block == beforePassage)
                        {
                            prefab = closedWallPrefab;
                            rotation = Quaternion.Euler(0, 0, 90);
                            objectCode = codes[1];
                            objectOrientation = 90;
                        }
                        else if (block == afterPassage)
                        {
                            prefab = closedWallPrefab;
                            rotation = Quaternion.Euler(0, 0, 270);
                            objectCode = codes[1];
                            objectOrientation = 270;
                        }
                        GameObject innerWallObject = UnityEngine.Object.Instantiate(
                            prefab,
                            new Vector2(screenCoords[0], screenCoords[1] + block),
                            rotation, // Rotation so it is vertical
                            room.GetRoom().transform
                        );
                        grid[frameCoords[0]][frameCoords[1] + block].PlaceElement(objectCode, objectOrientation);
                    }
                    if (barrierLength > 1)
                    {
                        objectPlaced = true;
                        break;
                    }
                }
            }

            if (objectPlaced)
            {
                break;
            }
        }
    }
}
