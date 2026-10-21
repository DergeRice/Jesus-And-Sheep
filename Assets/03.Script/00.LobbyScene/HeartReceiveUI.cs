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
        LocalizedText.Attach(nickname).SetFormat("{0}님이 사랑을 전해줬어요!", heartData.senderNickName);

        hashValue = heartData.heartHash;
        receiveButton.interactable = true;
        receiveButton.onClick.RemoveAllListeners();
        Action success = ()=>
        {
            if (!FirebaseSession.Enabled) GameManager.instance.heartManager.GainHeart();
            Destroy(gameObject);
        };
        
        receiveButton.onClick.AddListener(()=>
        {
            receiveButton.interactable = false;
            NetworkManager.instance.HeartReceive(hashValue, success, () => receiveButton.interactable = true);
        });
    }
}
