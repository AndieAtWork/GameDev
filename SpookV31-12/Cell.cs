using UnityEngine;

public class Cell
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

    public void PlaceElement(int newElementCode, int newElementOrientation)
    {
        CellBehaviour cellBehaviour = _cellObject.GetComponent<CellBehaviour>();
        cellBehaviour.PlaceElement(newElementCode, newElementOrientation);
    }

    public bool Available()
    {
        if (nextToGatewayB || nextToGatewayL || nextToGatewayR || nextToGatewayT)
        {
            return false;
        }
        CellBehaviour cellBehaviour = _cellObject.GetComponent<CellBehaviour>();
        if (cellBehaviour.elementCode != 0)
        {
            return false;
        }
        return true;
    }

    public bool PresentsGateway()
    {
        if (nextToGatewayB || nextToGatewayL || nextToGatewayR || nextToGatewayT)
        {
            return true;
        }
        return false;
    }

}