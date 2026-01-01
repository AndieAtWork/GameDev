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

    void CreateMap()
    {
        _rooms = new Room[roomNumber];
        int xIndex = 0;
        int yIndex = 0;
        for (int i = 0; i < roomNumber; i++)
        {
            _rooms[i] = new Room();
            _rooms[i].CreateFrame();
            _rooms[i].PaintAllCells(xIndex, yIndex);

            xIndex = xIndex + frameSize + _distancing; // Se van a imprimir en fila por el eje x
        }
        PlaceGateWays();
        PlaceAllBorders();
    }

    public void PlaceGateWays()
    {
        _gateWays = new HashSet<GameObject>();

        string[] gateDispositionOptions = { "T", "R", "B", "L" };
        string[] gateDispositionResversed = { "B", "L", "T", "R" };

        int[][] dispositionModifier = new int[][]
        {
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 0, -1 },
            new int[] { -1, 0 }
        };

        int[][] dispositionModifierReversed = new int[][]
        {
            new int[] { 0, -1 },
            new int[] { -1, 0 },
            new int[] { 0, 1 },
            new int[] { 1, 0 }
        };

        for (int i = 0; i < roomNumber; i++)
        {
            int index = Random.Range(0, 4);
            string gateDispositionEnter = gateDispositionOptions[index];
            string gateDispositionExit = gateDispositionResversed[index];
            
            Room currentRoom = _rooms[i];
            GameObject gate1 = currentRoom.SetRoomGates(gateDispositionEnter);
            GateWay gateBehaviour1 = gate1.GetComponent<GateWay>();
            int[] warpCoord1 = gateBehaviour1.GetFromScreenPositions();

            if (i < roomNumber - 1)
            {
                Room nextRoom = _rooms[i + 1];
                GameObject gate2 = nextRoom.SetRoomGates(gateDispositionExit);
                GateWay gateBehaviour2 = gate2.GetComponent<GateWay>();
                int[] warpCoord2 = gateBehaviour2.GetFromScreenPositions();
                //Debug.Log("nextRoom " + nextRoom._roomID.ToString());
                //Debug.Log("coords " + string.Join(", ", warpCoord2));
                int[] warpCoord2ToModifier = dispositionModifier[index];
                //Debug.Log("modifierTo " + string.Join(", ", warpCoord2ToModifier));

                int[] warpCoord1ToModifier = dispositionModifierReversed[index];

                //Debug.Log("currentRoom " + currentRoom._roomID.ToString());
                //Debug.Log("coords " + string.Join(", ", warpCoord1));
                //Debug.Log("modifierTo " + string.Join(", ", warpCoord1ToModifier));

                gateBehaviour1.SetToScreenPositions(warpCoord2[0] + warpCoord2ToModifier[0], warpCoord2[1] + warpCoord2ToModifier[1], i + 2);
                gateBehaviour2.SetToScreenPositions(warpCoord1[0] + warpCoord1ToModifier[0], warpCoord1[1] + warpCoord1ToModifier[1], i + 1);
            }
            else
            {
                gateBehaviour1.SetToScreenPositions(0, 0, 0, nextScene);
            }

        }

        //GateWay gateBehaviour = gateObject.GetComponent<GateWay>();
    }

    public void PlaceAllBorders()
    {
        foreach (Room room in _rooms)
        {
            room.SetRoomBorders();
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

    }

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


            //Debug.Log("x new:" + newX);
            //Debug.Log("y new:" + newY);

            //Debug.Log("x total:" + (newX + width));
            //Debug.Log("y total:" + (newY + height));

            //Debug.Log("_roomGrid" + _roomGrid.Length + " - " + _roomGrid[0].Length);

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

    int GetRoomID()
    {
        Room.IDcounter = Room.IDcounter + 1;
        return IDcounter;
    }

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

    HashSet<Cell> GetAllBorders(string orientation=null) // Orientation means: Right -> to the right there is nothing
    {
        HashSet<Cell> allCells = GetAllCells();
        HashSet<Cell> borderCells = new HashSet<Cell>();

        foreach (Cell cell in allCells)
        {
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

    public GameObject SetRoomGates(string orientation)
    {
        HashSet<Cell> cells = GetAllBorders(orientation);
        Cell cell = GetRandomCell(cells);

        int[] frameCoords = cell.FrameCoordinates();

        int[] coords = cell.ScreenCoordinates();
        int xIndex = coords[0];
        int yIndex = coords[1];

        int _xPosition = 0;
        int _yPosition = 0;

        if (orientation == "T")
        {
            _xPosition = xIndex;
            _yPosition = yIndex + 1;
        }
        else if (orientation == "B")
        {
            _xPosition = xIndex;
            _yPosition = yIndex - 1;
        }
        else if (orientation == "R")
        {
            _xPosition = xIndex + 1;
            _yPosition = yIndex;
        }
        else if (orientation == "L")
        {
            _xPosition = xIndex - 1;
            _yPosition = yIndex;
        }

        GameObject gateObject = UnityEngine.Object.Instantiate(
            MazeManager.Instance.warpPrefab,
            new Vector2(_xPosition, _yPosition),
            Quaternion.identity,
            _roomObject.transform
        );

        GateWay gateBehaviour = gateObject.GetComponent<GateWay>();
        gateBehaviour.SetFramePositions(frameCoords[0], frameCoords[1]); // Frame border
        AddGate(gateObject, frameCoords[0], frameCoords[1], orientation);

        gateBehaviour.SetFromScreenPositions(_xPosition, _yPosition, _roomID); // SceenPosition of warp, outside used frame

        return gateObject;

    }

    public void SetSecondaryGates()
    {

    }

    public void SetRoomBorders()
    {
        string[] orientations = { "T", "B", "R", "L" };

        foreach (string orientation in orientations)
        {

            HashSet<Cell> cells = GetAllBorders(orientation);

            foreach (Cell cell in cells)
            {

                int[] coords = cell.ScreenCoordinates();
                int xIndex = coords[0];
                int yIndex = coords[1];

                int _xPosition = 0;
                int _yPosition = 0;

                if (orientation == "T")
                {
                    _xPosition = xIndex;
                    _yPosition = yIndex + 1;
                }
                else if (orientation == "B")
                {
                    _xPosition = xIndex;
                    _yPosition = yIndex - 1;
                }
                else if (orientation == "R")
                {
                    _xPosition = xIndex + 1;
                    _yPosition = yIndex;
                }
                else if (orientation == "L")
                {
                    _xPosition = xIndex - 1;
                    _yPosition = yIndex;
                }

                GameObject gateObject = UnityEngine.Object.Instantiate(
                    MazeManager.Instance.wallPrefab,
                    new Vector2(_xPosition, _yPosition),
                    Quaternion.identity,
                    _roomObject.transform
                );
            }
        }
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