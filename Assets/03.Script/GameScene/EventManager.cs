using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using RandomElementsSystem.Types;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;
    GameLogicManager gameLogicManager;

    public SelectiveRandomWeightInt selectiveRandom;

    public List<GameEvent> gameEvents = new List<GameEvent>();

    bool currentEventSuccess;
    public List<UnityEvent> failActionList = new List<UnityEvent>();

    private void Start()
    {
        gameLogicManager = GameLogicManager.instance;

    }
    [ContextMenu("GoRandom")]
    public void ShowTwoSelection()
    {
        var randomValue = selectiveRandom.GetRandomValue();
        int secondValue = 0;

        for (int i = 0; i < 100; i++)
        {
            secondValue = selectiveRandom.GetRandomValue();

            if (secondValue != randomValue)
            {
                gameLogicManager.ShowSelectPanel(gameEvents[randomValue], gameEvents[secondValue]);
                break;
            }
        }

    }


    public void SetBallCountIncrease(int amount)
    {
        gameLogicManager.ballCount += amount;

        if (gameLogicManager.ballCount < 0) gameLogicManager.ballCount = 1;
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
            yield return new WaitForSeconds(0.1f);
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
            gameLogicManager.blockManager.SpawnBlock();
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





}
