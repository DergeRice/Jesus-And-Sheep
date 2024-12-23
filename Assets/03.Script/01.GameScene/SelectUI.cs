using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectUI : MonoBehaviour
{
    public Image rankImg, IconImg;
    public Button selectButton;
    public TMP_Text context, rankText;


    public void InitSetting(GameEvent gameEvent, Sprite rankSprite) 
    {
        rankImg.sprite = rankSprite;
        IconImg.sprite = gameEvent.icon;
        if (gameEvent.rank == GameEventRank.Legend) rankText.text = "S";
        if (gameEvent.rank == GameEventRank.Epic) rankText.text = "A";
        if (gameEvent.rank == GameEventRank.Common) rankText.text = "B";

        context.text = gameEvent.explaination;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => 
        { 
            gameEvent.unityAction.Invoke();
            Utils.DelayCall(() => { GameLogicManager.instance.isPlayerTurn = true; }, 0.3f);
        });
        
        
    }
}
