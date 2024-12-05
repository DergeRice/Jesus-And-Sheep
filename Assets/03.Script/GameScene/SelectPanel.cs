using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SelectPanel : PanelBase //,  IPointerDownHandler, IPointerUpHandler
{

    public Sprite legend, epic, common;

    public Button seeMeadowButton;

    public CanvasGroup parent;

    public SelectUI one, two, three;

    public void OnPointerDown()
    {
        parent.alpha = 0;
    }

    public void OnPointerUp()
    {
        parent.alpha = 1;
    }

    public void ShowPanel(GameEvent one, GameEvent two,GameEvent three)
    {
        root.SetActive(true);

        GameLogicManager.instance.isPlayerTurn = false;

        this.one.InitSetting(one, GetRankSprite(one));
        this.two.InitSetting(two, GetRankSprite(two));
        this.three.InitSetting(three, GetRankSprite(three));

        this.one.selectButton.onClick.AddListener(() =>{ root.SetActive(false);});
        this.two.selectButton.onClick.AddListener(() =>{ root.SetActive(false);});
        this.three.selectButton.onClick.AddListener(() =>{ root.SetActive(false);});
    }

    public Sprite GetRankSprite(GameEvent gameEvent)
    {
        Sprite um = null;
        if (gameEvent.rank == GameEventRank.Legend) um = legend;
        if (gameEvent.rank == GameEventRank.Epic) um = epic;
        if (gameEvent.rank == GameEventRank.Common) um = common;

        return um;
    }
}
