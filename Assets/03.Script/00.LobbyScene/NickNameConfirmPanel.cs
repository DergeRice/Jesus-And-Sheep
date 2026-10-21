using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class NickNameConfirmPanel : PanelBase
{
    public Button checkButton, backToEnroll;
    public TMP_InputField nicknameField;

    public GameObject cantEnroll, tutorialPanel;

    public void Start()
    {

        checkButton.onClick.AddListener(()=>CheckNickNameAvailable(nicknameField));
        backToEnroll.onClick.AddListener(()=>
        {
            cantEnroll.SetActive(false);
        });
    }

    public void CheckNickNameAvailable(TMP_InputField input)
    {
        Debug.Log("reqqq");
        if(string.IsNullOrEmpty(input.text))
        {
            GameManager.instance.ToastText("입력해주세요");
            return;
        }
        if(GameManager.instance.badWordManager.IsPossbieNickName(nicknameField.text) == false)
        {
            GameManager.instance.ToastText("비속어와 특수문자는 불가합니다.");
            return;
        }


        Action successAction = () =>
        {
            checkButton.interactable = false;
            User temp = new User { nickname = nicknameField.text };
            NetworkManager.instance.EnrollUser(temp, () =>
            {
                checkButton.interactable = true;
                root.SetActive(false);
                PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.isFirstTime, true);
                NetworkManager.instance.UpdateOwnData();
                GameManager.instance.lobbyManager.LoadMySavedData();
                tutorialPanel.SetActive(true);
            }, () =>
            {
                checkButton.interactable = true;
                cantEnroll.SetActive(true);
            });
        };

        Action failAction = () =>
        {
            cantEnroll.SetActive(true);
        };

        NetworkManager.instance.CheckNicknameAsync(nicknameField.text,successAction,failAction);
    }
}
