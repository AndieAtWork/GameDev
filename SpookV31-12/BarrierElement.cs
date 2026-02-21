using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class Barrier : ElementBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract override void PlaceObject(Room room);

    public void GetPassageWidth(int barrierLength, int passage, int minWidth, int maxWidth)
    {
        HashSet<int> barrierPassage = new HashSet<int>();
        barrierPassage.Add(passage);

        int width = Random.Range(minWidth, maxWidth + 1);
        if (width > barrierLength - 1)
        {
            width = barrierLength - 1;
        }

        while (barrierPassage.Count < width)
        {
            int menor = barrierPassage.Min();
            int mayor = barrierPassage.Max();
            int random = Random.Range(0, 2);
            if (random == 0) 
            {
                if (menor >= 0)
                {
                    barrierPassage.Add(menor - 1);
                }
                else
                {
                    barrierPassage.Add(mayor + 1);
                }
            }
            else if (random == 1) 
            {
                if (mayor <= barrierLength - 1)
                {
                    barrierPassage.Add(mayor + 1);
                }
                else
                {
                    barrierPassage.Add(menor - 1);
                }
            }
        }

        

    }
}
