using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GG.Infrastructure.Utils;

public class BlockManager : MonoBehaviour
{
    public Block blockPrefab;
    public GameObject plusItemPrefab, boxItemPrefab;


    public WeightedListOfInts selectiveRandom;

    public Transform blockParent;
    public Block[,] blockGrid = new Block[7, 9];
    //public GameObject[,] plusItemGrid = new GameObject[7, 9]; // plusItem�� ��ġ�� �����ϱ� ���� �迭 �߰�

    public float[] gridX = new float[]{
        -5.6f, -4.65f, -3.72f, -2.78f, -1.86f, -0.94f};
    public float[] gridY = new float[]{
        4.93f, 3.99f, 3.05f, 2.11f, 1.17f, 0.23f, -0.71f, -1.65f};

    private int levelCount = 1;

    public GameObject heartParticle;


    public int doubleSheepHpCount = 0;

    [ContextMenu("SpawnBlock")]
    public void SpawnBlock()
    {
        if (IsGridFull())
        {
            Debug.Log("��� �׸��尡 �� �־� ������ �������� �ʽ��ϴ�.");
            return;
        }

        int xIndex, yIndex;

        do
        {
            xIndex = Random.Range(0, 7);
            yIndex = Random.Range(1, 8);
        }
        while (blockGrid[xIndex, yIndex] != null); // �ش� ��ġ�� ������ ������ �ٽ� ���� ��ġ�� ã��

        var tempBlock = Instantiate(blockPrefab, blockParent);

        tempBlock.GetComponent<Block>().curX = xIndex;
        tempBlock.GetComponent<Block>().curY = yIndex;
        tempBlock.Init(GameLogicManager.instance.currentLevel);
        tempBlock.allBlockBrokenCheck = CheckAllBlockBroken;
        blockGrid[xIndex, yIndex] = tempBlock;
        tempBlock.transform.localPosition = new Vector3(gridX[xIndex], gridY[yIndex], 0);
    }

    [ContextMenu("SpawnPlusItem")]
    public void SpawnPlusItem()
    {
        // plusItemPrefab�� �� �׸��� ��ġ�� ����
        if (IsGridFull())
        {
            Debug.Log("�׸��尡 ���� �� �־� �߰� �������� ������ �� �����ϴ�.");
            return;
        }

        int xIndex, yIndex;

        do
        {
            xIndex = Random.Range(0, 7);
            yIndex = Random.Range(0, 9);
        }
        while (blockGrid[xIndex, yIndex] != null); // �ش� ��ġ�� ������ ������ �ٽ� ���� ��ġ�� ã��

        // �� ���� plusItem ����
        var plusItem = Instantiate(plusItemPrefab, blockParent);

        plusItem.GetComponent<Block>().curX = xIndex;
        plusItem.GetComponent<Block>().curY = yIndex;
        blockGrid[xIndex, yIndex] = plusItem.GetComponent<Block>();
        plusItem.transform.localPosition = new Vector3(gridX[xIndex], gridY[yIndex], 0);
    }

    public void SpawnBoxItem()
    {
        // plusItemPrefab�� �� �׸��� ��ġ�� ����
        if (IsGridFull())
        {
            Debug.Log("�׸��尡 ���� �� �־� �߰� �������� ������ �� �����ϴ�.");
            return;
        }

        int xIndex, yIndex;

        do
        {
            xIndex = Random.Range(0, 7);
            yIndex = Random.Range(0, 9);
        }
        while (blockGrid[xIndex, yIndex] != null); // �ش� ��ġ�� ������ ������ �ٽ� ���� ��ġ�� ã��

        // �� ���� plusItem ����
        var boxItem = Instantiate(boxItemPrefab, blockParent);

        boxItem.GetComponent<Block>().curX = xIndex;
        boxItem.GetComponent<Block>().curY = yIndex;
        blockGrid[xIndex, yIndex] = boxItem.GetComponent<Block>();

        boxItem.transform.localPosition = new Vector3(gridX[xIndex], gridY[yIndex], 0);
    }

    public void SpawnCountBlocks(int count)
    {
        for (int i = 0; i < count; i++)
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
                if (blockGrid[x, y] == null) // ����ִ� �׸��尡 ������ false ��ȯ
                    return false;
            }
        }
        return true; // ��� �׸��尡 �� ������ true ��ȯ
    }

    public void SpawnTopLane(int amount)
    {
        int topRowYIndex = 1; // 위쪽 줄의 yIndex

        // amount를 그리드의 최대 크기를 초과하지 않도록 제한
        amount = Mathf.Min(amount, blockGrid.GetLength(0));

        HashSet<int> usedXIndices = new HashSet<int>(); // 중복된 x좌표 방지용 Set

        // 빈 공간 찾기: 비어있는 칸의 x좌표를 리스트에 추가
        List<int> emptyXIndices = new List<int>();
        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            if (blockGrid[x, topRowYIndex] == null) // 블록이 없는 칸만 추가
            {
                emptyXIndices.Add(x);
            }
        }

        // 1. 우선순위가 높은 plusItemPrefab 생성
        if (emptyXIndices.Count > 0)
        {
            int randomX = emptyXIndices[Random.Range(0, emptyXIndices.Count)];
            emptyXIndices.Remove(randomX); // 해당 위치를 사용했으므로 제거

            var tempItem = Instantiate(plusItemPrefab, blockParent);
            tempItem.GetComponent<Block>().curX = randomX;
            tempItem.GetComponent<Block>().curY = topRowYIndex;
            tempItem.transform.localPosition = new Vector3(gridX[randomX], gridY[topRowYIndex], 0);

            // blockGrid에 추가 (아이템도 블록처럼 처리할 경우에만 활성화)
            blockGrid[randomX, topRowYIndex] = tempItem.GetComponent<Block>();
        }

        // 2. 우선순위가 높은 boxItemPrefab 생성 (currentLevel이 10으로 나누어떨어질 때만)
        if (GameLogicManager.instance.currentLevel % 10 == 0)
        {
            int boxItemCount = 1; // 최대 생성 가능 개수
            for (int i = 0; i < boxItemCount; i++)
            {
                int randomX = emptyXIndices[Random.Range(0, emptyXIndices.Count)];
                emptyXIndices.Remove(randomX); // 해당 위치를 사용했으므로 제거

                var tempBox = Instantiate(boxItemPrefab, blockParent);

                tempBox.transform.localPosition = new Vector3(gridX[randomX], gridY[topRowYIndex], 0);
                tempBox.GetComponent<Block>().curX = randomX;
                tempBox.GetComponent<Block>().curY = topRowYIndex;
                // blockGrid에 추가 (아이템도 블록처럼 처리할 경우에만 활성화)
                blockGrid[randomX, topRowYIndex] = tempBox.GetComponent<Block>();
            }
        }

        // 3. 나머지 공간에 일반 블록(blockPrefab) 생성
        int spawnedCount = 0;
        while (spawnedCount < amount && emptyXIndices.Count > 0)
        {
            int randomX = emptyXIndices[Random.Range(0, emptyXIndices.Count)];
            emptyXIndices.Remove(randomX); // 해당 위치를 사용했으므로 제거

            var tempBlock = Instantiate(blockPrefab, blockParent);
            tempBlock.GetComponent<Block>().curX = randomX;
            tempBlock.GetComponent<Block>().curY = topRowYIndex;
            tempBlock.allBlockBrokenCheck = CheckAllBlockBroken;

            int sheepHp = levelCount;
            if (doubleSheepHpCount > 0)
            {
                sheepHp *= 2;
            }
            tempBlock.Init(sheepHp);
            blockGrid[randomX, topRowYIndex] = tempBlock;

            // 생성된 블록의 위치 설정
            tempBlock.transform.localPosition = new Vector3(gridX[randomX], gridY[topRowYIndex], 0);

            spawnedCount++; // 생성된 블록 수 증가
        }
        if (doubleSheepHpCount > 0) doubleSheepHpCount--;
    }
    public bool CheckGameOver()
    {
        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            if (blockGrid[x, 7] != null && blockGrid[x, 7].blockType == BlockType.Item)
            {
                Destroy(blockGrid[x, 7].gameObject);
            }
        }

        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            if (blockGrid[x, 7] != null) // 블록이 차 있다면
            {
                return true;
                break;
            }
        }
        return false;
    }

    public void BlockGetDown(int level)
    {
        levelCount = level;
        StartCoroutine(BlockGetDownCo());


        selectiveRandom.SetWeightAtIndex(3, selectiveRandom.GetWeightAtIndex(3) * 1.03f);
        selectiveRandom.SetWeightAtIndex(4, selectiveRandom.GetWeightAtIndex(4) * 1.04f);
        selectiveRandom.SetWeightAtIndex(5, selectiveRandom.GetWeightAtIndex(5) * 1.035f);

        selectiveRandom.Normalize();

        Utils.DelayCall(() => SpawnTopLane(selectiveRandom.GetRandomByWeight()), 1f);

        if (CheckGameOver()) GameLogicManager.instance.gameCanvas.ShowGameOver();
    }

    IEnumerator BlockGetDownCo()
    {
        List<Block> blocksToMove = GetAvailableBlocks(true);

        float timeElapsed = 0f;
        List<Vector3> blockStartPositions = new List<Vector3>();
        List<Vector3> blockTargetPositions = new List<Vector3>();

        // 초기 위치와 목표 위치 캐싱
        foreach (var block in blocksToMove)
        {
            int nextY = Mathf.Min(block.curY + 1, blockGrid.GetLength(1) - 1);

            // 초기 위치 및 목표 위치 저장
            blockStartPositions.Add(block.transform.localPosition);
            blockTargetPositions.Add(new Vector3(gridX[block.curX], gridY[nextY], 0));
        }

        // 블록을 부드럽게 이동
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;
            float lerpValue = Mathf.Clamp01(timeElapsed);

            for (int i = 0; i < blocksToMove.Count; i++)
            {
                var block = blocksToMove[i];
                var startPos = blockStartPositions[i];
                var targetPos = blockTargetPositions[i];

                // 초기 위치와 목표 위치를 기준으로 Lerp
                block.transform.localPosition = Vector3.Lerp(startPos, targetPos, lerpValue);
            }

            yield return null;
        }

        // 이동이 끝난 후 위치 확정 및 grid 업데이트
        // 역순으로 탐색하여 이동
        for (int y = blockGrid.GetLength(1) - 2; y >= 0; y--)
        {
            foreach (var block in blocksToMove)
            {
                if (block.curY == y)
                {
                    int nextY = Mathf.Min(block.curY + 1, blockGrid.GetLength(1) - 1);

                    // 그리드 업데이트
                    blockGrid[block.curX, block.curY] = null;
                    blockGrid[block.curX, nextY] = block;
                    block.curY = nextY;

                    // 정확한 위치로 이동
                    block.transform.localPosition = new Vector3(gridX[block.curX], gridY[block.curY], 0);
                }
            }
        }
    }

    public void CheckAllBlockBroken()
    {
        if (GetAvailableBlocks(true).Count == 0)
        {
            GameLogicManager.instance.GetAllBallDown();
            GameLogicManager.instance.gameCanvas.ShowGreat();
        }
    }

    public List<Block> GetAvailableBlocks(bool containItem = false)
    {
        List<Block> availableBlocks = new List<Block>();

        // blockGrid 배열에서 null이 아닌 Block을 찾음
        foreach (Block block in blockGrid)
        {
            if (block != null && (containItem || block.blockType != BlockType.Item))
            {
                availableBlocks.Add(block);
            }
        }


        return availableBlocks;
    }

    public List<Block> GetRandomBlock(int count)
    {
        List<Block> availableBlocks = GetAvailableBlocks(); // ������ ���ϵ� ��������

        // �����ϰ� ������ �� �ִ� Block�� �����ϸ� ������ �͸� ��ȯ
        List<Block> randomBlocks = new List<Block>();
        int numberOfBlocksToSelect = Mathf.Min(count, availableBlocks.Count);

        // ������ Block�� count���� ������ �� �߿��� �����ϰ� ����
        for (int i = 0; i < numberOfBlocksToSelect; i++)
        {
            int randomIndex = Random.Range(0, availableBlocks.Count);
            randomBlocks.Add(availableBlocks[randomIndex]);
            availableBlocks.RemoveAt(randomIndex); // �̹� ������ Block�� ����Ʈ���� ����
        }

        return randomBlocks;
    }
    public List<Block> GetBlocksWithCountLessThan(int value)
    {
        List<Block> availableBlocks = GetAvailableBlocks(); // ������ ���ϵ� ��������
        List<Block> filteredBlocks = new List<Block>();

        foreach (Block block in availableBlocks)
        {
            // Block�� count�� value���� ������ Ȯ��
            if (block.Count < value)
            {
                filteredBlocks.Add(block);
            }
        }

        return filteredBlocks;
    }

    public List<Block> GetBlocksInSameYLineWithMaxY()
    {
        // ���� ū y���� ã�� ���� ����
        int maxY = int.MinValue;
        Block maxYBlock = null;

        // ���� ū y���� ���� Block�� ã�´�
        for (int x = 0; x < blockGrid.GetLength(0); x++)  // x ���� ��ȸ
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++)  // y ���� ��ȸ
            {
                Block currentBlock = blockGrid[x, y];

                // Null�� �ƴϰ�, y ���� �� ũ�� ����
                if (currentBlock != null && y > maxY)
                {
                    maxY = y;
                    maxYBlock = currentBlock;
                }
            }
        }

        // ���� ��ȿ�� Block�� ������ �� ����Ʈ ��ȯ
        if (maxYBlock == null)
        {
            return new List<Block>();
        }

        // �ִ� y ���� ���� Block�� y ��
        int targetY = -1;
        for (int x = 0; x < blockGrid.GetLength(0); x++)  // x ���� ��ȸ
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++)  // y ���� ��ȸ
            {
                if (blockGrid[x, y] == maxYBlock)
                {
                    targetY = y;
                    break;
                }
            }
            if (targetY != -1) break;  // y �� ã���� �ݺ��� ����
        }

        // ���� targetY�� ���� y ���� ���� Block�� ��ȯ
        List<Block> blocksInSameYLine = new List<Block>();
        for (int x = 0; x < blockGrid.GetLength(0); x++)  // x ���� ��ȸ
        {
            Block blockInSameLine = blockGrid[x, targetY];
            if (blockInSameLine != null)
            {
                blocksInSameYLine.Add(blockInSameLine);
            }
        }

        return blocksInSameYLine;
    }

    public void IncreaseHardLevelWeigh()
    {

    }
    public void RemoveBlock(Block target)
    {
        for (int x = 0; x < blockGrid.GetLength(0); x++) // x 방향 순회
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++) // y 방향 순회
            {
                if (blockGrid[x, y] == target) // Block이 일치하는 경우
                {
                    blockGrid[x, y] = null; // 해당 위치의 Block 제거
                    //Debug.Log($"Block removed at position ({x}, {y}).");

                }
            }
        }
    }

    [ContextMenu("Debug")]
    public void DebugBlockGrid()
    {
        string debugOutput = ""; // 출력할 문자열 초기화

        // y값이 작은 순서부터 확인
        for (int y = 0; y < blockGrid.GetLength(1); y++)
        {
            for (int x = 0; x < blockGrid.GetLength(0); x++)
            {
                // 현재 좌표의 블록 null 여부 확인
                string status = blockGrid[x, y] == null ? "n" : "f";
                // 좌표와 상태를 문자열로 추가
                debugOutput += $"({x},{y}): {status}  ";
            }
            // y 값마다 줄바꿈 추가
            debugOutput += "\n";
        }

        // 최종 출력
        Debug.Log(debugOutput);
    }

    public List<Block> GetBlocksInCross(Block block)
    {
        List<Block> blocks = new List<Block>();
        int x = block.curX; // Block의 X 좌표
        int y = block.curY; // Block의 Y 좌표

        // 상, 하, 좌, 우 블록 추가
        if (IsValidPosition(x, y + 1)) blocks.Add(blockGrid[x, y + 1]); // 상
        if (IsValidPosition(x, y - 1)) blocks.Add(blockGrid[x, y - 1]); // 하
        if (IsValidPosition(x - 1, y)) blocks.Add(blockGrid[x - 1, y]); // 좌
        if (IsValidPosition(x + 1, y)) blocks.Add(blockGrid[x + 1, y]); // 우

        return blocks;
    }


    public List<Block> GetBlocksInSameColumn(Block block)
    {
        List<Block> blocks = new List<Block>();
        int x = block.curX;

        // 같은 X 좌표의 모든 블록 추가
        for (int y = 0; y < blockGrid.GetLength(1); y++)
        {
            if (blockGrid[x, y] != null) blocks.Add(blockGrid[x, y]);
        }

        return blocks;
    }

    public List<Block> GetBlocksInSameRow(Block block)
    {
        List<Block> blocks = new List<Block>();
        int y = block.curY;

        // 같은 Y 좌표의 모든 블록 추가
        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            if (blockGrid[x, y] != null) blocks.Add(blockGrid[x, y]);
        }

        return blocks;
    }

    public List<Block> GetAdjacentBlocks(Block block)
    {
        List<Block> blocks = new List<Block>();
        int x = block.curX;
        int y = block.curY;

        // 8방향의 블록 추가
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // 자기 자신 제외
                if (IsValidPosition(x + dx, y + dy))
                {
                    blocks.Add(blockGrid[x + dx, y + dy]);
                }
            }
        }

        return blocks;
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < blockGrid.GetLength(0) && y >= 0 && y < blockGrid.GetLength(1) && blockGrid[x, y] != null;
    }
}

