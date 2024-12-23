using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum PlayerPrefsData
{
    hasLastGame,
    isBgmOff,
    isSfxOff,
    nickname,
    profileIndex,
    highScore,
    gold,
    isFirstTime,
    lastGameUnlockSpeedMode,
    lastAdWatchedDate,

    remainAdWatchCount,

    firstItemPurchased,
}
public enum EVibrate
{
    weak,
    strong,
    nope
}
public enum BlockType
{
    Common,
    Double,
    Triple,
    Item,
    Chest,
    Coin,
    
}
public enum BallType
{
    Common,
    Cross,
    Bomb,
    Vertical,
    Horizontal,
    Split,
    Drill,
    Holly,

    Double
}



public class EnumClass
{

}

public enum GameEventType
{
    NextDoubleCount,
    FirstLineErase,
    RandomSelect,
    RandomBlockSpawn,

}

public enum GameEventRank
{
    Legend,
    Epic,
    Common
}


[System.Serializable]
public class GameEvent
{
    [SerializeField] private string eventName;
    public string explaination;

    // public GameEventType gameEventType;

    public UnityEvent unityAction;
    public Sprite icon;
    public GameEventRank rank;
}
[Serializable]
public class SaveData
{
    public BlockData[] blockDatas;  // 2D �迭�� 1D �迭�� ����
    public float[] percentOfBlockLevel;
    public bool IsGameOver;
    public bool IsShot;
    public int currentLevel;            // int ��
    public BallType[] ballTypes;
}

[System.Serializable]
public class UserList
{
    public User[] users;  // 사용자 정보 배열
}

[Serializable]
public class User
{
    public int id;
    public string nickname;
    public int profileindex;
    public int highscore;
    public DateTime lastSendTime; // 마지막 하트를 보낸 시간
}

[Serializable]
public class HeartData
{
    public string heartHash;
    public string senderNickName;
    public string targetNickName;
}

[System.Serializable]
public class ShootData
{
    public bool IsShot;
    public SerializableVector3 CurrentPos;
    public SerializableVector3 TargetVector;
}


[Serializable]
public class BlockData
{
    public int count;
    public int countMax;
    public string blockType;
    public int curX;
    public int curY;
    public SerializableVector3 targetPosition;

    public BlockData(Block block)
    {
        count = block.Count;
        countMax = block.countMax;
        blockType = block.blockType.ToString();
        curX = block.curX;
        curY = block.curY;
    }
}

[System.Serializable]
public class BlockGridWrapper
{
    public BlockData[] blockGrid;

    public BlockGridWrapper(List<List<BlockData>> blockList)
    {
        // Flattening the list of lists into a single array
        blockGrid = blockList.SelectMany(row => row).ToArray();
    }
}

[Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}