using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using RandomElementsSystem.Types;
using GG.Infrastructure.Utils;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;
    GameLogicManager gameLogicManager;

    public WeightedListOfInts eventRank;

    public List<GameEvent> gameEvents = new List<GameEvent>();
     List<GameEvent> gameLegendEvents = new List<GameEvent>();
     List<GameEvent> gameEpicEvents = new List<GameEvent>();
     List<GameEvent> gameCommonEvents = new List<GameEvent>();
    public List<GameEvent> specialBallEvents = new List<GameEvent>();

    bool currentEventSuccess;
    public List<UnityEvent> failActionList = new List<UnityEvent>();

    private void Start()
    {
        gameLogicManager = GameLogicManager.instance;
        PopulateEventLists();
    }
    private void PopulateEventLists()
    {
        foreach (var gameEvent in gameEvents)
        {
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
        // Determine the event list based on the weighted rank
        List<GameEvent> selectedList = null;
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
            Debug.LogWarning("No events available for the selected rank!");
            return;
        }

        // Get unique random events from the selected list
        int firstValue = Random.Range(0, selectedList.Count);
        int secondValue = GetUniqueRandomIndex(selectedList.Count, firstValue);

        if (Random.value < 0.90f) // 90%
        {
            // Show a special ball event along with two random events
            gameLogicManager.ShowSelectPanel(
                specialBallEvents[Random.Range(0, specialBallEvents.Count)],
                selectedList[firstValue],
                selectedList[secondValue]
            );
            return;
        }

        // If not 90%, include a third unique event
        int thirdValue = GetUniqueRandomIndex(selectedList.Count, firstValue, secondValue);
        gameLogicManager.ShowSelectPanel(
            selectedList[firstValue],
            selectedList[secondValue],
            selectedList[thirdValue]
        );
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

        if (gameLogicManager.GetBallCount < 0) gameLogicManager.ballCount = 1;
    }
    public void SetBallCountMultifly(float amount)
    {
        gameLogicManager.ballCount = (int)(gameLogicManager.ballCount * amount);
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
        for (int i = 0; i < turnAmount; i++)
        {
            gameLogicManager.AllBallComeDown();
            gameLogicManager.isPlayerTurn = false;
            yield return gameLogicManager.isPlayerTurn == true;
            yield return new WaitForSeconds(2f);
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
            gameLogicManager.blockManager.SpawnRandomBlock();
        }
    }

    public void SetCoinTossCommonBallTo(int failActionIndex)
    {
        currentEventSuccess = Random.value > 0.5f;

        if (currentEventSuccess) SetBallCountIncrease(5);
        else failActionList[failActionIndex].Invoke();
    }

    public void SetCoinTossSpecialBallTo(int failActionIndex)
    {
        currentEventSuccess = Random.value > 0.5f;

        if (currentEventSuccess) gameLogicManager.GetRandomSpecialBall(5);
        else failActionList[failActionIndex].Invoke();
    }


    public void GetCrossBall() { gameLogicManager.GetSpecialBall(BallType.Cross); }
    public void GetBombBall() { gameLogicManager.GetSpecialBall(BallType.Bomb); }
    public void GetVerticalBall() { gameLogicManager.GetSpecialBall(BallType.Vertical); }
    public void GetHorizontalBall() { gameLogicManager.GetSpecialBall(BallType.Horizontal); }
    public void GetSplitBall() { gameLogicManager.GetSpecialBall(BallType.Split); }
    public void GetDrillBall() { gameLogicManager.GetSpecialBall(BallType.Drill); }
    public void GetHollyBall() { gameLogicManager.GetSpecialBall(BallType.Holly); }


}
