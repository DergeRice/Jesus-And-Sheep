using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectPanel : PanelBase
{
    public Image firstImg, secondImg;

    public TMP_Text firstText, secondText;

    public Button firstButton, secondButton;

    
    public void ShowPanel(GameEvent one, GameEvent two)
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


    }
}
