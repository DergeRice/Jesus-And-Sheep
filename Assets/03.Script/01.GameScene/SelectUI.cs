using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectUI : MonoBehaviour
{
    public Image rankImg, IconImg;
    public Button selectButton;
    public TMP_Text context, rankText;

    private GameEventDefinition currentEvent;

    private void OnEnable()
    {
        LanguageManager.Instance.LanguageChanged += RefreshExplanation;
        RefreshExplanation(LanguageManager.Instance.CurrentLanguageCode);
    }

    private void OnDisable()
    {
        if (LanguageManager.HasInstance)
            LanguageManager.Instance.LanguageChanged -= RefreshExplanation;
    }

    private void RefreshExplanation(string languageCode)
    {
        if (currentEvent != null && context != null)
            LocalizedText.Attach(context).SetSource(currentEvent.GetExplanation(languageCode));
    }

    public void InitSetting(GameEventSelectionOption option, Sprite rankSprite)
    {
        GameEventDefinition gameEvent = option.Definition;
        currentEvent = gameEvent;
        rankImg.sprite = rankSprite;
        IconImg.sprite = gameEvent.icon;
        if (gameEvent.rank == GameEventRank.Legend) rankText.text = "S";
        if (gameEvent.rank == GameEventRank.Epic) rankText.text = "A";
        if (gameEvent.rank == GameEventRank.Common) rankText.text = "B";

        if (context != null)
        {
            Vector4 padding = context.margin;
            padding.x = Mathf.Max(padding.x, 16f);
            padding.z = Mathf.Max(padding.z, 16f);
            context.margin = padding;
            RefreshExplanation(LanguageManager.Instance.CurrentLanguageCode);
        }
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>
        {
            option.OnSelected?.Invoke();
            Utils.DelayCall(() =>
            {
                var logic = GameLogicManager.instance;
                if (logic != null && !logic.isGameOver && (logic.eventManager == null || !logic.eventManager.IsSkippingTurns))
                    logic.isPlayerTurn = true;
            }, 0.3f);
        });


    }
}
