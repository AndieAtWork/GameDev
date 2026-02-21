using UnityEngine;

public class GateWay : MonoBehaviour
{
    public int warpFromPositionX;
    public int warpFromPositionY;
    public int warpToPositionY;
    public int warpToPositionX;

    public int toRoomID;
    public int fromRoomID;

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

    public void SetFramePositions(int x, int y)
    {
        frameX = x;
        frameY = y;
    }

    public void SetFromScreenPositions(int fromX, int fromY, int fromRoom)
    {
        warpFromPositionX = fromX;
        warpFromPositionY = fromY;

        fromRoomID = fromRoom;
    }

    public void SetToScreenPositions(int toX, int toY, int toRoom, string scene = null)
    {
        warpToPositionY = toY;
        warpToPositionX = toX;

        toRoomID = toRoom;

        nextScene = scene;

    }

    public int[] GetFramePositions()
    {
        int[] coord = { frameX, frameY };
        return coord;
    }

    public int[] GetFromScreenPositions()
    {
        int[] coord = { warpFromPositionX, warpFromPositionY };
        return coord;
    }

    public int[] GetToScreenPositions()
    {
        int[] coord = { warpToPositionX, warpToPositionY };
        return coord;
    }
}
