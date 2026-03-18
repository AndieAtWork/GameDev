using UnityEngine;

public class GateWay : MonoBehaviour
{
    // From -> warp position in the screen
    // To -> where the warp leads in the screen
    public int warpFromPositionX;
    public int warpFromPositionY;
    public int warpToPositionY;
    public int warpToPositionX;

    public string orientation;

    public int toRoomID;
    public int fromRoomID;

    // These are the position of the closest cell. Not yet loaded.
    public int frameX;
    public int frameY;

    static bool canBeWarped = true;
    private PlayerBehaviour player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = PlayerBehaviour.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Gateways have trigger collisions
    void OnTriggerEnter2D(Collider2D other)
    {
        // canBeWarped is not in use
        // player.canBeWraped asks whether the player has moved after crossing the gate
        // if the player hasn't moved since, it does not trigger the reaction
        if (other.CompareTag("Player") && canBeWarped && player.canBeWarped)
        {
            // This moves the player after using a gateway a little further from where the following gate is triggered
            float auxDistance = 0.05f;
            float auxDistanceX = 0f;
            float auxDistanceY = 0f;
            int rotation = 0;

            if (player.forward) // When the player is moving forward, the rotation is different
            {
                if (orientation == "T")
                {
                    auxDistanceX = 0;
                    auxDistanceY = -auxDistance;
                    rotation = 180;
                }
                else if (orientation == "B")
                {
                    auxDistanceX = 0;
                    auxDistanceY = auxDistance;
                    rotation = 0;
                }
                else if (orientation == "R")
                {
                    auxDistanceX = -auxDistance;
                    auxDistanceY = 0;
                    rotation = 90;
                }
                else if (orientation == "L")
                {
                    auxDistanceX = auxDistance;
                    auxDistanceY = 0;
                    rotation = 270;
                }
            }
            else // When the player is moving backwards
            {
                if (orientation == "B")
                {
                    auxDistanceX = 0;
                    auxDistanceY = auxDistance;
                    rotation = 180;
                }
                else if (orientation == "T")
                {
                    auxDistanceX = 0;
                    auxDistanceY = -auxDistance;
                    rotation = 0;
                }
                else if (orientation == "L")
                {
                    auxDistanceX = auxDistance;
                    auxDistanceY = 0;
                    rotation = 90;
                }
                else if (orientation == "R")
                {
                    auxDistanceX = -auxDistance;
                    auxDistanceY = 0;
                    rotation = 270;
                }
            }

            //Debug.Log("-------------");
            //Debug.Log(auxDistanceX);
            //Debug.Log(auxDistanceY);

            // Position and rotation are adjusted
            other.transform.position = new Vector2(warpToPositionX + auxDistanceX, warpToPositionY + auxDistanceY);
            other.transform.rotation = Quaternion.Euler(0, 0, rotation);
            CameraBehaviour cameraBehaviour = CameraBehaviour.Instance;
            GameObject camera = cameraBehaviour.gameObject;

            // Though the camera is moved in CameraBehaviour, it is rotated here
            camera.transform.rotation = Quaternion.Euler(0, 0, rotation);
            
            // canBeWarped = false;
            player.canBeWarped = false;

            //Debug.Log("WARPED:");
            //Debug.Log("warpX " + warpToPositionX);
            //Debug.Log("warpY " + warpFromPositionY);
            //Debug.Log("orientation " + orientation);
            //Debug.Log("can be warped " + player.canBeWarped.ToString());
        }
    }

    //void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        canBeWarped = true;
    //    }
    //}

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
    public void SetToScreenPositions(int toX, int toY, int toRoom) 
    {
        warpToPositionY = toY;
        warpToPositionX = toX;

        toRoomID = toRoom;
    }

    public int[] GetFramePositions() // This is the position of the closes cell
    {
        int[] coord = { frameX, frameY };
        return coord;
    }

    public int[] GetFromScreenPositions() // Used to create scene JSON
    {
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        int[] coord = { x, y };
        return coord;
    }

    public int[] GetToScreenPositions() // Used to create scene JSON
    {
        int[] coord = { warpToPositionX, warpToPositionY };
        return coord;
    }

    public void SetDestinationOrientation(string o) // The orientation the player takes when using the gate
    {
        orientation = o;
    }
}
