using UnityEngine;
using System.Collections.Generic;
using System.Linq;


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

    public string nextScene;

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

    // Main function
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

        //PlaceGateWays(_rooms, true); // Primary gateways are instantiated in all rooms
        PlaceAllBorders(); // Colliders are instantiated in all map
    }

    // Runs through a series of given rooms and connects them
    private void PlaceGateWays(Room[] rooms, bool primary = false) // Rooms are in order for primary gates, and in disorder for secondary gates
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

                    if (!primary && index > 3)
                    {   
                        notFound = true;
                        break;
                    }
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
                if (primary) // Primary gates in the last room take you to another Scene
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
        foreach (Room room in _rooms)
        {
            room.SetRoomBordersSprites();
        }
    }

}

class Room
{

    private Cell[][] _roomGrid; // The cell grid
    private bool _firstUnit = true; // The first box that makes up the room
    private int _frameSize; // The square size of the frame

    private GameObject _roomObject;
    public int _roomID;

    private static int IDcounter = 0;

    private HashSet<GameObject> _gateWays; // The gateways from only this room

    private HashSet<Cell> _takenSpace;

    private Dictionary<string, WallBehaviour> _walls; 
    // So as to not repeat walls and to set their neighbour relationships


    // Creates the main frame, adds celular units to make rooms with more interesting shapes
    public void CreateFrame()
    {
        _roomID = GetRoomID();
        _roomObject = new GameObject("Room" + _roomID);
        _frameSize = MazeManager.Instance.frameSize;

        int minBoxNumber = MazeManager.Instance.minBoxNumber;
        int maxBoxNumber = MazeManager.Instance.maxBoxNumber;

        int roomMinWidth = MazeManager.Instance.roomMinWidth;
        int roomMaxWidth = MazeManager.Instance.roomMinWidth;
        int roomMinHeigth = MazeManager.Instance.roomMinHeigth;
        int roomMaxHeigth = MazeManager.Instance.roomMaxHeigth;

        int roomCompression = MazeManager.Instance.compression; 

        // Grid is instantianted all null
        _roomGrid = new Cell[_frameSize][];

        for (int x = 0; x < _frameSize; x++)
        {
            _roomGrid[x] = new Cell[_frameSize];
        }

        // Number of rectangle boxes is set and then filled
        int boxNumber = Random.Range(minBoxNumber, maxBoxNumber + 1);
        for (int y = 0; y < boxNumber; y++)
        {
            // The width and height can be inverted when creating each box
            int width = Random.Range(roomMinWidth, roomMaxWidth + 1);
            int height = Random.Range(roomMinHeigth, roomMaxHeigth + 1);
            int orientation = Random.Range(0, 2);

            if (orientation == 0)
            {
                AddCelularUnit(width, height, roomCompression);
            }
            else if (orientation == 1)
            {
                AddCelularUnit(height, width, roomCompression);
            }
        }

        _gateWays = new HashSet<GameObject>();
        _walls = new Dictionary<string, WallBehaviour>();
        
    }

    // Adds rectangles to the main frame
    void AddCelularUnit(int width, int height, int roomCompression)
    {
        //Debug.Log("width cellular:" + width);
        //Debug.Log("height cellular:" + height);

        if (_firstUnit)
        {
            int middlePoint = _frameSize / 2; // Erases decimals, gets middle point of frame
            int halfHeight = height / 2;
            int halfWidth = width / 2;

            int startX = middlePoint - halfWidth;
            int startY = middlePoint - halfHeight;

            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    _roomGrid[x][y] = new Cell(_roomObject, x, y, _roomID);
                }
            }
            _firstUnit = false;
        }
        else
        {
            Cell randomCell = GetRandomCell();
            int[] coords = randomCell.FrameCoordinates();
            int xCell = coords[0];
            int yCell = coords[1];
            int newX = 0;
            int newY = 0;

            string[] frameDispositionOptions = { "top-left", "top-right", "bottom-right", "bottom-left" };
            int index = Random.Range(0, 4);
            string frameDisposition = frameDispositionOptions[index];

            
            if (frameDisposition == "top-left")
            {
                newX = xCell - width + roomCompression;
                newY = yCell + height - roomCompression;
            }
            else if (frameDisposition == "top-right")
            {
                newX = xCell + width - roomCompression;
                newY = yCell + height - roomCompression;
            }
            else if (frameDisposition == "bottom-right")
            {
                newX = xCell + width - roomCompression;
                newY = yCell - height + roomCompression;
            }
            else if (frameDisposition == "bottom-left")
            {
                newX = xCell - width + roomCompression;
                newY = yCell - height + roomCompression;
            }

            if (newX < 0)
            {
                newX = 0;
            }
            if ((newX + width) > _roomGrid.Length)
            {
                newX = _roomGrid.Length - width;
            }
            if (newY < 0)
            {
                newY = 0;
            }
            if ((newY + height) > _roomGrid[0].Length)
            {
                newY = _roomGrid.Length - height;
            }

            for (int x = newX; x < newX + width; x++)
            {
                for (int y = newY; y < newY + height; y++)
                {
                    //Debug.Log("x:" + x);
                    //Debug.Log("y:" + y);
                    _roomGrid[x][y] = new Cell(_roomObject, x, y, _roomID);
                }
            }

        }

    }

    // All rooms have a number
    int GetRoomID()
    {
        Room.IDcounter = Room.IDcounter + 1;
        return IDcounter;
    }

    // Gets cells in room grid
    HashSet<Cell> GetAllCells()
    {
        HashSet<Cell> allCells = new HashSet<Cell>();
        for (int x = 0; x < _frameSize; x++)
        {
            for (int y = 0; y < _frameSize; y++)
            {
                Cell cell = _roomGrid[x][y];
                if (cell != null)
                {
                    allCells.Add(cell);
                }
            }
        }
        return allCells;
    }

    // Returns cells with a given orientation
    public HashSet<Cell> GetAllBorders(string orientation) // Orientation means: Right -> to the right there is nothing
    {
        HashSet<Cell> allCells = GetAllCells();
        HashSet<Cell> borderCells = new HashSet<Cell>();

        foreach (Cell cell in allCells)
        {
            // Next variables are the cell right next to the door, and should not have borders
            if (cell.nextToGatewayT && orientation == "T")
            {
                continue;
            }
            if (cell.nextToGatewayB && orientation == "B")
            {
                continue;
            }
            if (cell.nextToGatewayR && orientation == "R")
            {
                continue;
            }
            if (cell.nextToGatewayL && orientation == "L")
            {
                continue;
            }

            int[] coord = cell.FrameCoordinates();
            int x = coord[0];
            int y = coord[1];

            bool rightBorder = false;
            if (x == _frameSize - 1)
            {
                rightBorder = true;
            }
            else
            {
                Cell rightCell = _roomGrid[x + 1][y];
                if (rightCell == null)
                {
                    rightBorder = true;
                }
            }

            bool leftBorder = false;
            if (x == 0)
            {
                leftBorder = true;
            }
            else
            {
                Cell leftCell = _roomGrid[x - 1][y];
                if (leftCell == null)
                {
                    leftBorder = true;
                }
            }

            bool bottomBorder = false;
            if (y == 0)
            {
                bottomBorder = true;
            }
            else
            {
                Cell bottomCell = _roomGrid[x][y - 1];
                if (bottomCell == null)
                {
                    bottomBorder = true;
                }
            }

            bool topBorder = false;
            if (y == _frameSize - 1) 
            {
                topBorder = true;
            }
            else
            {
                Cell topCell = _roomGrid[x][y + 1];
                if (topCell == null)
                {
                    topBorder = true;
                }
            }

            if (rightBorder && orientation == "R")
            {
                borderCells.Add(cell);
            }
            else if (leftBorder && orientation == "L")
            {
                borderCells.Add(cell);
            }
            else if (topBorder && orientation == "T")
            {
                borderCells.Add(cell);
            }
            else if (bottomBorder && orientation == "B")
            {
                borderCells.Add(cell);
            }
            
            if ((rightBorder || leftBorder || topBorder || bottomBorder) && orientation == null)
            {
                borderCells.Add(cell);
            }
        }
        return borderCells;
    }

    // Returns the borders where a gate is viable
    public HashSet<Cell> GetAllGateBorders(string orientation)
    {
        HashSet<Cell> validBorders = new HashSet<Cell>();

        return validBorders;
    }

    // Random Cell in a set or in the whole grid
    public Cell GetRandomCell(HashSet<Cell> set = null)
    {
        Cell randomCell = null;
        if (set == null)
        {
            HashSet<Cell> allCells = GetAllCells();
            int index = Random.Range(0, allCells.Count);
            randomCell = allCells.ElementAt(index);
        }
        else
        {
            int index = Random.Range(0, set.Count);
            randomCell = set.ElementAt(index);
        }
        
        return randomCell;
    }

    // All cells get their Sprite and are given coordinates
    public void PaintAllCells(int x, int y)
    {
        HashSet<Cell> allCells = GetAllCells();
        foreach (Cell cell in allCells)
        {
            int[] coords = cell.FrameCoordinates();
            int xIndex = coords[0];
            int yIndex = coords[1];
            cell.InitializeCell(x + xIndex, y + yIndex);
        }
    }

    // Add Gate to a room
    // Should set nextTo and neighbouring cells
    public void AddGate(GameObject gateway, int frameX, int frameY, string orientation)
    {
        _gateWays.Add(gateway);
        if (orientation == "T")
        {
            _roomGrid[frameX][frameY].nextToGatewayT = true;
        }
        else if (orientation == "B")
        {
            _roomGrid[frameX][frameY].nextToGatewayB = true;
        }
        else if (orientation == "R")
        {
            _roomGrid[frameX][frameY].nextToGatewayR = true;
        }
        else if (orientation == "L")
        {
            _roomGrid[frameX][frameY].nextToGatewayL = true;
        }
            
    }
    
    // Gateway is created and partially initialized
    public GameObject SetRoomGates(Cell cell, string orientation)
    {
        int[] coords = cell.ScreenCoordinates();
        int xindex = coords[0];
        int yindex = coords[1];

        int _xposition = 0;
        int _yposition = 0;

        if (orientation == "T")
        {
            _xposition = xindex;
            _yposition = yindex + 1;
        }
        else if (orientation == "B")
        {
            _xposition = xindex;
            _yposition = yindex - 1;
        }
        else if (orientation == "R")
        {
            _xposition = xindex + 1;
            _yposition = yindex;
        }
        else if (orientation == "L")
        {
            _xposition = xindex - 1;
            _yposition = yindex;
        }

        GameObject gateObject = UnityEngine.Object.Instantiate(
            MazeManager.Instance.warpPrefab,
            new Vector2(_xposition, _yposition),
            Quaternion.identity,
            _roomObject.transform
        );

        GateWay gateBehaviour = gateObject.GetComponent<GateWay>();

        int[] frameCoords = cell.FrameCoordinates();
        gateBehaviour.SetFramePositions(frameCoords[0], frameCoords[1]); // frame border
        AddGate(gateObject, frameCoords[0], frameCoords[1], orientation);

        gateBehaviour.SetFromScreenPositions(_xposition, _yposition, _roomID); // sceenposition of warp, outside used frame

        return gateObject;
    }

    // Begins setting _takenSpace
    private void SetPath()
    {

    }

    // Once all the walls have their bools set, the correct sprite is set
    public void SetRoomBordersSprites()
    {

    }

    // Room borders and corners are created in _walls
    public void SetRoomBorders()
    {
        string[] orientations = { "T", "B", "R", "L" };

        // Normal borders
        foreach (string orientation in orientations)
        {

            HashSet<Cell> cells = GetAllBorders(orientation);

            foreach (Cell cell in cells)
            {

                int[] coords = cell.ScreenCoordinates();
                int xIndex = coords[0];
                int yIndex = coords[1];

                int[] frameCoords = cell.FrameCoordinates();
                int xFrame = frameCoords[0];
                int yFrame = frameCoords[1];

                int _xPosition = 0;
                int _yPosition = 0;

                if (orientation == "T")
                {
                    _xPosition = xIndex;
                    _yPosition = yIndex + 1;
                    yFrame++;
                }
                else if (orientation == "B")
                {
                    _xPosition = xIndex;
                    _yPosition = yIndex - 1;
                    yFrame--;
                }
                else if (orientation == "R")
                {
                    _xPosition = xIndex + 1;
                    _yPosition = yIndex;
                    xFrame++;
                }
                else if (orientation == "L")
                {
                    _xPosition = xIndex - 1;
                    _yPosition = yIndex;
                    xFrame--;
                }
                AddBorder(_xPosition, _yPosition, xFrame, yFrame);
            }
        }

        // Corners borders
        HashSet<Cell> allCells = GetAllCells();
        foreach (Cell cell in allCells)
        {
            fixCorner(cell);
        }

        foreach (KeyValuePair<string, WallBehaviour> entry in _walls)
        {
            WallBehaviour wall = entry.Value;
            int x = wall.frameX;
            int y = wall.frameY;
            
            // Neighbouring walls
            if (_walls.ContainsKey((x + 1) + "/" + (y)))
            {
                wall.right = true;
            }
            if (_walls.ContainsKey((x - 1) + "/" + (y)))
            {
                wall.left = true;
            }
            if (_walls.ContainsKey((x) + "/" + (y + 1)))
            {
                wall.top = true;
            }
            if (_walls.ContainsKey((x) + "/" + (y - 1)))
            {
                wall.bottom = true;
            }

            // Neighbouring cells
            // Right cell
            if (x + 1 < _frameSize && y < _frameSize)
            {
                if (x + 1 >= 0 && y >= 0)
                {
                    if (_roomGrid[x + 1][y] != null)
                    {
                        wall.rightCell = true;
                    }
                }
            }
            // Left cell
            if (x - 1 < _frameSize && y < _frameSize)
            {
                if (x - 1 >= 0 && y >= 0)
                {
                    if (_roomGrid[x - 1][y] != null)
                    {
                        wall.leftCell = true;
                    }
                }
            }
            // Top cell
            if (x < _frameSize && y + 1 < _frameSize)
            {
                if (x >= 0 && y + 1 >= 0)
                {
                    if (_roomGrid[x][y + 1] != null)
                    {
                        wall.topCell = true;
                    }
                }
            }
            // Bottom cell
            if (x < _frameSize && y - 1 < _frameSize)
            {
                if (x >= 0 && y - 1 >= 0)
                {
                    if (_roomGrid[x][y - 1] != null)
                    {
                        wall.bottomCell = true;
                    }
                }
            }

            wall.LoadSprite();
        }

    }

    // Sets it so a border is not added twice
    private void AddBorder(int  screenX, int screenY, int x, int y)
    {
        string key = x + "/" + y;
        if (!_walls.ContainsKey(key))
        {
            GameObject gateObject = UnityEngine.Object.Instantiate(
                    MazeManager.Instance.wallPrefab,
                    new Vector2(screenX, screenY),
                    Quaternion.identity,
                    _roomObject.transform
                );
            WallBehaviour wall = gateObject.GetComponent<WallBehaviour>();
            wall.LoadFrame(x, y);
            _walls[key] = wall;
        }
    }

    // Recognizes borders to find corners
    private bool[] findCorners(int frameX, int frameY)
    {
        bool rightBorder = false;
        bool leftBorder = false;
        bool bottomBorder = false;
        bool topBorder = false;

        if (frameX == 0)
        {
            leftBorder = true;
        }
        else if (_roomGrid[frameX - 1][frameY] == null)
        {
            leftBorder = true;
        }

        if (frameX == _frameSize - 1)
        {
            rightBorder = true;
        }
        else if (_roomGrid[frameX + 1][frameY] == null)
        {
            rightBorder = true;
        }

        if (frameY == 0)
        {
            bottomBorder = true;
        }
        else if (_roomGrid[frameX][frameY + -1] == null)
        {
            bottomBorder = true;
        }

        if (frameY == _frameSize - 1)
        {
            topBorder = true;
        }
        else if (_roomGrid[frameX][frameY + 1] == null)
        {
            topBorder = true;
        }

        return new bool[] { topBorder, bottomBorder, rightBorder, leftBorder };
    }

    private string IsInnerCorner(int frameX, int frameY, string orientation)
    {
        Cell cell = _roomGrid[frameX][frameY];
        if (orientation == "T")
        {

        }
        else if (orientation == "B")
        {

        }
        else if (orientation == "R")
        {

        }
        else if (orientation == "L")
        {

        }
        return null;
    }

    // Finds the walls corners and adds the to _walls
    private void fixCorner(Cell cell)
    {
        int[] coords = cell.FrameCoordinates();
        int frameX = coords[0];
        int frameY = coords[1];

        bool[] corners = findCorners(frameX, frameY);
        bool topBorder = corners[0];
        bool bottomBorder = corners[1];
        bool rightBorder = corners[2];
        bool leftBorder = corners[3];

        int[] screenCoords = cell.ScreenCoordinates();
        int screenX = screenCoords[0];
        int screenY = screenCoords[1];

        // Corners are recognized and created
        if (topBorder && rightBorder)
        {
            AddBorder(screenX + 1, screenY + 1, frameX + 1, frameY + 1);
        }
        if (topBorder && leftBorder)
        {
            AddBorder(screenX - 1, screenY + 1, frameX - 1, frameY + 1);
        }
        if (bottomBorder && rightBorder)
        {
            AddBorder(screenX + 1, screenY - 1, frameX + 1, frameY - 1);
        } 
        if (bottomBorder && leftBorder)
        {
            AddBorder(screenX - 1, screenY - 1, frameX - 1, frameY - 1);
        }

    }

    private void MarkAsGatewaySpace(int frameX, int frameY, string orientation)
    {
        //Cell cell = _roomGrid[frameX][frameY];
    } 
}

class Cell
{
    private int _xPosition; // Position according to map
    private int _yPosition;
    private int _frameX; // Position according to frame array
    private int _frameY;
    private GameObject _gameObjectParent; // Room: empty 
    private GameObject _cellObject; // Cell object associated
    private int _cellID; // current cell
    private int _roomID; // room parent

    public bool nextToGatewayT = false;
    public bool nextToGatewayB = false;
    public bool nextToGatewayR = false;
    public bool nextToGatewayL = false;

    private static int IDcounter = 0;

    public Cell(GameObject roomObject, int x, int y, int roomID)
    {
        _gameObjectParent = roomObject;
        _frameX = x;
        _frameY = y;
        _roomID = roomID;
        _cellID = GetCellId();
    }

    public int[] FrameCoordinates()
    {
        return new int[] { _frameX, _frameY };

    }

    public int[] ScreenCoordinates()
    {
        return new int[] { _xPosition, _yPosition };

    }

    public void InitializeCell(int x, int y)
    {
        _xPosition = x;
        _yPosition = y;
        _cellObject = UnityEngine.Object.Instantiate(
            MazeManager.Instance.cellTypePrefab,
            new Vector2(_xPosition, _yPosition),
            Quaternion.identity,
            _gameObjectParent.transform
        );
        CellBehaviour cellBehaviour = _cellObject.GetComponent<CellBehaviour>();
        cellBehaviour.InitializeCell(_cellID);
        cellBehaviour.LoadFloorSprite();

    }

    private int GetCellId()
    {
        Cell.IDcounter = Cell.IDcounter + 1;
        return IDcounter;
    }

}