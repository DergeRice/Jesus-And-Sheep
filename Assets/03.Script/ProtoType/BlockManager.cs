using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockManager : MonoBehaviour
{
    public Block blockPrefab;
    public GameObject plusItemPrefab;

    public Transform blockParent;
    public Block[,] blockGrid = new Block[7, 9];
    public GameObject[,] plusItemGrid = new GameObject[7,9]; // plusItem의 위치를 추적하기 위한 배열 추가

    public float[] gridX = new float[]{
        -5.6f, -4.65f, -3.72f, -2.78f, -1.86f, -0.94f};
    public float[] gridY = new float[]{
        4.93f, 3.99f, 3.05f, 2.11f, 1.17f, 0.23f, -0.71f, -1.65f};

    private int levelCount = 1;

    

    [ContextMenu("SpawnBlock")]
    public void SpawnBlock()
    {
        if (IsGridFull())
        {
            Debug.Log("모든 그리드가 차 있어 블록을 생성하지 않습니다.");
            return;
        }

        int xIndex, yIndex;

        do
        {
            xIndex = Random.Range(0, 7);
            yIndex = Random.Range(0, 9);
        }
        while (blockGrid[xIndex, yIndex] != null); // 해당 위치에 블록이 있으면 다시 랜덤 위치를 찾음

        var tempBlock = Instantiate(blockPrefab, blockParent);
        tempBlock.Init(30);
        blockGrid[xIndex, yIndex] = tempBlock;
        tempBlock.transform.localPosition = new Vector3(gridX[xIndex], gridY[yIndex], 0);
    }

    [ContextMenu("SpawnPlusItem")]
    public void SpawnPlusItem()
    {
        // plusItemPrefab을 빈 그리드 위치에 생성
        if (IsGridFull())
        {
            Debug.Log("그리드가 가득 차 있어 추가 아이템을 생성할 수 없습니다.");
            return;
        }

        int xIndex, yIndex;

        do
        {
            xIndex = Random.Range(0, 7);
            yIndex = Random.Range(0, 9);
        }
        while (blockGrid[xIndex, yIndex] != null); // 해당 위치에 블록이 있으면 다시 랜덤 위치를 찾음

        // 빈 곳에 plusItem 생성
        var plusItem = Instantiate(plusItemPrefab, blockParent);
        plusItemGrid[xIndex, yIndex] = plusItem;
        plusItem.transform.localPosition = new Vector3(gridX[xIndex], gridY[yIndex], 0);
    }

    [ContextMenu("Spawn5Block")]
    public void Spawn5Blocks()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnBlock();
        }
    }

    public void Start()
    {
        SpawnTopLane(2);
    }

    private bool IsGridFull()
    {
        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++)
            {
                if (blockGrid[x, y] == null) // 비어있는 그리드가 있으면 false 반환
                    return false;
            }
        }
        return true; // 모든 그리드가 차 있으면 true 반환
    }

    public void SpawnTopLane(int amount)
    {
        int topRowYIndex = 1; // 맨 윗줄의 yIndex

        // amount가 그리드의 가로 크기를 초과하지 않도록 제한
        amount = Mathf.Min(amount, blockGrid.GetLength(0));

        int spawnedCount = 0;
        HashSet<int> usedXIndices = new HashSet<int>(); // 중복된 x값을 방지하기 위한 Set

        while (spawnedCount < amount)
        {
            // 랜덤한 x좌표 선택
            int x = Random.Range(0, blockGrid.GetLength(0));

            // 이미 해당 x좌표에 블록이 있으면 건너뛰기
            if (blockGrid[x, topRowYIndex] != null || usedXIndices.Contains(x))
                continue;

            usedXIndices.Add(x); // 사용한 x좌표를 기록

            // 해당 위치에 블록 생성
            var tempBlock = Instantiate(blockPrefab, blockParent);
            tempBlock.Init(levelCount);
            blockGrid[x, topRowYIndex] = tempBlock;

            // 블록 위치 설정
            tempBlock.transform.localPosition = new Vector3(gridX[x], gridY[topRowYIndex], 0);

            spawnedCount++; // 생성된 블록 수 증가
        }

        // 아이템 추가: 빈 칸을 찾고, 랜덤한 위치에 한 개 생성
        List<int> emptyXIndices = new List<int>();

        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            if (blockGrid[x, topRowYIndex] == null) // 비어 있는 칸만 수집
            {
                emptyXIndices.Add(x);
            }
        }

        if (emptyXIndices.Count > 0)
        {
            // 빈 칸 중 랜덤한 위치 선택
            int randomX = emptyXIndices[Random.Range(0, emptyXIndices.Count)];

            // 아이템 생성
            var tempItem = Instantiate(plusItemPrefab, blockParent);
            tempItem.transform.localPosition = new Vector3(gridX[randomX], gridY[topRowYIndex], 0);

            //blockGrid에 추가하려면 아래 코드 활성화(아이템이 이동 대상인 경우 필요)
             blockGrid[randomX, topRowYIndex] = tempItem.GetComponent<Block>();
        }
    }

    public void BlockGetDown(int level)
    {
        levelCount = level;
        StartCoroutine(BlockGetDownCo());
        Utils.DelayCall(() => SpawnTopLane(Random.Range(1, 5)), 1f);
    }

    IEnumerator BlockGetDownCo()
    {
        List<Block> blocksToMove = new List<Block>();
        List<GameObject> itemsToMove = new List<GameObject>(); // plusItem을 위한 리스트 추가

        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++)
            {
                if (blockGrid[x, y] != null)
                {
                    blocksToMove.Add(blockGrid[x, y]);
                }

                if (plusItemGrid[x, y] != null)
                {
                    itemsToMove.Add(plusItemGrid[x, y]);
                }
            }
        }

        float timeElapsed = 0f;
        List<Vector3> blockTargetPositions = new List<Vector3>();
        List<Vector3> itemTargetPositions = new List<Vector3>(); // plusItem의 목표 위치 리스트

        foreach (var block in blocksToMove)
        {
            int currentY = Array.IndexOf(gridY, block.transform.localPosition.y);
            int nextY = Mathf.Min(currentY + 1, blockGrid.GetLength(1) - 1);
            blockTargetPositions.Add(new Vector3(block.transform.localPosition.x, gridY[nextY], 0));
        }

        foreach (var item in itemsToMove)
        {
            int currentY = Array.IndexOf(gridY, item.transform.localPosition.y);
            int nextY = Mathf.Min(currentY + 1, plusItemGrid.GetLength(1) - 1);
            itemTargetPositions.Add(new Vector3(item.transform.localPosition.x, gridY[nextY], 0));
        }

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;
            float lerpValue = Mathf.Clamp01(timeElapsed);

            for (int i = 0; i < blocksToMove.Count; i++)
            {
                var block = blocksToMove[i];
                var targetPos = blockTargetPositions[i];
                block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, targetPos, lerpValue);
            }

            for (int i = 0; i < itemsToMove.Count; i++)
            {
                var item = itemsToMove[i];
                var targetPos = itemTargetPositions[i];
                item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, targetPos, lerpValue);
            }

            yield return null;
        }

        // 그리드 상태 갱신
        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            for (int y = blockGrid.GetLength(1) - 2; y >= 0; y--)
            {
                if (blockGrid[x, y] != null)
                {
                    int nextY = y + 1;
                    blockGrid[x, nextY] = blockGrid[x, y];
                    blockGrid[x, y] = null;
                    blockGrid[x, nextY].transform.localPosition = new Vector3(gridX[x], gridY[nextY], 0);
                }

                if (plusItemGrid[x, y] != null)
                {
                    int nextY = y + 1;
                    plusItemGrid[x, nextY] = plusItemGrid[x, y];
                    plusItemGrid[x, y] = null;
                    plusItemGrid[x, nextY].transform.localPosition = new Vector3(gridX[x], gridY[nextY], 0);
                }
            }
        }
    }
}
