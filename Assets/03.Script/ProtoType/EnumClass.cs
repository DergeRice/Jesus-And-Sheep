using System;
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
}
public enum BallType
{
    Common,
    Cross,
    Bomb,
    Vertical,
    Horizontal,
    Arrow
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

[System.Serializable]
public class GameEvent
{
    [SerializeField] private string eventName;
    public string explaination;

    // public GameEventType gameEventType;

    public UnityEvent unityAction;
    public Sprite icon;
}
