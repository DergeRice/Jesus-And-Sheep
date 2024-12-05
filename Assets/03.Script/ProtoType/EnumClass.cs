using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public enum EVibrate
{
    weak,
    strong,
    nope
}
public enum BlockType
{
    Common,
    Giant,
    Split,
    Double,
    BottomIgnore,
    Item,
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
    Holly
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
    public BlockData[] blockDatas;  // 2D 배열을 1D 배열로 저장
    public float[] percentOfBlockLevel;
    public SerializableVector3 currentPos;
    public SerializableVector3 targetVector;
    public bool IsGameOver;
    public bool IsShot;
    public int currentLevel;            // int 값
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