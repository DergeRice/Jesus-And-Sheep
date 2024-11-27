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
        firstImg.sprite = one.icon;
        firstText.text = one.explaination;
        firstButton.onClick.RemoveAllListeners();
        firstButton.onClick.AddListener(() => one.unityAction.Invoke());


        secondImg.sprite = two.icon;
        secondText.text = two.explaination;
        secondButton.onClick.RemoveAllListeners();
        secondButton.onClick.AddListener(() => two.unityAction.Invoke());
    }
}
