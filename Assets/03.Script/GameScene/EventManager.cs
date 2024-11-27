using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;

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


    private void Start()
    {
        //공5개_추가_다음3턴_양체력2배.explaination =
        //    "공 5개를 추가하고, 다음 3턴 동안 나오는 양들의 체력이 2배가 됩니다.";
        //공5개_추가_다음3턴_양체력2배.action += () =>
        //{
        //    GameLogicManager.instance.ballCount += 5;
        //    GameLogicManager.instance.blockManager.doubleSheepHpCount = 3;
        //};

        //가장_앞줄양들_전부구원_공5개감소.explaination =
        //    "가장 앞에 있는 줄의 양들을 모두 즉시 구원합니다. 공 5개 감소";
        //가장_앞줄양들_전부구원_공5개감소.action += () =>
        //{
        //    GameLogicManager.instance.ballCount += 5;
        //    //var blockList = GameLogicManager.instance.blockManager.GetBlocksInSameYLineWithMaxY() = 3;
        //};
    }
}
