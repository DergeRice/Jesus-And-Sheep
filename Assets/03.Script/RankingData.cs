using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RankingData 
{
    public int id;

    public int profileIndex;
    public string name;
    public string churchName;
    public int score;

    public string encryptedScore;
    public string code;

    public string encryptedTime;
    public string encryptedGameMode;
    public string gameMode;

}
