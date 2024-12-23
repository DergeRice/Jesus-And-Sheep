using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartPanel : PanelBase
{
    public Button shopButton, friendButton, adButton, goldButton;

    public TMP_Text remainText;

    public int adMax = 5; // 하루 최대 광고 시청 가능 횟수
    public int currentRemainAd;

    TabManager tabManager;

    private void Start()
    {
        UpdateAdCount();

        tabManager = FindFirstObjectByType<TabManager>();
        
        shopButton.onClick.AddListener(()=> 
        {
            tabManager.EnableTab(0);
            root.SetActive(false);
        });

        goldButton.onClick.AddListener(()=> 
        {
            tabManager.EnableTab(0);
            root.SetActive(false);
        });

        friendButton.onClick.AddListener(()=> 
        {
            tabManager.EnableTab(3);
            root.SetActive(false);
        });


        adButton.onClick.AddListener(()=>
        {
            AdsInitializer.instance.rewardedAd.adSuccessAction = UseAdWatch;
            AdsInitializer.instance.rewardedAd.ShowAd();
        });
    }

    private void UpdateAdCount()
    {
        string lastAdWatchedDate = PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.lastAdWatchedDate);
        string todayDate = System.DateTime.Now.ToString("yyyy-MM-dd"); // 날짜 포맷 (년-월-일)

        if (todayDate != lastAdWatchedDate)
        {
            // 날짜가 바뀌었으면 광고 시청 가능 횟수를 초기화
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.remainAdWatchCount, adMax);
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.lastAdWatchedDate, todayDate);
            currentRemainAd = adMax;
        }
        else
        {
            // 오늘 날짜라면 저장된 남은 횟수 가져오기
            currentRemainAd = PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.remainAdWatchCount);
        }

        // UI 업데이트
        UpdateUI();
    }

    public void UseAdWatch()
    {
        if (currentRemainAd > 0)
        {
            currentRemainAd--;
            GameManager.instance.heartManager.GainHeart();
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.remainAdWatchCount, currentRemainAd);
            UpdateUI();
        }
        else
        {
            GameManager.instance.ToastText("남은 광고 시청 횟수가 없습니다.");
            Debug.Log("남은 광고 시청 횟수가 없습니다!");
        }
    }

    private void UpdateUI()
    {
        remainText.text = $"남은 광고 횟수 :{currentRemainAd}/{adMax}";
        adButton.interactable = currentRemainAd > 0; // 광고 버튼 활성화/비활성화
    }
}
