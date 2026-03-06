using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SceneryElement : ElementBehaviour
{
    // This frame of reference helps read the array of sprites into the correct shape
    public int width;
    public int height;

    public bool doesBlock; // Ex: Pottery can be broken, so it doesn't matter if it blocks the way

    public GameObject[] sprites;
    public int[] codes;

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
        Cell[][] grid = room.GetGrid(); // Room's grid

        HashSet<Cell> allCells = room.GetAllCells();
        Cell[] arrayCells = allCells.ToArray();
        Room.Shuffle(arrayCells); // Randomized cells

        int[] rotation = { 0, 1, 2, 3 };
        Room.Shuffle(rotation); // Radomized order of orientations

        for (int i = 0; i < arrayCells.Length; i++)
        {
            GameObject[][] shape = CreateShape(); // The array of arrays is created
            //Debug.Log("ShapeIsFull?");
            //Debug.Log(ShapeIsFull(shape));

            for (int j = 0; j < 4; j++)
            {
                bool correctPlacement = true;
                GameObject[][] rotatedShape = CreateRotation(shape, rotation[j]);
                // The array of arrays is set on the orientation

                int selectedRotation = 0;
                if (rotation[j] == 1)
                {
                    selectedRotation = 90;
                }
                else if (rotation[j] == 2)
                {
                    selectedRotation = 180;
                }
                else if (rotation[j] == 3)
                {
                    selectedRotation = 270;
                }

                //Debug.Log("grid x " + grid.Length.ToString());
                //Debug.Log("grid y " + grid[0].Length.ToString());

                //Debug.Log("rotation" + rotation[j].ToString());
                //Debug.Log("angle" + selectedRotation.ToString());

                Cell cell = arrayCells[i];
                int[] frameCoords = cell.FrameCoordinates();
                int[] screenCoords = cell.ScreenCoordinates();

                //Debug.Log("random frameCoords:");
                //Debug.Log(frameCoords[0].ToString() + "-" + frameCoords[1].ToString());

                int currentWidth = rotatedShape.Length; // corresponding to the new shape's dimensions
                int currentHeight = rotatedShape[0].Length;

                for (int x = 0; x < currentWidth; x++)
                {
                    for (int y = 0; y < currentHeight; y++)
                    {
                        //Debug.Log("x" + x.ToString());
                        //Debug.Log("y" + y.ToString());

                        //Debug.Log("xframe" + frameCoords[0].ToString());
                        //Debug.Log("yframe" + frameCoords[1].ToString());

                        if (x + frameCoords[0] < grid.Length && y + frameCoords[1] < grid[0].Length)
                        {
                            if (grid[frameCoords[0] + x ][frameCoords[1] + y] != null && grid[frameCoords[0] + x][frameCoords[1] + y].Available())
                            { }
                            else
                            {
                                correctPlacement = false; // Not available
                                //Debug.Log("incorrect1");
                                break;
                            }
                        }
                        else
                        {
                            correctPlacement = false; // Too large in some direction
                            //Debug.Log("incorrect2");
                            break;
                        }
                    }
                    if (!correctPlacement) // If it doesn't fit, try again with new rotation (and then a new cell, if all fails)
                    {
                        break;
                    }
                }
                //Debug.Log(correctPlacement);

                if (correctPlacement) // It fits MISSING: CHECK IT DOESNT BLOCK IF SCENERY CANNOT BE WALKED OVER OR CAN BE BROKEN
                {
                    for (int x = 0; x < currentWidth; x++)
                    {
                        for (int y = 0; y < currentHeight; y++)
                        {
                            if (x + frameCoords[0] < grid.Length && y + frameCoords[1] < grid[0].Length)
                            {

                                GameObject valueShape = rotatedShape[x][y];
                                if (valueShape != null) 
                                {
                                    int index = System.Array.IndexOf(sprites, valueShape);
                                    int code = codes[index];

                                    grid[frameCoords[0] + x][frameCoords[1] + y].PlaceElement(code, selectedRotation);

                                    GameObject sceneryBlock = UnityEngine.Object.Instantiate(
                                        valueShape,
                                        new Vector2(screenCoords[0] + x, screenCoords[1] + y),
                                        Quaternion.Euler(0, 0, selectedRotation),
                                        room.GetRoom().transform
                                    );
                                }
                                
                            }
                        }
                    }
                    return;
                }
            }
        }
    }

    private GameObject[][] CreateRotation(GameObject[][] originalShape, int rotation) // rotates to the right
    {
        GameObject[][] shape = originalShape;
        for (int i = 0; i < rotation; i++)
        {
            // Proportions are switched with each rotation
            int oldWidth = shape.Length;
            int oldHeight = shape[0].Length;
            int width = oldHeight;
            int height = oldWidth;

            GameObject[][] newShape = new GameObject[width][];
            for (int j = 0; j < width; j++)
            {
                newShape[j] = new GameObject[height];
            }

            int currentWidth = 0;
            int currentHeight = oldHeight - 1;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newShape[x][y] = shape[currentWidth][currentHeight];
                    currentWidth++;
                    if (currentWidth == oldWidth)
                    {
                        currentWidth = 0;
                        currentHeight--;
                    }
                }
            }
            shape = newShape;
        }
        return shape;
    }

    private GameObject[][] CreateShape() // Uses the width and height to make a two dimensional array with the given shape
    {
        GameObject[][] shape = new GameObject[width][];
        for (int i = 0; i < width; i++)
        {
            shape[i] = new GameObject[height];
        }
        int currentWidth = 0;
        int currentHeight = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            shape[currentWidth][currentHeight] = sprites[i];
            currentWidth++;
            if (currentWidth == width)
            {
                currentWidth = 0;
                currentHeight++;
            }
        }
        return shape;

    }

    // This method is just for testing figures
    private bool ShapeIsFull(GameObject[][] shape)
    {
        for (int x = 0; x < shape.Length; x++)
        {
            for (int y = 0; y < shape[x].Length; y++)
            {
                if (shape[x][y] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

}
