using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using RandomElementsSystem.Types;
using GG.Infrastructure.Utils;
using System.Collections;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;
    GameLogicManager gameLogicManager;

    public WeightedListOfInts eventRank;

    public List<GameEventDefinition> eventDefinitions = new List<GameEventDefinition>();
    public List<GameEventDefinition> specialBallDefinitions = new List<GameEventDefinition>();

    [FormerlySerializedAs("gameEvents")]
    [SerializeField] private List<GameEvent> legacyGameEvents = new List<GameEvent>();
    [FormerlySerializedAs("specialBallEvents")]
    [SerializeField] private List<GameEvent> legacySpecialBallEvents = new List<GameEvent>();

    readonly List<GameEventDefinition> gameLegendEvents = new List<GameEventDefinition>();
    readonly List<GameEventDefinition> gameEpicEvents = new List<GameEventDefinition>();
    readonly List<GameEventDefinition> gameCommonEvents = new List<GameEventDefinition>();

    bool currentEventSuccess;
    public bool IsSkippingTurns { get; private set; }
    public List<UnityEvent> failActionList = new List<UnityEvent>();

    public TimeLineManager timeLineManager;

    private void Start()
    {
        gameLogicManager = GameLogicManager.instance;
        PopulateEventLists();
    }
    private void PopulateEventLists()
    {
        gameLegendEvents.Clear();
        gameEpicEvents.Clear();
        gameCommonEvents.Clear();

        foreach (var gameEvent in eventDefinitions)
        {
            if (gameEvent == null)
            {
                continue;
            }

            switch (gameEvent.rank)
            {
                case GameEventRank.Legend:
                    gameLegendEvents.Add(gameEvent);
                    break;
                case GameEventRank.Epic:
                    gameEpicEvents.Add(gameEvent);
                    break;
                case GameEventRank.Common:
                    gameCommonEvents.Add(gameEvent);
                    break;
            }
        }

        Debug.Log($"Legend Events: {gameLegendEvents.Count}, Epic Events: {gameEpicEvents.Count}, Common Events: {gameCommonEvents.Count}");
    }

    [ContextMenu("GoRandom")]
    public void ShowTwoSelection()
    {
        SoundManager.instance.PlayEventPanelSound();
        gameLogicManager.isPlayerTurn = false;
        // Determine the event list based on the weighted rank
        List<GameEventDefinition> selectedList = null;
        switch (eventRank.GetRandomByWeight())
        {
            case 0: // Legend
                selectedList = gameLegendEvents;
                break;
            case 1: // Epic
                selectedList = gameEpicEvents;
                break;
            case 2: // Common
                selectedList = gameCommonEvents;
                break;
        }

        if (selectedList == null || selectedList.Count == 0)
        {
            gameLogicManager.isPlayerTurn = true;
            Debug.LogWarning("No events available for the selected rank!");
            return;
        }

        // Get unique random events from the selected list
        int firstValue = Random.Range(0, selectedList.Count);
        int secondValue = GetUniqueRandomIndex(selectedList.Count, firstValue);

        if (Random.value < 0.90f && specialBallDefinitions.Count > 1) // 90%
        {
            // Show a special ball event along with two random events
            gameLogicManager.ShowSelectPanel(
                CreateSelectionOption(specialBallDefinitions[Random.Range(1, specialBallDefinitions.Count)]),
                CreateSelectionOption(selectedList[firstValue]),
                CreateSelectionOption(selectedList[secondValue])
            );
            return;
        }

        // If not 90%, include a third unique event
        int thirdValue = GetUniqueRandomIndex(selectedList.Count, firstValue, secondValue);
        gameLogicManager.ShowSelectPanel(
            CreateSelectionOption(selectedList[firstValue]),
            CreateSelectionOption(selectedList[secondValue]),
            CreateSelectionOption(selectedList[thirdValue])
        );
    }

    private GameEventSelectionOption CreateSelectionOption(GameEventDefinition definition)
    {
        return new GameEventSelectionOption(definition, () => ExecuteDefinition(definition));
    }

    public void ExecuteDefinition(GameEventDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        for (int i = 0; i < definition.effects.Count; i++)
        {
            ExecuteEffect(definition.effects[i]);
        }
    }

    private void ExecuteEffect(GameEventEffect effect)
    {
        switch (effect.effectType)
        {
            case GameEventEffectType.BallCountIncrease:
                SetBallCountIncrease(effect.intValue);
                break;
            case GameEventEffectType.BallCountMultiply:
                SetBallCountMultifly(effect.floatValue);
                break;
            case GameEventEffectType.NextSheepHpDouble:
                SetNextSheepHpDouble(effect.intValue);
                break;
            case GameEventEffectType.FirstLineErase:
                SetFirstLineErease();
                break;
            case GameEventEffectType.AllBlockErase:
                SetAllBlockErease();
                break;
            case GameEventEffectType.AllBlockDoubleHp:
                SetAllBlockDoubleHp();
                break;
            case GameEventEffectType.WaitForTurn:
                SetWaitForTurn(effect.intValue);
                break;
            case GameEventEffectType.RandomBlockTriple:
                SetRandomBlockTriple(effect.intValue);
                break;
            case GameEventEffectType.UnderHpErase:
                SetUnderHpErase(effect.intValue);
                break;
            case GameEventEffectType.RandomBlockErase:
                SetRandomBlockErase(effect.intValue);
                break;
            case GameEventEffectType.AllBlockDamage:
                SetAllBlockDamage(effect.intValue);
                break;
            case GameEventEffectType.RandomThreeBlockDamage:
                SetRandomThreeBlockDamage(effect.intValue);
                break;
            case GameEventEffectType.RandomBlockDoubleHp:
                SetRandomBlockDoubleHp(effect.intValue);
                break;
            case GameEventEffectType.RandomBlockGenerate:
                SetRandomBlockGenerate(effect.intValue);
                break;
            case GameEventEffectType.CoinTossCommonBallTo:
                SetCoinTossCommonBallTo(effect.intValue);
                break;
            case GameEventEffectType.CoinTossSpecialBallTo:
                SetCoinTossSpecialBallTo(effect.intValue);
                break;
            case GameEventEffectType.GetSpecialBall:
                gameLogicManager.GetSpecialBall(effect.ballType);
                break;
        }
    }

    private int GetUniqueRandomIndex(int count, params int[] exclude)
    {
        for (int i = 0; i < 100; i++)
        {
            int randomIndex = Random.Range(0, count);
            if (System.Array.IndexOf(exclude, randomIndex) == -1)
                return randomIndex;
        }

        Debug.LogWarning("Failed to find a unique random index after 100 attempts.");
        return 0; // Fallback to a default value
    }


    public void SetBallCountIncrease(int amount)
    {
        gameLogicManager.ChangeBallCount(amount);


    }
    public void SetBallCountMultifly(float amount)
    {
        int target = Mathf.Max(1, Mathf.FloorToInt(gameLogicManager.GetBallCount * amount));
        gameLogicManager.ChangeBallCount(target - gameLogicManager.GetBallCount);
    }

    public void SetNextSheepHpDouble(int amount)
    {
        gameLogicManager.blockManager.doubleSheepHpCount += amount;
    }

    public void SetFirstLineErease()
    {
        var temp = gameLogicManager.blockManager.GetBlocksInSameYLineWithMaxY();

        foreach (var item in temp)
        {
            item.DestroyAnimation();
        }
    }

    public void SetAllBlockErease()
    {
        var temp = gameLogicManager.blockManager.GetAvailableBlocks();

        foreach (var item in temp)
        {
            item.DestroyAnimation();
        }
    }

    public void SetAllBlockDoubleHp()
    {
        var temp = gameLogicManager.blockManager.GetAvailableBlocks();

        foreach (var item in temp)
        {
            item.Count *= 2;
        }
    }

    public void SetWaitForTurn(int turnAmount)
    {
        StartCoroutine(SetWaitForTurnCoroutine(turnAmount));
    }
    IEnumerator SetWaitForTurnCoroutine(int turnAmount)
    {
        IsSkippingTurns = true;
        try
        {
            for (int i = 0; i < turnAmount; i++)
            {
                if (gameLogicManager.isGameOver) yield break;
                gameLogicManager.isPlayerTurn = false;
                gameLogicManager.AllBallComeDown();
                yield return new WaitUntil(() => gameLogicManager.isPlayerTurn || gameLogicManager.isGameOver);
                if (gameLogicManager.isGameOver) yield break;
                yield return new WaitForSeconds(2f);
            }
        }
        finally
        {
            IsSkippingTurns = false;
        }
    }

    public void SetRandomBlockTriple(int count)
    {
        var temp = gameLogicManager.blockManager.GetRandomBlock(count);

        foreach (var item in temp)
        {
            item.Count *= 3;
        }
    }

    public void SetUnderHpErase(int value)
    {
        var temp = gameLogicManager.blockManager.GetBlocksWithCountLessThan(value);

        foreach (var item in temp)
        {
            item.DestroyAnimation();
        }
    }

    public void SetRandomBlockErase(int value)
    {
        var temp = gameLogicManager.blockManager.GetRandomBlock(value);

        foreach (var item in temp)
        {
            item.DestroyAnimation();
        }
    }

    public void SetAllBlockDamage(int amount)
    {
        var temp = gameLogicManager.blockManager.GetAvailableBlocks();

        foreach (var item in temp)
        {
            item.Count -= amount;
        }
    }

    public void SetRandomThreeBlockDamage(int amount)
    {
        var temp = gameLogicManager.blockManager.GetRandomBlock(3);

        foreach (var item in temp)
        {
            item.Count -= amount;
        }
    }

    public void SetRandomBlockDoubleHp(int amount)
    {
        var temp = gameLogicManager.blockManager.GetRandomBlock(amount);

        foreach (var item in temp)
        {
            item.Count *= 2;
        }
    }

    public void SetRandomBlockGenerate(int count)
    {
        for (int i = 0; i < count; i++)
        {
            gameLogicManager.blockManager.SpawnRandomBlock(false);
        }
    }

    public void SetCoinTossCommonBallTo(int failActionIndex)
    {
        currentEventSuccess = Random.value > 0.5f;

        if (currentEventSuccess) 
        {
            SetBallCountIncrease(8);
            timeLineManager.PlayTimeLine(true);
        }
        else 
        {
            failActionList[failActionIndex].Invoke();
            timeLineManager.PlayTimeLine(false);
        }
    }

    public void SetCoinTossSpecialBallTo(int failActionIndex)
    {
        currentEventSuccess = Random.value > 0.5f;

        if (currentEventSuccess) 
        {
            gameLogicManager.GetRandomSpecialBall(3);
            timeLineManager.PlayTimeLine(true);
        }
        else 
        {
            failActionList[failActionIndex].Invoke();
            timeLineManager.PlayTimeLine(false);
        }
    }


    public void GetCrossBall() { gameLogicManager.GetSpecialBall(BallType.Cross); }
    public void GetBombBall() { gameLogicManager.GetSpecialBall(BallType.Bomb); }
    public void GetVerticalBall() { gameLogicManager.GetSpecialBall(BallType.Vertical); }
    public void GetHorizontalBall() { gameLogicManager.GetSpecialBall(BallType.Horizontal); }
    public void GetSplitBall() { gameLogicManager.GetSpecialBall(BallType.Split); }
    public void GetDrillBall() { gameLogicManager.GetSpecialBall(BallType.Drill); }
    public void GetHollyBall() { gameLogicManager.GetSpecialBall(BallType.Holly); }

#if UNITY_EDITOR
    [ContextMenu("Migrate Legacy Events To SO")]
    private void MigrateLegacyEventsToScriptableObjects()
    {
        const string rootFolder = "Assets/08.Data";
        const string eventFolder = "Assets/08.Data/GameEvents";
        const string normalFolder = "Assets/08.Data/GameEvents/Normal";
        const string specialFolder = "Assets/08.Data/GameEvents/Special";

        EnsureFolder("Assets", "08.Data");
        EnsureFolder(rootFolder, "GameEvents");
        EnsureFolder(eventFolder, "Normal");
        EnsureFolder(eventFolder, "Special");

        eventDefinitions = MigrateListToAssets(legacyGameEvents, "legacyGameEvents", normalFolder, false);
        specialBallDefinitions = MigrateListToAssets(legacySpecialBallEvents, "legacySpecialBallEvents", specialFolder, true);

        EditorUtility.SetDirty(this);
        if (gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Migrated {eventDefinitions.Count} normal events and {specialBallDefinitions.Count} special events.");
    }

    private List<GameEventDefinition> MigrateListToAssets(List<GameEvent> legacyList, string propertyName, string folderPath, bool isSpecialBall)
    {
        var createdDefinitions = new List<GameEventDefinition>();
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty listProperty = serializedObject.FindProperty(propertyName);

        if (listProperty == null)
        {
            Debug.LogError($"Missing serialized property: {propertyName}");
            return createdDefinitions;
        }

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty item = listProperty.GetArrayElementAtIndex(i);
            string displayName = item.FindPropertyRelative("eventName").stringValue;
            string explanation = item.FindPropertyRelative("explaination").stringValue;
            Sprite icon = item.FindPropertyRelative("icon").objectReferenceValue as Sprite;
            GameEventRank rank = (GameEventRank)item.FindPropertyRelative("rank").enumValueIndex;

            GameEventDefinition definition = ScriptableObject.CreateInstance<GameEventDefinition>();
            definition.eventName = displayName;
            definition.explanation = explanation;
            definition.icon = icon;
            definition.rank = rank;
            definition.isSpecialBallEvent = isSpecialBall;
            definition.effects = ParseEffects(item.FindPropertyRelative("unityAction"));

            string safeName = MakeSafeAssetName($"{i:00}_{displayName}");
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{safeName}.asset");
            AssetDatabase.CreateAsset(definition, assetPath);
            createdDefinitions.Add(definition);
        }

        return createdDefinitions;
    }

    private List<GameEventEffect> ParseEffects(SerializedProperty unityActionProperty)
    {
        var effects = new List<GameEventEffect>();
        SerializedProperty calls = unityActionProperty
            .FindPropertyRelative("m_PersistentCalls")
            .FindPropertyRelative("m_Calls");

        for (int i = 0; i < calls.arraySize; i++)
        {
            SerializedProperty call = calls.GetArrayElementAtIndex(i);
            string methodName = call.FindPropertyRelative("m_MethodName").stringValue;
            SerializedProperty args = call.FindPropertyRelative("m_Arguments");

            GameEventEffect effect = CreateEffectFromLegacyCall(methodName, args);
            if (effect != null)
            {
                effects.Add(effect);
            }
        }

        return effects;
    }

    private GameEventEffect CreateEffectFromLegacyCall(string methodName, SerializedProperty args)
    {
        int intArgument = args.FindPropertyRelative("m_IntArgument").intValue;
        float floatArgument = args.FindPropertyRelative("m_FloatArgument").floatValue;

        switch (methodName)
        {
            case nameof(SetBallCountIncrease):
                return new GameEventEffect { effectType = GameEventEffectType.BallCountIncrease, intValue = intArgument };
            case nameof(SetBallCountMultifly):
                return new GameEventEffect { effectType = GameEventEffectType.BallCountMultiply, floatValue = floatArgument };
            case nameof(SetNextSheepHpDouble):
                return new GameEventEffect { effectType = GameEventEffectType.NextSheepHpDouble, intValue = intArgument };
            case nameof(SetFirstLineErease):
                return new GameEventEffect { effectType = GameEventEffectType.FirstLineErase };
            case nameof(SetAllBlockErease):
                return new GameEventEffect { effectType = GameEventEffectType.AllBlockErase };
            case nameof(SetAllBlockDoubleHp):
                return new GameEventEffect { effectType = GameEventEffectType.AllBlockDoubleHp };
            case nameof(SetWaitForTurn):
                return new GameEventEffect { effectType = GameEventEffectType.WaitForTurn, intValue = intArgument };
            case nameof(SetRandomBlockTriple):
                return new GameEventEffect { effectType = GameEventEffectType.RandomBlockTriple, intValue = intArgument };
            case nameof(SetUnderHpErase):
                return new GameEventEffect { effectType = GameEventEffectType.UnderHpErase, intValue = intArgument };
            case nameof(SetRandomBlockErase):
                return new GameEventEffect { effectType = GameEventEffectType.RandomBlockErase, intValue = intArgument };
            case nameof(SetAllBlockDamage):
                return new GameEventEffect { effectType = GameEventEffectType.AllBlockDamage, intValue = intArgument };
            case nameof(SetRandomThreeBlockDamage):
                return new GameEventEffect { effectType = GameEventEffectType.RandomThreeBlockDamage, intValue = intArgument };
            case nameof(SetRandomBlockDoubleHp):
                return new GameEventEffect { effectType = GameEventEffectType.RandomBlockDoubleHp, intValue = intArgument };
            case nameof(SetRandomBlockGenerate):
                return new GameEventEffect { effectType = GameEventEffectType.RandomBlockGenerate, intValue = intArgument };
            case nameof(SetCoinTossCommonBallTo):
                return new GameEventEffect { effectType = GameEventEffectType.CoinTossCommonBallTo, intValue = intArgument };
            case nameof(SetCoinTossSpecialBallTo):
                return new GameEventEffect { effectType = GameEventEffectType.CoinTossSpecialBallTo, intValue = intArgument };
            case nameof(GetCrossBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Cross };
            case nameof(GetBombBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Bomb };
            case nameof(GetVerticalBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Vertical };
            case nameof(GetHorizontalBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Horizontal };
            case nameof(GetSplitBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Split };
            case nameof(GetDrillBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Drill };
            case nameof(GetHollyBall):
                return new GameEventEffect { effectType = GameEventEffectType.GetSpecialBall, ballType = BallType.Holly };
        }

        Debug.LogWarning($"Unsupported legacy event method: {methodName}");
        return null;
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string target = $"{parent}/{folderName}";
        if (!AssetDatabase.IsValidFolder(target))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static string MakeSafeAssetName(string source)
    {
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
        foreach (char invalidChar in invalidChars)
        {
            source = source.Replace(invalidChar, '_');
        }

        return source.Replace('/', '_').Replace('\\', '_').Trim();
    }
#endif
}
