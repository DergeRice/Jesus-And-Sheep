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
    public int currentLevel, doubleCount;
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

        gameLogicManager.gameDataSaveManager = this;
        PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.hasLastGame, true);

        if(GameManager.instance.isContinueMode)
        {
            LoadGame();
            LoadDataApply(GameManager.instance.isLastGameSpeed);
        }
    }

    [ContextMenu("SaveJunSick")]
    public void SaveGame()
    {
        blockGrid = gameLogicManager.blockManager.blockGrid;
        percentOfBrickLevel = gameLogicManager.blockManager.SavePercentOfBlock();
        junsick = ConvertBlocksToList(blockGrid);
        currentLevel = gameLogicManager.currentLevel;
        ballTypes = gameLogicManager.shootingBallDatas.ToArray();
        currentPos = gameLogicManager.jesus.transform.position;
        doubleCount = gameLogicManager.blockManager.doubleSheepHpCount;

        saveData = new SaveData
        {
            blockDatas = ConvertBlocksToList(blockGrid),
            percentOfBlockLevel = percentOfBrickLevel,
            IsGameOver = IsGameOver,
            currentLevel = currentLevel,
            ballTypes = ballTypes,
            currentPos = new SerializableVector3(this.currentPos),
            nextDoubleCount = doubleCount
            
        };

        // 블록 상태는 JSON으로 저장
        string json = JsonUtility.ToJson(saveData, true);        
        string encryptedJson = Utils.Encrypt(json, encryptionKey);

        File.WriteAllText(savePath, encryptedJson);
        
        // 공 던진 여부는 PlayerPrefs에 저장
        SaveShotStatus();

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
        string json = Utils.Decrypt(encryptedJson, encryptionKey);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // 블록 상태와 관련된 데이터 로드
        junsick = saveData.blockDatas;
        ballTypes = saveData.ballTypes;
        percentOfBrickLevel = saveData.percentOfBlockLevel;
        IsGameOver = saveData.IsGameOver;
        currentLevel = saveData.currentLevel;
        doubleCount = saveData.nextDoubleCount;

        // 공 던진 여부를 PlayerPrefs에서 로드
        IsShot = PlayerPrefs.GetInt("IsShot", 0) == 1;

        if(IsShot)
        {
            LoadShotStatus();
        }else
        {
            currentPos =  saveData.currentPos.ToVector3();
        }

        Debug.Log("Game Loaded Successfully.");
    }

    [ContextMenu("Umpply Game")]
    public void LoadDataApply(bool isSpeedMode)
    {
        if (IsGameOver == true)
        {
            Debug.Log("already finished game");
        }

        if (isSpeedMode == true)
        {
            Time.timeScale = 2f;
        }

        gameLogicManager.blockManager.ReGenerateBlocks(junsick);

        gameLogicManager.shootingBallDatas = ballTypes.ToList();
        gameLogicManager.currentLevel = currentLevel;
        gameLogicManager.blockManager.doubleSheepHpCount = doubleCount;

        for (int i = 0; i < percentOfBrickLevel.Length; i++)
        {
            gameLogicManager.blockManager.selectiveRandom.SetWeightAtIndex(i, percentOfBrickLevel[i]);
        }

        gameLogicManager.jesus.transform.position = currentPos;

        // 공 던졌으면 해당 벡터로 공을 던짐
        if (IsShot)
        {
            gameLogicManager.StartShoot(targetVector);
            // Utils.DelayCall( ()=>{},0.1f);
        }
        gameLogicManager.gameCanvas.SetBackImgsSetting(gameLogicManager.currentLevel);
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

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // ��׶���� ���� �� ������ ����
            SaveShotStatus();
        }
    }

    public void SaveShotStatus()
    {
        // 공 던졌는지 여부를 PlayerPrefs에 저장
        IsShot = gameLogicManager.isShot;
        PlayerPrefs.SetInt("IsShot", IsShot ? 1 : 0);
        PlayerPrefs.Save();
        
        ShootData data = new ShootData();
        data.TargetVector = new SerializableVector3(gameLogicManager.launchDirection);
        data.CurrentPos = new SerializableVector3(gameLogicManager.jesus.transform.position);
        
        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(Application.persistentDataPath, "ShootData.json");

        File.WriteAllText(filePath, json);
        Debug.Log($"Ball data saved: {filePath}");

    }


    public void LoadShotStatus()
    {
        // 저장된 공 던졌는지 여부를 불러오기
        string filePath = Path.Combine(Application.persistentDataPath, "ShootData.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            ShootData data = JsonUtility.FromJson<ShootData>(json);

            targetVector = data.TargetVector.ToVector3();
            currentPos = data.CurrentPos.ToVector3();

            Debug.Log("Ball data loaded successfully");
        }
        gameLogicManager.jesus.transform.position = currentPos;
        Utils.DelayCall(()=>gameLogicManager.StartShoot(targetVector),0.2f);
    }


    private void OnApplicationQuit()
    {
        // �� ���� �� ������ ����
        SaveShotStatus();
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
