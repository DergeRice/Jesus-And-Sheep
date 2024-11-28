using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using RandomElementsSystem.Types;

public class BlockManager : MonoBehaviour
{
    public Block blockPrefab;
    public GameObject plusItemPrefab, boxItemPrefab;


    public MinMaxRandomInt selectiveRandom;

    public Transform blockParent;
    public Block[,] blockGrid = new Block[7, 9];
    public GameObject[,] plusItemGrid = new GameObject[7, 9]; // plusItem�� ��ġ�� �����ϱ� ���� �迭 �߰�

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
        tempBlock.Init(GameLogicManager.instance.currentLevel);
        tempBlock.ballCollsionEffect = SpawnHeartParticle;
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
        plusItemGrid[xIndex, yIndex] = plusItem;
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
        plusItemGrid[xIndex, yIndex] = boxItem;
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
            tempBlock.ballCollsionEffect = SpawnHeartParticle;
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
        Utils.DelayCall(() => SpawnTopLane(Random.Range(2, 6)), 1f);

        if (CheckGameOver()) GameLogicManager.instance.gameCanvas.ShowGameOver();
    }

    IEnumerator BlockGetDownCo()
    {
        List<Block> blocksToMove = new List<Block>();
        // List<GameObject> itemsToMove = new List<GameObject>(); // plusItem�� ���� ����Ʈ �߰�

        for (int x = 0; x < blockGrid.GetLength(0); x++)
        {
            for (int y = 0; y < blockGrid.GetLength(1); y++)
            {
                if (blockGrid[x, y] != null)
                {
                    blocksToMove.Add(blockGrid[x, y]);
                }

                // if (plusItemGrid[x, y] != null)
                // {
                //     itemsToMove.Add(plusItemGrid[x, y]);
                // }
            }
        }

        float timeElapsed = 0f;
        List<Vector3> blockTargetPositions = new List<Vector3>();
        // List<Vector3> itemTargetPositions = new List<Vector3>(); // plusItem�� ��ǥ ��ġ ����Ʈ

        foreach (var block in blocksToMove)
        {
            int currentY = Array.IndexOf(gridY, block.transform.localPosition.y);
            int nextY = Mathf.Min(currentY + 1, blockGrid.GetLength(1) - 1);
            blockTargetPositions.Add(new Vector3(block.transform.localPosition.x, gridY[nextY], 0));
        }

        // foreach (var item in itemsToMove)
        // {
        //     int currentY = Array.IndexOf(gridY, item.transform.localPosition.y);
        //     int nextY = Mathf.Min(currentY + 1, plusItemGrid.GetLength(1) - 1);
        //     itemTargetPositions.Add(new Vector3(item.transform.localPosition.x, gridY[nextY], 0));
        // }

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

            // for (int i = 0; i < itemsToMove.Count; i++)
            // {
            //     var item = itemsToMove[i];
            //     var targetPos = itemTargetPositions[i];
            //     item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, targetPos, lerpValue);
            // }

            yield return null;
        }

        // �׸��� ���� ����
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

                // if (plusItemGrid[x, y] != null)
                // {
                //     int nextY = y + 1;
                //     plusItemGrid[x, nextY] = plusItemGrid[x, y];
                //     plusItemGrid[x, y] = null;
                //     plusItemGrid[x, nextY].transform.localPosition = new Vector3(gridX[x], gridY[nextY], 0);
                // }
            }
        }
    }

    public void SpawnHeartParticle(Vector3 pos)
    {
        Instantiate(heartParticle, pos, Quaternion.identity);
    }

    public void CheckAllBlockBroken()
    {
        if (GetAvailableBlocks().Count == 1)
        {
            GameLogicManager.instance.GetAllBallDown();
            GameLogicManager.instance.gameCanvas.ShowGreat();
        }
    }

    public List<Block> GetAvailableBlocks()
    {
        List<Block> availableBlocks = new List<Block>();

        // blockGrid �迭���� null�� �ƴ� Block�� ã��
        foreach (Block block in blockGrid)
        {
            if (block != null && block.blockType != BlockType.Item)
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
                    Debug.Log($"Block removed at position ({x}, {y}).");

                }
            }
        }
    }
}

