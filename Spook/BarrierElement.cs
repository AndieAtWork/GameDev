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

    // Returns all cells of passage within barrier
    public HashSet<int> GetPassageWidth(int barrierLength, int passage, int minWidth, int maxWidth)
    {
        HashSet<int> barrierPassage = new HashSet<int>(); // All the cells within the barrier left blank (or with bridge)
        barrierPassage.Add(passage); // passage is the initial position

        int width = Random.Range(minWidth, maxWidth + 1); // The random width is set via BarrierDisposition 
        if (width > barrierLength - 1) // width == barrier - 1 Means there is a piece of wall and a 1width passage
        {
            width = barrierLength - 1; // max width set
        }

        while (barrierPassage.Count < width)
        {
            int min = barrierPassage.Min();
            int max = barrierPassage.Max();
            int random = Random.Range(0, 2); // The passage grows in width in a random direction
            if (random == 0) 
            {
                if (min > 0) // min reached 0 cannot be added
                {
                    barrierPassage.Add(min - 1);
                }
                else
                {
                    barrierPassage.Add(max + 1);
                }
            }
            else if (random == 1) 
            {
                if (max < barrierLength - 1) // max the size of max index cannot be added 
                {
                    barrierPassage.Add(max + 1);
                }
                else
                {
                    barrierPassage.Add(min - 1);
                }
            }
        }
        return barrierPassage;
    }
}
