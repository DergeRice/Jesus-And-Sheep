using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameMode
{
    ClassicMode, 
    SurvivalMode,
    TimeAttackmode
}

public class SelectGameModePanel : MonoBehaviour
{
    public GameMode gameMode;
    public List<GameObject> gamemodeObjs = new List<GameObject>();


    public void SetPanelObjects(int gameModeIndex)
    {
        for (int i = 0; i < gamemodeObjs.Count; i++)
        {
            gamemodeObjs[i].SetActive(false);
        }
        gamemodeObjs[gameModeIndex].SetActive(true);
    }

}
