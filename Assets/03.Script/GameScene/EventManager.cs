using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;

    public int zaebal;


    private void Start()
    {
        공5개_추가_다음3턴_양체력2배.explaination =
            "공 5개를 추가하고, 다음 3턴 동안 나오는 양들의 체력이 2배가 됩니다.";
        공5개_추가_다음3턴_양체력2배.action += () =>
        {
            GameLogicManager.instance.ballCount += 5;
            GameLogicManager.instance.blockManager.doubleSheepHpCount = 3;
        };

        가장_앞줄양들_전부구원_공5개감소.explaination =
            "가장 앞에 있는 줄의 양들을 모두 즉시 구원합니다. 공 5개 감소";
        가장_앞줄양들_전부구원_공5개감소.action += () =>
        {
            GameLogicManager.instance.ballCount += 5;
            //var blockList = GameLogicManager.instance.blockManager.GetBlocksInSameYLineWithMaxY() = 3;
        };
    }
}
