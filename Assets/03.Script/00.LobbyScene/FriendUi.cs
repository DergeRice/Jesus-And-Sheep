using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class FriendUi : MonoBehaviour
{
    public string friendName;
    public TMP_Text nameText, timeText;
    public Image heart;
    public Button sendButton;
    public DateTime lastSendTime;

    public int sendCoolTimeMinute; // 쿨타임 (분 단위)

    private void Start()
    {
        sendCoolTimeMinute = 360;
        timeText.gameObject.SetActive(false);
        CheckThisFriendSendAvailable();
    }

    private void Update()
    {
        // 주기적으로 쿨타임 체크
        if (timeText.gameObject.activeSelf)
        {
            UpdateCooldownText();
        }
    }

    private void CheckThisFriendSendAvailable()
    {
        DateTime currentTime = DateTime.Now;
        DateTime nextSendAvailableTime = lastSendTime.AddMinutes(sendCoolTimeMinute);

        if (currentTime >= nextSendAvailableTime)
        {
            // 하트를 보낼 수 있는 상태
            SetSendAvailableState();
        }
        else
        {
            // 아직 쿨타임 상태
            SetCooldownState(nextSendAvailableTime);
        }
    }

    public void Init(User user)
    {
        friendName = user.nickname;
        nameText.text = user.nickname;

        // lastSendTime과 sendCoolTimeMinute을 User에서 받아옴
        lastSendTime = user.lastSendTime;

        // 하트를 보낼 수 있는지 확인
        Action success = SuccessSend;
        sendButton.onClick.AddListener(() =>
        {
            NetworkManager.instance.SendHeart(friendName, success);
        });

        // 친구가 하트를 보낼 수 있는지 체크
        CheckThisFriendSendAvailable();
    }
    public void SuccessSend()
    {
        lastSendTime = DateTime.Now;
        GameManager.instance.friendManager.SaveSendState(friendName,lastSendTime);
        SetAlreadySendState();
    }

    private void SetSendAvailableState()
    {
        sendButton.interactable = true;
        heart.color = Color.white;
        timeText.gameObject.SetActive(false);
    }

    private void SetCooldownState(DateTime nextSendAvailableTime)
    {
        sendButton.interactable = false;
        heart.color = Color.gray;
        timeText.gameObject.SetActive(true);

        UpdateCooldownText(nextSendAvailableTime);
    }

    private void UpdateCooldownText()
    {
        DateTime currentTime = DateTime.Now;
        DateTime nextSendAvailableTime = lastSendTime.AddMinutes(sendCoolTimeMinute);
        TimeSpan remainingTime = nextSendAvailableTime - currentTime;

        if (remainingTime.TotalSeconds <= 0)
        {
            CheckThisFriendSendAvailable();
        }
        else
        {
            timeText.text = string.Format("{0:D1}:{1:D2}:{2:D2}", remainingTime.Hours ,remainingTime.Minutes, remainingTime.Seconds);
        }
    }

    private void UpdateCooldownText(DateTime nextSendAvailableTime)
    {
        TimeSpan remainingTime = nextSendAvailableTime - DateTime.Now;

        if (remainingTime.TotalSeconds <= 0)
        {
            CheckThisFriendSendAvailable();
        }
        else
        {
            timeText.text = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);
        }
    }

    public void SetAlreadySendState()
    {
        sendButton.interactable = false;
        heart.color = Color.gray;
        timeText.gameObject.SetActive(true);
        UpdateCooldownText();
    }
}