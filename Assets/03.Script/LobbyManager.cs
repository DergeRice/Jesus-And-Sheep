using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Networking;
using System;
using GUPS.AntiCheat.Protected.Prefs;

public class LobbyManager : MonoBehaviour
{
    public GameObject allRoot, tutorial, nicknamePanel;

    public TMP_Text myNameText,myHighScoreText, goldText;
    
    public void Start()
    {
        GameManager.instance.lobbyManager =this;

        NetworkManager.instance.Init();
        Time.timeScale = 1.0f;

        if(PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.isFirstTime)== string.Empty)
        {
            nicknamePanel.SetActive(true);
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.gold,1000);
        }

        goldText.text = PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.gold);
        SoundManager.instance.ChangeBgm(0);
        SoundManager.instance.bgmAudioSource.Play();
        AdsInitializer.instance.interstitialAd.LoadAd();
        AdsInitializer.instance.bannerAd.HideBannerAd();

        AdsInitializer.instance.rewardedAd.LoadAd();
        LoadMySavedData();

        //CanvasManager.instance.ChangeNotice(NetworkManager.instance.engNotice,NetworkManager.instance.korNotice);
    }

    public void LoadMySavedData()
    {
        myNameText.text = NetworkManager.instance.ownData.nickname;
        myHighScoreText.text = NetworkManager.instance.ownData.highscore.ToString();
        goldText.text = NetworkManager.instance.GetGold().ToString();
        // GameManager.instance.friendManager.LoadData();
    }
    public void StartGameScene()
    {
        StartCoroutine(LoadGameSceneAsync());
    }

    private IEnumerator LoadGameSceneAsync()
    {
        // 비동기 씬 로딩 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("02.GameScene");
        
        // 로딩 진행 상태 확인
        while (!asyncLoad.isDone)
        {
            // 진행 상태를 보여주는 기능 추가 가능
            yield return null;
        }

        // 씬 로딩 완료 후 로딩 화면 숨기기
        // loadingScreen.SetActive(false);
    }

    public void ShowPolicy()
    {
        Application.OpenURL("https://www.notion.so/14a66596c89d80cfb071c2279ae1fa07?pvs=4");
    }
    public void ShowHomePage()
    {
        Application.OpenURL("https://www.baragames.co.kr");
    }

    public void OpenEmail()
    {
        string mailto = "leesangjin2372@gmail.com"; 
        string subject = EscapeURL("[예수님과 양떼들]게임 문의"); 
        string body = EscapeURL("내용을 입력해주세요."); 
    
        Application.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body); 
        
        //string engEmailText = LangManager.instance.isEng ?  "Send Email to leesangjin2372@gmail.com." : "leesangjin2372@gmail.com으로 메일을 보냅니다.";
        //NetworkManager.instance.ToastText(engEmailText);
    } 
    string EscapeURL(string url) 
    { 
        return UnityWebRequest.EscapeURL(url).Replace("+", "%20"); 
    }

    public void AndroidQuit()
    {
        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call<bool>("moveTaskToBack", true);
        Application.Quit();
        Debug.Log("Quit App");
    }

    
}

public class RandomHashCodeGenerator 
{
    private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    
    private static System.Random random = new System.Random();

    // 6자리의 랜덤 해시코드를 생성하는 메서드
    public static string GenerateRandomHashCode(int length = 6)
    {
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }
    
}
