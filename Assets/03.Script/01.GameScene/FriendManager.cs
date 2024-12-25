using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;
using System.Linq;

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

            // 중복된 닉네임 제거
            RemoveDuplicateNicknames();

            MakeFriendUiList(userList);
        }
        else
        {
            Debug.LogWarning("Save file not found, initializing with an empty list.");
            userList = new List<User>();
        }
    }

    private void RemoveDuplicateNicknames()
    {
        // 닉네임을 기준으로 중복 제거
        var uniqueUsers = userList
            .GroupBy(user => user.nickname) // 닉네임별 그룹화
            .Select(group => group.First()) // 각 그룹의 첫 번째 항목만 선택
            .ToList();

        // 중복 제거 후 userList 업데이트
        if (uniqueUsers.Count < userList.Count)
        {
            Debug.Log($"Removed {userList.Count - uniqueUsers.Count} duplicate users.");
        }

        userList = uniqueUsers;
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
