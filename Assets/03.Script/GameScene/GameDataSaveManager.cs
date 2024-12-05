using UnityEngine;

public class GameDataSaveManager : MonoBehaviour
{
    public bool isEndedGame;
    private bool isShootedAlreay = false;
    private Vector3 initialPos , shootingVec;

    public int savedLevel;
    
    public string blockDataJson;
    
    public string savedWeighedValues;

}
