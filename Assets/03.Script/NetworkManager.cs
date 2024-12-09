using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using DG.Tweening;
using TMPro;
using Newtonsoft.Json.Linq;
using GUPS.AntiCheat;
using GUPS.AntiCheat.Protected.Prefs;
using System.Security.Cryptography;
using System.Text;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public string severSelectURL;
    public string severInsertURL;

    public string awsIP ;
    public string selectServerURL; 
    public string insertServerURL;


    public List<User> userDatas; 

    public User ownData;

    public LoadingPanel loadingPanel;

    public Transform toastRoot;
    // public CanvasGroup rankSuccessPanel;

    public bool onlineMode = false;

    public bool isSurvivalMode;

    public string key;

    public string korNotice, engNotice;

/// <summary>
/// Awake is called when the script instance is being loaded.
/// </summary>
    private void Awake()
    {
        gameObject.transform.SetParent(null);
        if(instance != null) Destroy(gameObject);
        if(instance == null){ instance = this;}
       

        DontDestroyOnLoad(this);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        selectServerURL = awsIP + severSelectURL;
        insertServerURL = awsIP + severInsertURL;
        UpdateOwnData();
        OnlineTest();
        //RecommendCheck(CanvasManager.instance.addFriendPanel.myRecommendCode);
    }


    public void UpdateOwnData()
    {
        ownData.nickname = PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.nickname);
        ownData.highscore = PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.highScore);
        ownData.profileindex = PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.profileIndex);
    }



    [ContextMenu("TestInsert")]
    public void EnrollOwnData()
    {
        NetworkManager.instance.EnrollUser(ownData);
    }

    [ContextMenu("TestSelect")]
    public void SelectData()
    {
        NetworkManager.instance.GetData();
    }



    public void RankSuccess()
    {
        // rankSuccessPanel.alpha =1;
        // rankSuccessPanel.DOFade(0,5f);
    }

    public void EnrollUser(User rankingData, Action action = null, Action failAction = null)
    {
        loadingPanel.gameObject.SetActive(true);
        //rankingData.highscore = int.Parse(Utils.EncryptScore(rankingData.highscore.ToString(), key));


        action += () => {
            loadingPanel.gameObject.SetActive(false);
            RankSuccess();
        };

        failAction += () => loadingPanel.gameObject.SetActive(false);
        //failAction += () => ToastText(LangManager.IsEng()? "Could't Upload": "랭킹을 등록할 수 없어요.");
        StartCoroutine(EnrollUserDataToServer(rankingData, action, failAction));
    }

    public void GetData(Action action = null, Action failAction = null)
    {

        
        loadingPanel.gameObject.SetActive(true);
        action += () => loadingPanel.gameObject.SetActive(false);

        failAction += () => loadingPanel.gameObject.SetActive(false);

        //string showingText = LangManager.IsEng() ? "Can not upload ranking" :"랭킹을 등록할 수 없어요.";
        //failAction += () => ToastText(showingText);
        StartCoroutine(GetDataFromServer(action,failAction));
    }

    IEnumerator GetDataFromServer(Action successAction,Action failAction)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://52.79.46.242:3000/select"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                failAction.Invoke();
            }
            else
            {
                // 서버로부터 받은 응답을 출력합니다
                string responseData = www.downloadHandler.text;
                userDatas = JsonConvert.DeserializeObject<List<User>>(responseData);
              
                successAction?.Invoke();
                
            }
        }

        
    }


    IEnumerator EnrollUserDataToServer(User data, Action successAction,Action failAction)
    {
        // RankingData 객체를 JSON 형식의 문자열로 변환합니다.
        string jsonData = JsonUtility.ToJson(data);

        // JSON 데이터를 바이트 배열로 변환합니다.
        byte[] jsonDataBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // POST 요청을 생성합니다.
        UnityWebRequest www = new UnityWebRequest("http://52.79.46.242:3001/enroll/", "POST");
        www.uploadHandler = new UploadHandlerRaw(jsonDataBytes);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("insert");

        // 요청을 보냅니다.
        yield return www.SendWebRequest();

        // 응답을 확인합니다.
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
            failAction.Invoke();
        }
        else
        {
            // 서버로부터 받은 응답을 출력합니다.
            successAction?.Invoke();
            Debug.Log("Response: " + www.downloadHandler.text);
        }
    }

    public void OnlineTest(Action action = null, Action failAction = null)
    {
        loadingPanel.gameObject.SetActive(true);
        

        action += () => loadingPanel.gameObject.SetActive(false);

        //string showingText = LangManager.IsEng() ? "Conneted to server" :"온라인 상태입니다.";
        

        //action += () => ToastText(showingText);
        action += () => 
        {
            onlineMode = true;
            //CanvasManager.instance.onlineIndicator.SetOnlineState(onlineMode);
            
        };

        failAction += () => loadingPanel.gameObject.SetActive(false);

        StartCoroutine(OnlineTestFromServer(action,failAction));
    }
    public void HeartReceiveCheck(string _code)
    {
        Debug.Log($"{_code}로 request보냄");
        StartCoroutine(HeartReceiveCheckToServer(_code));
    }

    public void RecommendAdd(string _code,Action success)
    {
        StartCoroutine(RecommendAddToServer(_code, success));
    }

    [ContextMenu("RecommendTest")]
    public void RecommendAdd()
    {
        // StartCoroutine(RecommendAddToServer(RecommendTest));
    }

    


    [ContextMenu("NoticeUpload")]
    public void NoticeUpload()
    {
        StartCoroutine(UploadNotice());
    }

    IEnumerator OnlineTestFromServer(Action successAction,Action failAction)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://52.79.46.242:3000/notice"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                failAction.Invoke();
            }
            else
            {

                string jsonText = www.downloadHandler.text;
                Debug.Log(jsonText);

                // jsonText에서 불필요한 부분을 제거하고, 실제 문자열을 추출합니다.
                string jsonBody = jsonText.Trim(new char[] { '{', '}', '"' });

                // jsonBody는 "테스트입니다.^It's a test" 형태일 것입니다.
                string notice = jsonBody.Split(':')[0].Trim(); // 키를 추출

                // notice는 "테스트입니다.^It's a test" 형태일 것입니다.
                string[] splitText = notice.Split('^');

                if (splitText.Length == 2)
                {
                    string kor = splitText[0];
                    string eng = splitText[1];

                    // 이제 각각의 문자열을 UI에 표시합니다.
                    //CanvasManager.instance.ChangeNotice(eng, kor);
                    
                    korNotice = kor;
                    engNotice = eng;

                    successAction?.Invoke();
                }
            }
        }
    }
    IEnumerator RecommendAddToServer(string _code, Action success)
    {
        string jsonBody = JsonUtility.ToJson("////////////////////umjunsick");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest("http://52.79.46.242:3001/recommendAdd", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 요청 보내기
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                //CanvasManager.instance.rewardPanel.ShowPanel(2000);
                success.Invoke();
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
    }

    //IEnumerator HeartReceiveCheckToServer(string _code)
    //{
    //    var request = new UnityWebRequest("http://52.79.46.242:3001/checkMyHeartList", "POST");
    //    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(_code);
    //    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //    request.downloadHandler = new DownloadHandlerBuffer();
    //    request.SetRequestHeader("Content-Type", "text/plain");
    //    loadingPanel.gameObject.SetActive(true);

    //    // 요청 보내기
    //    yield return request.SendWebRequest();

    //    if (request.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError("Error: " + request.error);
    //    }
    //    else
    //    {
    //        loadingPanel.gameObject.SetActive(false);

    //        string responseData = www.downloadHandler.text;
    //        userDatas = JsonConvert.DeserializeObject<List<HeartData>>(responseData);

    //        //int recommendAmount = int.Parse(request.downloadHandler.text);
    //        //if(recommendAmount > 0) 
    //        //{
    //        //    //CanvasManager.instance.rewardPanel.ShowPanel(int.Parse(request.downloadHandler.text)*2000);
    //        //    GameManager.instance.ToastText(recommendAmount.ToString() + "명의 친구에게 추천을 받았어요!");
    //        //}
    //        //Debug.Log(request.downloadHandler.text);
    //    }
    //}
    IEnumerator UploadNotice()
    {

        // POST 요청을 생성합니다.
        UnityWebRequest www = UnityWebRequest.PostWwwForm("http://52.79.46.242:3000/noticeupload",noticeText);

        Debug.Log("Sending insert request");

        // 요청을 보냅니다.
        yield return www.SendWebRequest();

        // 응답을 확인합니다.
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Response: " + www.downloadHandler.text);
        }
    }

    

    public void GoldChange(int value)
    {
        var gold = ProtectedPlayerPrefs.GetInt("gold");


        gold  += value;

        if(gold < 0) gold = 0;
        ProtectedPlayerPrefs.SetInt("gold",gold);
        ProtectedPlayerPrefs.SetInt("goldDoubleCheck",ProtectedPlayerPrefs.GetInt("gold"));
        // PlayerPrefs.SetInt
    }


    public int GetCurGold()
    {
        
        return ProtectedPlayerPrefs.GetInt("gold");
    }
}