using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using GG.Infrastructure.Utils;

public class GameDataSaveManager : MonoBehaviour
{
    GameLogicManager gameLogicManager;

    [SerializeField]
    public List<List<BlockData>> blockList = new List<List<BlockData>>();

    public Block[,] blockGrid; // 저장할 블록 데이터
    public float[] percentOfBrickLevel;
    public Vector3 currentPos;
    public Vector3 targetVector;
    public bool IsGameOver;
    public bool IsShot;
    public int currentLevel;
    public BallType[] ballTypes;

    private string savePath;
    private readonly string encryptionKey = "U8hHV9ksC8Q7sXvmlTjXfi5z0fbMmcg3"; // 16, 24, or 32 bytes

    private void Awake()
    {
        // 저장 파일 경로 설정
        savePath = Path.Combine(Application.persistentDataPath, "SavedGameData.json");
        Debug.Log($"Save path: {savePath}");
        Application.quitting += OnApplicationQuit;
    }
    private void Start()
    {
        gameLogicManager = GameLogicManager.instance; // 나중에 고쳐야함
    }

    [ContextMenu("SaveJunSick")]
    public void SaveGame()
    {
        blockGrid = gameLogicManager.blockManager.blockGrid;
        percentOfBrickLevel = gameLogicManager.blockManager.SavePercentOfBlock();
        currentPos = gameLogicManager.jesus.transform.position;
        targetVector = gameLogicManager.launchDirection;
        IsGameOver = gameLogicManager.isGameOver;
        IsShot = gameLogicManager.isShot;
        currentLevel = gameLogicManager.currentLevel;
        ballTypes = gameLogicManager.shootingBallDatas.ToArray();

        SaveData saveData = new SaveData
        {
            blockDatas = ConvertBlocksToList(blockGrid),
            percentOfBlockLevel = percentOfBrickLevel,
            currentPos = new SerializableVector3(currentPos),
            targetVector = new SerializableVector3(targetVector),
            IsGameOver = IsGameOver,
            IsShot = IsShot,
            currentLevel = currentLevel,
            ballTypes = ballTypes
            
        };

        string json = JsonUtility.ToJson(saveData, true);        // 파일로 저장
        string encryptedJson = Encrypt(json, encryptionKey);

        Debug.Log(json);

        File.WriteAllText(savePath, encryptedJson);
       
        Debug.Log("Game Saved Successfully.");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found!");
            return;
        }

        string encryptedJson = File.ReadAllText(savePath);
        string json = Decrypt(encryptedJson, encryptionKey);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);


        // 블록 데이터를 복구
        blockGrid = ConvertListToBlocks(saveData.blockDatas);

        percentOfBrickLevel = saveData.percentOfBlockLevel;
        currentPos = saveData.currentPos.ToVector3();
        targetVector = saveData.targetVector.ToVector3();
        IsGameOver = saveData.IsGameOver;
        IsShot = saveData.IsShot;
        currentLevel = saveData.currentLevel;

        Debug.Log("Game Loaded Successfully.");
    }

    private BlockData[] ConvertBlocksToList(Block[,] blockGrid)
    {
        List<BlockData> blockList = new List<BlockData>();
        for (int y = 0; y < blockGrid.GetLength(1); y++)
        {
            for (int x = 0; x < blockGrid.GetLength(0); x++)
            {
                var block = blockGrid[x, y];
                if (block != null)
                {
                    blockList.Add(new BlockData(block));
                }
            }
        }
        return blockList.ToArray(); // 1D 배열로 반환
    }

    private Block[,] ConvertListToBlocks(BlockData[] blockList)
    {
        int width = 7;
        int height = 9;

        var blockGrid = new Block[width, height];

        int index = 0; // 1D 배열을 다루기 위한 인덱스

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var blockData = blockList[index];
                if (blockData != null)
                {
                    // Block 인스턴스 생성 및 초기화
                    Block block = new Block
                    {
                        Count = blockData.count,
                        countMax = blockData.countMax,
                        blockType = Enum.Parse<BlockType>(blockData.blockType),
                        curX = blockData.curX,
                        curY = blockData.curY
                    };
                    //init 땡기기
                    // 해당 위치에 Block 할당
                    blockGrid[x, y] = block;
                }

                index++; // 배열 인덱스 증가
            }
        }
        return blockGrid;
    }

    private string Encrypt(string plainText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16]; // Initialization vector set to 0 for simplicity

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string Decrypt(string cipherText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16]; // Same IV used in encryption

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 백그라운드로 갔을 때 데이터 저장
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        // 앱 종료 시 데이터 저장
        SaveGame();
    }
}
