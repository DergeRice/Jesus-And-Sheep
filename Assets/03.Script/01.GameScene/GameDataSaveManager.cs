using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using GG.Infrastructure.Utils;
using System.Linq;

public class GameDataSaveManager : MonoBehaviour
{
    GameLogicManager gameLogicManager;

    [SerializeField]
    public List<List<BlockData>> blockList = new List<List<BlockData>>();

    public Block[,] blockGrid; // ������ ���� ������
    public float[] percentOfBrickLevel;
    public Vector3 currentPos;
    public Vector3 targetVector;
    public bool IsGameOver;
    public bool IsShot;
    public int currentLevel;
    public BallType[] ballTypes;

    public BlockData[] junsick;

    private string savePath;
    private readonly string encryptionKey = "U8hHV9ksC8Q7sXvmlTjXfi5z0fbMmcg3"; // 16, 24, or 32 bytes

    SaveData saveData;
    private void Awake()
    {
        // ���� ���� ��� ����
        savePath = Path.Combine(Application.persistentDataPath, "SavedGameData.json");
        Debug.Log($"Save path: {savePath}");
        Application.quitting += OnApplicationQuit;
    }
    private void Start()
    {
        gameLogicManager = GameLogicManager.instance; // have to fix out later
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

        junsick = ConvertBlocksToList(blockGrid);

        saveData = new SaveData
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

        string json = JsonUtility.ToJson(saveData, true);        
        string encryptedJson = Encrypt(json, encryptionKey);

        Debug.Log(json);

        File.WriteAllText(savePath, encryptedJson);
       
        Debug.Log("Game Saved Successfully.");
    }
    

    [ContextMenu("LoadJunSick")]
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

        // gameLogicManager.blockManager.blockGrid = blockGrid;
        junsick  = saveData.blockDatas;
        ballTypes = saveData.ballTypes;
        percentOfBrickLevel = saveData.percentOfBlockLevel;
        currentPos = saveData.currentPos.ToVector3();
        targetVector = saveData.targetVector.ToVector3();
        IsGameOver = saveData.IsGameOver;
        IsShot = saveData.IsShot;
        currentLevel = saveData.currentLevel;

        Debug.Log("Game Loaded Successfully.");
    }

    [ContextMenu("Umpply Game")]
    public void LoadDataApply()
    {
        if(IsGameOver == true)
        {
            Debug.Log("already finished game");
        }

        gameLogicManager.blockManager.ReGenerateBlocks(junsick);

        gameLogicManager.shootingBallDatas = ballTypes.ToList();
        gameLogicManager.currentLevel = currentLevel;

        for (int i = 0; i < percentOfBrickLevel.Length; i++)
        {
            gameLogicManager.blockManager.selectiveRandom.SetWeightAtIndex(i,percentOfBrickLevel[i]);
        }

        gameLogicManager.jesus.transform.position = currentPos;

        if(IsShot)
        {
            gameLogicManager.StartShoot(targetVector);
        }
    }

    private BlockData[] ConvertBlocksToList(Block[,] blockGrid)
    {
        List<BlockData> blockList = new List<BlockData>();

        int width = blockGrid.GetLength(0);
        int height = blockGrid.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Block block = blockGrid[x, y];
                if (block != null) // null이 아닌 경우만 추가
                {
                    var um = new BlockData(block);
                    blockList.Add(um);
                }
            }
        }

        return blockList.ToArray(); // 1D 배열로 변환
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
            // ��׶���� ���� �� ������ ����
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        // �� ���� �� ������ ����
        SaveGame();
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

}
