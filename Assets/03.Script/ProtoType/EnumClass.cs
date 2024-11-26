using System;
using UnityEngine;
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
[System.Serializable]
public class GameEvent : GameEventBase
{
    public string eventName;
    public string explaination;
    public Texture icon;
}
public class GameEventBase
{
    public Action action;
}
