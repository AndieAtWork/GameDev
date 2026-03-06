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

    public void InitializeCell(int x, int y, bool set = false, int code = 0, GameObject prefab = null)
    {
        GameObject cellPrefab = null;
        if (set)
        {
            cellPrefab = prefab;
        }
        else
        {
           cellPrefab = MazeManager.Instance.cellTypePrefab;
        }
            _xPosition = x;
        _yPosition = y;
        _cellObject = UnityEngine.Object.Instantiate(
            cellPrefab,
            new Vector2(_xPosition, _yPosition),
            Quaternion.identity,
            _gameObjectParent.transform
        );
        CellBehaviour cellBehaviour = _cellObject.GetComponent<CellBehaviour>();
        cellBehaviour.InitializeCell(_cellID);
        if (set) 
        {
            cellBehaviour.SetSprite(code);
        }
        else
        {
            cellBehaviour.LoadFloorSprite();
        }
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
        if (PresentsGateway()) // cannot be placed in the way of a gate
        {
            return false;
        }
        CellBehaviour cellBehaviour = _cellObject.GetComponent<CellBehaviour>();
        if (cellBehaviour.elementCode != 0) // Empty space
        {
            return false;
        }
        return true;
    }

    public bool AvailableForBarrier(string barrierOrientation)
    {
        if (PresentsGateway()) // cannot be placed in the way of a gate
        {
            return false;
        }
        CellBehaviour cellBehaviour = _cellObject.GetComponent<CellBehaviour>();
        if (cellBehaviour.elementCode == 0) // Empty space
        {
            return true;
        }
        if (cellBehaviour.elementCode != 0) // Space is not empty
        {
            // This is specifically for when a barrier crosses the passage of another barrier
            // If the orientations are different, they are given a leeway
            // The distanceFromWalls from Barrier Disposition is not taken into account

            string orientation = null;
            if (cellBehaviour.elementOrientation == 0 || cellBehaviour.elementOrientation == 180)
            {
                orientation = "L";
            }
            else
            {
                orientation = "B";
            }
            
            if (barrierOrientation == orientation)
            {
                return false;
            }
            else
            {
                return true;
            }
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

    public GameObject GetCellObject()
    {
        return _cellObject;
    }

}