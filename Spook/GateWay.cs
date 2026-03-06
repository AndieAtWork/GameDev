using UnityEngine;

public class GateWay : MonoBehaviour
{
    // From -> warp position in the screen
    // To -> where the warp leads in the screen
    public int warpFromPositionX;
    public int warpFromPositionY;
    public int warpToPositionY;
    public int warpToPositionX;

    public int toRoomID;
    public int fromRoomID;

    // These are the position of the closest cell. Not yet loaded.
    public int frameX;
    public int frameY;

    public string nextScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetFramePositions(int x, int y) // Not loaded yet
    {
        frameX = x;
        frameY = y;
    }

    public void SetFromScreenPositions(int fromX, int fromY, int fromRoom) // Used when SAVING to create the scene JSON
    {
        warpFromPositionX = fromX;
        warpFromPositionY = fromY;

        fromRoomID = fromRoom;
    }

    // Used when SAVING to create the scene JSON. Used when loading to set warp behaviour
    public void SetToScreenPositions(int toX, int toY, int toRoom, string scene = null) 
    {
        warpToPositionY = toY;
        warpToPositionX = toX;

        toRoomID = toRoom;

        nextScene = scene;

    }

    public int[] GetFramePositions() // This is the position of the closes cell
    {
        int[] coord = { frameX, frameY };
        return coord;
    }

    public int[] GetFromScreenPositions() // Used to create scene JSON
    {
        int[] coord = { warpFromPositionX, warpFromPositionY };
        return coord;
    }

    public int[] GetToScreenPositions() // Used to create scene JSON
    {
        int[] coord = { warpToPositionX, warpToPositionY };
        return coord;
    }
}
