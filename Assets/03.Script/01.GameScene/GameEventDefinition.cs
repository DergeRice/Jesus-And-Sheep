using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEventDefinition", menuName = "Jesus And Sheep/Game Event Definition")]
public class GameEventDefinition : ScriptableObject
{
    public string eventName;
    public string eventNameEn;

    [TextArea(2, 5)]
    public string explanation;

    [TextArea(2, 5)]
    public string explanationEn;

    public Sprite icon;
    public GameEventRank rank;
    public bool isSpecialBallEvent;
    public List<GameEventEffect> effects = new List<GameEventEffect>();

    public string GetDisplayName(string languageCode)
    {
        if (languageCode == "en" && !string.IsNullOrWhiteSpace(eventNameEn))
        {
            return eventNameEn;
        }

        if (languageCode == "en" && LanguageManager.HasInstance)
        {
            return LanguageManager.Instance.Localize(eventName);
        }

        return eventName;
    }

    public string GetExplanation(string languageCode)
    {
        if (languageCode == "en" && !string.IsNullOrWhiteSpace(explanationEn))
        {
            return explanationEn;
        }

        if (languageCode == "en" && LanguageManager.HasInstance)
        {
            return LanguageManager.Instance.Localize(explanation);
        }

        return explanation;
    }
}

[Serializable]
public class GameEventEffect
{
    public GameEventEffectType effectType;
    public int intValue;
    public float floatValue;
    public BallType ballType = BallType.Common;
}

public enum GameEventEffectType
{
    BallCountIncrease,
    BallCountMultiply,
    NextSheepHpDouble,
    FirstLineErase,
    AllBlockErase,
    AllBlockDoubleHp,
    WaitForTurn,
    RandomBlockTriple,
    UnderHpErase,
    RandomBlockErase,
    AllBlockDamage,
    RandomThreeBlockDamage,
    RandomBlockDoubleHp,
    RandomBlockGenerate,
    CoinTossCommonBallTo,
    CoinTossSpecialBallTo,
    GetSpecialBall,
}
