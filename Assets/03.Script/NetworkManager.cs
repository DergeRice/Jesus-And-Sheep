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


    public List<RankingData> classicRankingDatas ;
    public List<RankingData> survivalRankingDatas ;

    public RankingData ownData;

    public LoadingPanel loadingPanel;

    public Transform toastRoot;
    // public CanvasGroup rankSuccessPanel;

    public bool onlineMode = false;

    private TMP_Text toastText;

    public GameObject toastUIPrefeb;

    public string noticeText;

    public string RecommendTest;

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

    public void SetSurvivalMode(bool isSurvival)
    {
        isSurvivalMode = isSurvival;
    }

    public void UpdateOwnData()
    {
        ownData.name = PlayerPrefs.GetString("nickName");
        ownData.churchName = PlayerPrefs.GetString("churchName");
        ownData.profileIndex = PlayerPrefs.GetInt("profileIndex");
        ownData.code = PlayerPrefs.GetString("MyRecommend");
    }



    [ContextMenu("TestInsert")]
    public void EnrollOwnData()
    {
        NetworkManager.instance.InsertData(ownData);
    }

    [ContextMenu("TestSelect")]
    public void SelectData()
    {
        NetworkManager.instance.GetData();
    }

    public void ToastText(string text)
    {
        var toastPanel = Instantiate(toastUIPrefeb,toastRoot).GetComponent<CanvasGroup>();
        
        toastText = toastPanel.transform.GetChild(0).GetComponent<TMP_Text>();

        toastText.text = text;
        Destroy(toastPanel.gameObject,2f);
    }


    public void RankSuccess()
    {
        // rankSuccessPanel.alpha =1;
        // rankSuccessPanel.DOFade(0,5f);
    }

    public void InsertData(RankingData rankingData,Action action = null,Action failAction = null)
    {
        loadingPanel.gameObject.SetActive(true);
        rankingData.encryptedScore =  EncryptScore(rankingData.score.ToString(), key);
        

        if(rankingData.name == "") rankingData.name = "Name";
        if(rankingData.gameMode == "") rankingData.gameMode = "classic";
        if(rankingData.churchName == "") rankingData.churchName = "ChurchName";

        rankingData.encryptedTime = EncryptScore(DateTime.Now.Minute.ToString(),key);
        rankingData.encryptedGameMode = EncryptScore(rankingData.gameMode,key);

        action += () => {
            loadingPanel.gameObject.SetActive(false);
            RankSuccess();
            };

        failAction += () => loadingPanel.gameObject.SetActive(false);
        //failAction += () => ToastText(LangManager.IsEng()? "Could't Upload": "랭킹을 등록할 수 없어요.");
        StartCoroutine(InsertDataToServer(rankingData,action,failAction));
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
                classicRankingDatas = JsonConvert.DeserializeObject<List<RankingData>>(responseData);
              
                successAction?.Invoke();
                
            }
        }

        
    }


    IEnumerator InsertDataToServer(RankingData data, Action successAction,Action failAction)
    {
        // RankingData 객체를 JSON 형식의 문자열로 변환합니다.
        string jsonData = JsonUtility.ToJson(data);

        // JSON 데이터를 바이트 배열로 변환합니다.
        byte[] jsonDataBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // POST 요청을 생성합니다.
        UnityWebRequest www = new UnityWebRequest("http://52.79.46.242:3000/insert/", "POST");
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

        //string showingFailText = LangManager.IsEng() ? "Can Not Conneted to server" :"오프라인 상태입니다.";
        //failAction += () => ToastText(showingFailText);
        StartCoroutine(OnlineTestFromServer(action,failAction));
    }
    public void RecommendCheck(string _code)
    {
        Debug.Log($"{_code}로 request보냄");
        StartCoroutine(RecommendCheckFromServer(_code));
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
        string jsonBody = JsonUtility.ToJson(new CodeRequest { code = _code });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest("http://52.79.46.242:3000/recommendAdd", "POST"))
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

    IEnumerator RecommendCheckFromServer(string _code)
    {
        var request = new UnityWebRequest("http://52.79.46.242:3000/Recommed", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(_code);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "text/plain");
        loadingPanel.gameObject.SetActive(true);

        // 요청 보내기
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            loadingPanel.gameObject.SetActive(false);
            
            int recommendAmount = int.Parse(request.downloadHandler.text);
            if(recommendAmount > 0) 
            {
                //CanvasManager.instance.rewardPanel.ShowPanel(int.Parse(request.downloadHandler.text)*2000);
                ToastText(recommendAmount.ToString() + "명의 친구에게 추천을 받았어요!");
            }
            Debug.Log(request.downloadHandler.text);
        }
    }
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

    private string EncryptScore(string plainText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.GenerateIV();
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var ms = new System.IO.MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // //코드에서 냄새가 나기 시작한다... 스멀스멀..
    public string Decrypt(string encryptedText, string key)
    {
        try
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedText);
            byte[] iv = new byte[16];
            byte[] cipherText = new byte[fullCipher.Length - 16];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var ms = new System.IO.MemoryStream(cipherText))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new System.IO.StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Decryption failed: " + ex.Message);
            return null;
        }
    }

    // [ContextMenu("EncryptAndDEcrypted")]

    // // 테스트를 위한 예제 함수
    // public void TestDecryption()
    // {
    //     ownData.encryptedScore = EncryptScore(ownData.score.ToString(),key);

    //     // string encryptedText = ownData.encryptedScore; // 암호화된 텍스트
    //     // string key = "7Jsd9sZ#tGJlf48QbA1pL6k2MjVx8NzO";  // 32바이트 키

    //     string decryptedText = Decrypt(ownData.encryptedScore, key);
    //     Debug.Log("Decrypted text: " + decryptedText);
    // }


    public class CodeRequest
    {
        public string code;
    }

    public class Notice
    {
        public string kor, eng;
    }
}