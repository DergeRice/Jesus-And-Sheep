using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

public class FriendManager : MonoBehaviour
{
    public FriendUi friendUiPrefab;
    public Transform friendUiRoot;
    public Button addFriendBtn;
    public GameObject addFriendPanel;

    private List<User> userList = new List<User>();
    private const string SaveFileName = "FriendData.json";

    private void Awake()
    {
       GameManager.instance.friendManager = this;
        addFriendBtn.onClick.AddListener(()=>addFriendPanel.SetActive(true));
    }
    private void Start()
    {
        LoadData();
    }

    public void MakeFriendUiList(List<User> users)
    {
        ClearExistingUIs();

        for (int i = 0; i < users.Count; i++)
        {
            var ui = Instantiate(friendUiPrefab.gameObject, friendUiRoot).GetComponent<FriendUi>();
            ui.Init(users[i]);
        }

        userList = users;
    }

    private void ClearExistingUIs()
    {
        foreach (Transform child in friendUiRoot)
        {
            Destroy(child.gameObject);
        }
    }
    public void SaveSendState(string name, DateTime time)
    {
        for (int i = 0; i < userList.Count; i++)
        {
            if (userList[i].nickname == name)
            {
                userList[i].lastSendTime = time;
                SaveData(); // 데이터 저장 추가
                return;
            }
        }
    }


    public void SaveData()
    {
        string json = JsonConvert.SerializeObject(userList, Formatting.Indented);
        File.WriteAllText(GetSaveFilePath(), json);
    }

    public void LoadData()
    {
        string path = GetSaveFilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            userList = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            MakeFriendUiList(userList);
        }
        else
        {
            Debug.LogWarning("Save file not found, initializing with an empty list.");
            userList = new List<User>();
        }
    }
    private string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    public void AddFriend(string nickName)
    {
        User user = new User();
        user.nickname = nickName;
        userList.Add(user);
        SaveData();
        LoadData();
    }

    public bool CanSendHeart(string nickName)
    {
        for (int i = 0; i < userList.Count; i++)
        {
            if (userList[i].nickname == nickName)
            {
                TimeSpan timeSinceLastSend = DateTime.Now - userList[i].lastSendTime;
                return timeSinceLastSend.TotalHours >= 6; // 6시간 이후 가능
            }
        }
        return true; // 유저가 목록에 없으면 가능
    }
}
