using UnityEngine;

public class WallBehaviour : MonoBehaviour
{
    public Sprite horizontalWall;
    public Sprite verticalWall;

    public Sprite bottomRightCorner;
    public Sprite topRightCorner;
    public Sprite bottomLeftCorner;
    public Sprite topLeftCorner;

    public Sprite rightTip;
    public Sprite leftTip;
    public Sprite topTip;
    public Sprite bottomTip;

    public Sprite defaultWall;

    public bool top;
    public bool bottom;
    public bool left;
    public bool right;

    public bool topCell;
    public bool bottomCell;
    public bool leftCell;
    public bool rightCell;

    public int frameX;
    public int frameY;

    private SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadFrame(int x, int y)
    {
        frameX = x;
        frameY = y;
    }

    public void LoadSprite()
    {
        bool set = false;

        if (!bottomCell && !topCell && !rightCell && !leftCell) // Corner
        {
            if (right && top && !bottom && !left)
            {
                _renderer.sprite = bottomLeftCorner;
                set = true;
            }
            else if (right && bottom && !top && !left)
            {
                _renderer.sprite = topLeftCorner;
                set = true;
            }
            else if (left && top && !bottom && !right)
            {
                _renderer.sprite = bottomRightCorner;
                set = true;
            }
            else if (left && bottom && !top && !right)
            {
                _renderer.sprite = topRightCorner;
                set = true;
            }

        }
        if (!set)
        {
            if (right && top && leftCell && bottomCell) // Inner Corners
            {
                _renderer.sprite = bottomLeftCorner;
                set = true;
            }
            else if (right && bottom && leftCell && topCell)
            {
                _renderer.sprite = topLeftCorner;
                set = true;
            }
            else if (left && top && rightCell && bottomCell)
            {
                _renderer.sprite = bottomRightCorner;
                set = true;
            }
            else if (left && bottom && rightCell && topCell)
            {
                _renderer.sprite = topRightCorner;
                set = true;
            }
            else if (top && bottom) // Vertical  && !right && !left
            {
                _renderer.sprite = verticalWall;
                set = true;
            }
            else if (right && left) // Horizontal  && !top && !bottom
            {
                _renderer.sprite = horizontalWall;
                set = true;
            }
            else if (right && !top && !bottom && !left) // Tips
            {
                _renderer.sprite = leftTip;
                set = true;
            }
            else if (left && !top && !bottom && !right)
            {
                _renderer.sprite = rightTip;
                set = true;
            }
            else if (top && !right && !left && !bottom)
            {
                _renderer.sprite = bottomTip;
                set = true;
            }
            else if (bottom && !left && !top && !right)
            {
                _renderer.sprite = topTip;
                set = true;
            }
        }
        

        if (!set)
        {
            _renderer.sprite = defaultWall;
        }

    }   
}
