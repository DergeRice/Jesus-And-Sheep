using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HeartReceiveUI : MonoBehaviour
{
    public TMP_Text nickname;
    public string hashValue;
    public Button receiveButton;

    public void Init(HeartData heartData)
    {
        nickname.text =  $"{heartData.senderNickName}님이 사랑을 전해줬어요!";
        hashValue = heartData.heartHash;
        Action success = ()=>
        {
            GameManager.instance.heartManager.GainHeart();
            Destroy(gameObject);
        };
        
        receiveButton.onClick.AddListener(()=>
        {
            NetworkManager.instance.HeartReceive(hashValue,success);
        });
    }
}
