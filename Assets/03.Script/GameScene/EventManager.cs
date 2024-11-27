using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;
    GameLogicManager gameLogicManager;

    public List<GameEvent> gameEvents = new List<GameEvent>();
    //[Header(" 공5개_추가_다음3턴_양체력2배")]
    //GameEvent a;
    //[Header(" 가장_앞줄양들_전부구원_공5개감소")]
    //GameEvent b;
    //[Header(" 공_절반만_남기고_전부_구원")]
    //[Header(" 화면에있는_모든양들_체력2배_공3개추가")]
    //[Header(" 랜덤_10개_블록_체력_2배_공5개추가")]
    //[Header(" 한턴쉬고_공5개추가")]
    //[Header(" 두턴쉬고_공10개추가")]
    //[Header(" 세턴쉬고_공15개추가")]
    //[Header(" 랜덤_한개_블록_체력_3배_공3개추가")]
    //[Header(" 체력30미만_양들_즉시구원_공1개감소")]
    //[Header(" 체력100미만_양들_즉시구원_공10개감소")]
    //[Header(" 모든애들에게50데미지_공5개감소")]
    //[Header(" 모든애들에게10데미지_공1개감소")]
    //[Header(" 랜덤3개블록_100데미지_공3개감소")]
    //[Header(" 랜덤으로블록5개설치_공5개추가")]
    //[Header(" 랜덤으로블록10개설치_랜덤특수공1개추가")]
    //[Header(" 반반확률로_공5추가_공2감소")]
    //[Header(" 반반확률로_공5추가_블록3개소환")]
    //[Header(" 반반확률로_특수공1추가_공2감소")]
    //[Header(" 반반확률로_특수공1추가_블록5개소환")]
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        gameLogicManager = GameLogicManager.instance;
    }


    public void BallCountChange(int amount)
    {
        gameLogicManager.ballCount  += amount;
    }
    public void BallCountChange(float amount)
    {
        gameLogicManager.ballCount  = (int)(gameLogicManager.ballCount * amount);
    }

    public void NextDouble(int amount)
    {
        gameLogicManager.blockManager.doubleSheepHpCount += amount;
    }

    public void EraseFirstLine()
    {
       var temp = gameLogicManager.blockManager.GetBlocksInSameYLineWithMaxY();
       
       foreach (var item in temp)
       {
            item.DestroyAnimation();
       }
    }

    public void EraseAllBlocks()
    {
       var temp = gameLogicManager.blockManager.GetAvailableBlocks();
       
       foreach (var item in temp)
       {
            item.DestroyAnimation();
       }
    }
    
    public void SetDoubleHpAllBlock()
    {
        var temp = gameLogicManager.blockManager.GetAvailableBlocks();
       
       foreach (var item in temp)
       {
            item.count *= 2 ;
       }
    }

    public void SetWaitForTurn(int turnAmount)
    {
        for (int i = 0; i < turnAmount; i++)
        {
            gameLogicManager.GetAllBallDown();
        }
    }

}
