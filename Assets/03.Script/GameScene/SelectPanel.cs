using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SelectPanel : PanelBase //,  IPointerDownHandler, IPointerUpHandler
{
    public Image firstImg, secondImg, thirdImg;

    public TMP_Text firstText, secondText, thirdText;

    public Button firstButton, secondButton, thirdButton;

    public Button seeMeadowButton;

    public CanvasGroup parent;

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

        firstImg.sprite = one.icon;
        firstText.text = one.explaination;
        firstButton.onClick.RemoveAllListeners();
        firstButton.onClick.AddListener(() =>
        {
            one.unityAction.Invoke();
            root.SetActive(false);
            Utils.DelayCall(() => { GameLogicManager.instance.isPlayerTurn = true; }, 0.3f);
        });


        secondImg.sprite = two.icon;
        secondText.text = two.explaination;
        secondButton.onClick.RemoveAllListeners();
        secondButton.onClick.AddListener(() =>
        {
            two.unityAction.Invoke();
            root.SetActive(false);
            Utils.DelayCall(()=> { GameLogicManager.instance.isPlayerTurn = true;},0.3f);
        });

        thirdImg.sprite = three.icon;
        thirdText.text = three.explaination;
        thirdButton.onClick.RemoveAllListeners();
        thirdButton.onClick.AddListener(() =>
        {
            three.unityAction.Invoke();
            root.SetActive(false);
            Utils.DelayCall(()=> { GameLogicManager.instance.isPlayerTurn = true;},0.3f);
        });


    }
}
