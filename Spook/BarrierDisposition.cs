using UnityEngine;

public class BarrierDisposition : MonoBehaviour
{
    public GameObject barrierPrefab;
    public int barrierChance; // Chance of there being a barrier
    public int barrierDice; // How many times that chance is played

    public int distanceFromWalls;
    
    // width of passages crossing barriers
    public int passageMinWidth;
    public int passageMaxWidth;
}
