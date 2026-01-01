using UnityEngine;
using System.Collections.Generic;

public class CellBehaviour : MonoBehaviour
{
    public int[] probabilityScore;
    public Sprite[] floorTiles;
    public string[] floorCodes;

    public Sprite[] objectSprites;
    public string[] objectCodes;
    public int[] objectPosition;

    public int cellID;

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

    public void InitializeCell(int ID)
    {
        cellID = ID;
    }

    public void LoadFloorSprite() {
        Dictionary<string, object>[] probabilityRange = new Dictionary<string, object>[floorTiles.Length];

        for (int i = 0; i < floorTiles.Length; i++)
        {
            probabilityRange[i] = new Dictionary<string, object>();
        }
        int storedbBottom = 0;
        for (int i = 0; i < floorTiles.Length; i++)
        {
            probabilityRange[i]["bottom"] = storedbBottom;
            probabilityRange[i]["top"] = storedbBottom + probabilityScore[i];

            storedbBottom = storedbBottom + probabilityScore[i];
        }
        int randomSetting = Random.Range(1, 101);

        for(int i = 0; i < floorTiles.Length; i++)
        {
            int bottom = (int)probabilityRange[i]["bottom"];
            int top = (int)probabilityRange[i]["top"];

            if (randomSetting > bottom && randomSetting <= top)
            {
                _renderer.sprite = floorTiles[i];
                break;
            }
        }
    }
}

