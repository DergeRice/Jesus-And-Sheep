using UnityEngine;
using System;
using TMPro;
using DG.Tweening;

using Newtonsoft.Json.Linq;
using GUPS.AntiCheat;
using GUPS.AntiCheat.Protected.Prefs;
using System.Security.Cryptography;
using GUPS.AntiCheat.Protected;

public class HeartManager : MonoBehaviour
{
    private const string HeartCountKey = "HeartCount"; // 하트 수 저장
    private const string LastHeartChargeTimeKey = "LastHeartChargeTime"; // 마지막 하트 충전 시간 저장

    [SerializeField] private string heartCount;// 현재 하트 수
    private DateTime lastHeartChargeTime; // 마지막 하트 충전 시간
    private const int maxHeartCount = 5; // 최대 하트 수
    private const int chargeIntervalMinutes = 30; // 하트 충전 간격 (분 단위)

    public TMP_Text heartFillText, heartFillCoolText;


    private void Awake()
    {
        GameManager.instance.heartManager = this;
        LoadHeartData(); // 하트 데이터 로드
        UpdateHeartCharge(); // 하트 충전 업데이트
        // heartCount = ProtectedPlayerPrefs.GetInt();
    }

    private void Update()
    {
        // 하트가 최대값보다 적을 때만 쿨타임 적용
        if (int.Parse(heartCount) < maxHeartCount)
        {
            UpdateHeartCharge(); // 충전 상태 업데이트
            UpdateCoolTimeText(); // 쿨타임 텍스트 업데이트
        }
        else
        {
            heartFillCoolText.gameObject.SetActive(false); // 최대 하트 수에 도달하면 쿨타임 텍스트 숨기기
        }

        heartFillText.text = $"{heartCount} / {maxHeartCount}";
    }
    private void LoadHeartData()
    {
        // 하트 수 로드 (기본값은 최대 하트 수)
        heartCount = ProtectedPlayerPrefs.GetInt(HeartCountKey, maxHeartCount).ToString();

        // 마지막 충전 시간을 string으로 저장했으므로, DateTime으로 변환
        string lastChargeTimeString = ProtectedPlayerPrefs.GetString(LastHeartChargeTimeKey, DateTime.UtcNow.ToString("o"));
        lastHeartChargeTime = DateTime.Parse(lastChargeTimeString);

        // 게임 종료 후 누락된 하트 계산
        UpdateHeartCharge();
    }

    private void UpdateCoolTimeText()
    {
        if (int.Parse(heartCount) >= maxHeartCount)
        {
            heartFillCoolText.gameObject.SetActive(false); // 최대치면 쿨타임 숨김
            return;
        }

        DateTime currentTime = DateTime.Now;
        DateTime nextChargeTime = lastHeartChargeTime.AddMinutes(chargeIntervalMinutes);
        TimeSpan timeUntilNextCharge = nextChargeTime - currentTime;

        // 남은 시간 표시
        if (timeUntilNextCharge.TotalSeconds > 0)
        {
            heartFillCoolText.gameObject.SetActive(true);
            heartFillCoolText.text = $"{timeUntilNextCharge.Minutes:D2}:{timeUntilNextCharge.Seconds:D2}";
        }
        else
        {
            heartFillCoolText.gameObject.SetActive(false);
        }
    }

    private void UpdateHeartCharge()
    {
        DateTime currentTime = DateTime.Now;
        TimeSpan timeSinceLastCharge = currentTime - lastHeartChargeTime;

        // Debug용 변수 업데이트
        debugLastHeartChargeTime = lastHeartChargeTime.ToString("o");

        if (int.Parse(heartCount) < maxHeartCount)
        {
            int newHearts = (int)(timeSinceLastCharge.TotalMinutes / chargeIntervalMinutes);
            if (newHearts > 0)
            {
                heartCount = Mathf.Min(int.Parse(heartCount) + newHearts, maxHeartCount).ToString();
                lastHeartChargeTime = lastHeartChargeTime.AddMinutes(newHearts * chargeIntervalMinutes);
                SaveHeartData();
            }
        }
    }
    private void SaveHeartData()
    {
        ProtectedPlayerPrefs.SetInt(HeartCountKey, int.Parse(heartCount)); // 하트 수 저장

        // 마지막 충전 시간을 string으로 저장
        ProtectedPlayerPrefs.SetString(LastHeartChargeTimeKey, lastHeartChargeTime.ToString("o")); // "o" 포맷은 ISO 8601 표준
        ProtectedPlayerPrefs.Save();
    }

    [ContextMenu("UseHeart")]
    public void UseHeart()
    {
        if(int.Parse(heartCount) == 5)
        {
            lastHeartChargeTime = DateTime.Now;
        }

        if (int.Parse(heartCount) > 0)
        {
            heartCount = (int.Parse(heartCount)-1).ToString();
            
            SaveHeartData(); // 하트 사용 후 저장
        }
        else
        {
            Debug.Log("하트가 부족합니다!");
        }
    }

    [ContextMenu("GainHeart")]
    public void GainHeart()
    {
        heartCount = (int.Parse(heartCount)+1).ToString();
        SaveHeartData(); // 하트 사용 후 저장
    }
    public void GainHeart(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            heartCount = (int.Parse(heartCount)+1).ToString();
            SaveHeartData(); // 하트 사용 후 저장
        }
    }

    public int GetHeartCount()
    {
        return int.Parse(heartCount);
    }

    public void ResetHeartCount()
    {
        heartCount = "0";
        lastHeartChargeTime = DateTime.UtcNow;
        SaveHeartData(); // 초기화 후 저장
    }

    [SerializeField] private string debugLastHeartChargeTime;



}
