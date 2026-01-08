using UnityEngine;

public class WallBehaviour : MonoBehaviour
{
    public Sprite horizontalWall;
    public Sprite verticalWall;

    public Sprite bottomRightCorner;
    public Sprite topRightCorner;
    public Sprite bottomLeftCorner;
    public Sprite topLeftCorner;

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
        if (!bottomCell && !topCell && !rightCell && !leftCell) // Corner
        {
            if (right && top && !bottom && !left)
            {
                _renderer.sprite = bottomLeftCorner;
            }
            else if (right && bottom && !top && !left)
            {
                _renderer.sprite = topLeftCorner;
            }
            else if (left && top && !bottom && !right)
            {
                _renderer.sprite = bottomRightCorner;
            }
            else if (left && bottom && !top && !right)
            {
                _renderer.sprite = topRightCorner;
            }

        }
        else if (right && top && leftCell && bottomCell) // Inner Corners
        {
            _renderer.sprite = bottomLeftCorner;
        }
        else if (right && bottom && leftCell && topCell)
        {
            _renderer.sprite = topLeftCorner;
        }
        else if (left && top && rightCell && bottomCell)
        {
            _renderer.sprite = bottomRightCorner;
        }
        else if (left && bottom && rightCell && topCell)
        {
            _renderer.sprite = topRightCorner;
        }
        else if (top && bottom && !right && !left) // Vertical
        {
            _renderer.sprite = verticalWall;
        }
        else if (right && left && !top && !bottom) // Horizontal
        {
            _renderer.sprite = horizontalWall;
        }

        if (_renderer == null) 
        {
            _renderer.sprite = defaultWall;
        }

    }   
}
