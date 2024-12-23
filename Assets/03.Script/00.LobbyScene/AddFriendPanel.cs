using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AddFriendPanel : PanelBase
{
    public Button checkButton, backToEnroll;
    public TMP_InputField nicknameField;

    public GameObject cantEnroll;

    public void Start()
    {

        checkButton.onClick.AddListener(CheckNickNameAvailable);
        backToEnroll.onClick.AddListener(()=>
        {
            cantEnroll.SetActive(false);
        });
    }

    public async void CheckNickNameAvailable()
    {
        if(GameManager.instance.badWordManager.IsPossbieNickName(nicknameField.text) == false)
        {
            GameManager.instance.ToastText("비속어와 특수문자는 불가합니다.");
            return;
        }

        if(NetworkManager.instance.ownData.nickname == nicknameField.text)
        {
            GameManager.instance.ToastText("자신은 친구가 될 수 없어요.");
            return;
        }
        
        // bool isEmptyNickName ; //success 면 empty라는 소리;
        // 그래서 친구추가 불가
        // if call back is success : no nickname user found so, I cant add friend;

        Action successAction =()=>
        {
            cantEnroll.SetActive(true);
        };

        Action failAction = () =>
        {
            root.SetActive(false);
            GameManager.instance.friendManager.AddFriend(nicknameField.text);
        };
        NetworkManager.instance.CheckNicknameAsync(nicknameField.text,successAction,failAction);

        
    }
}
