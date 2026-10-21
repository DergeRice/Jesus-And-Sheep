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
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
    
    public static NetworkManager instance;

    [Tooltip("Fallback API URL. Resources/Network/server.json can override this for each deployment.")]
    public string baseURL;
    private int requestTimeoutSeconds = 15;
    private string firebaseIdToken;
    private float tokenObtainedAt;
    private string tokenUserId;
    private bool cloudDirty;
    private float nextCloudSaveAt;
    private bool cloudSaveInFlight;
    public bool CloudSaveInFlight => cloudSaveInFlight;
    public bool CloudSavePending => cloudDirty;
    public IEnumerator FlushCloudSave()
    {
        yield return new WaitUntil(() => !cloudSaveInFlight);
        if (cloudDirty && onlineMode && !string.IsNullOrEmpty(ownData.nickname)) yield return SaveCloudSnapshot();
    }

    private async Task ConfigureFirebaseRequestAsync(UnityWebRequest request)
    {
        if (!FirebaseSession.Enabled) return;
        // Tokens remain memory-only; refresh when expired or when switching accounts.
        if (string.IsNullOrEmpty(firebaseIdToken) || tokenUserId != FirebaseSession.UserId || Time.realtimeSinceStartup - tokenObtainedAt > 2700)
        {
            firebaseIdToken = await FirebaseSession.GetIdTokenAsync();
            tokenObtainedAt = Time.realtimeSinceStartup;
            tokenUserId = FirebaseSession.UserId;
        }
        request.SetRequestHeader("Authorization", "Bearer " + firebaseIdToken);
    }

    private IEnumerator PrepareRequest(UnityWebRequest request)
    {
        Task authorization = ConfigureFirebaseRequestAsync(request);
        yield return new WaitUntil(() => authorization.IsCompleted);
        if (authorization.IsFaulted || authorization.IsCanceled)
            Debug.LogWarning("Firebase authentication unavailable. Request will fail without credentials.");
    }

    private void ApplyCloudGiftCount(int serverCount)
    {
        int applied = ProtectedPlayerPrefs.GetInt("FirebaseAppliedGiftCount", 0);
        if (serverCount > applied)
        {
            GameManager.instance.heartManager.GainHeart(serverCount - applied);
            ProtectedPlayerPrefs.SetInt("FirebaseAppliedGiftCount", serverCount);
            ProtectedPlayerPrefs.Save();
            MarkCloudDirty();
        }
    }

    public void ReloadFirebaseAccount()
    {
        onlineMode = false;
        firebaseIdToken = null;
        tokenUserId = null;
        cloudDirty = false;
        OnlineTest();
    }

    public void MarkCloudDirty()
    {
        if (!FirebaseSession.Enabled || FirebaseCloudSave.Restoring) return;
        cloudDirty = true;
        nextCloudSaveAt = Time.realtimeSinceStartup + 3f;
    }

    private void Update()
    {
        if (FirebaseSession.Enabled && onlineMode && cloudDirty && !cloudSaveInFlight &&
            Time.realtimeSinceStartup >= nextCloudSaveAt && !string.IsNullOrEmpty(ownData.nickname))
            StartCoroutine(SaveCloudSnapshot());
    }

    private IEnumerator SaveCloudSnapshot()
    {
        cloudSaveInFlight = true;
        cloudDirty = false;
        using (var request = new UnityWebRequest(baseURL + "/save", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(new { snapshot = FirebaseCloudSave.Capture() })));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = requestTimeoutSeconds;
            yield return PrepareRequest(request);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                cloudDirty = true;
                nextCloudSaveAt = Time.realtimeSinceStartup + 30f;
            }
        }
        cloudSaveInFlight = false;
    }

    private IEnumerator FirebaseLogin(Action success, Action failure)
    {
        Task login = FirebaseSession.InitializeAsync();
        yield return new WaitUntil(() => login.IsCompleted);
        if (login.IsFaulted || login.IsCanceled) { failure?.Invoke(); yield break; }
        string previousOwner = ProtectedPlayerPrefs.GetString("FirebaseLocalDataOwner", "");
        if (!string.IsNullOrEmpty(previousOwner) && previousOwner != FirebaseSession.UserId)
        {
            FirebaseCloudSave.BackupGuest(previousOwner);
            FirebaseCloudSave.Restore(FirebaseCloudSave.NewAccount());
        }
        using (var request = UnityWebRequest.Get(baseURL + "/me"))
        {
            request.timeout = requestTimeoutSeconds;
            yield return PrepareRequest(request);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) { failure?.Invoke(); yield break; }
            JObject body;
            try { body = JObject.Parse(request.downloadHandler.text); }
            catch (JsonException) { failure?.Invoke(); yield break; }
            bool registered = body.Value<bool>("registered");
            if (registered)
            {
                ownData = body["user"].ToObject<User>();
                using (var save = UnityWebRequest.Get(baseURL + "/save"))
                {
                    save.timeout = requestTimeoutSeconds;
                    yield return PrepareRequest(save);
                    yield return save.SendWebRequest();
                    // Do not upload a stale local save if restore fails.
                    if (save.result != UnityWebRequest.Result.Success) { failure?.Invoke(); yield break; }
                    try
                    {
                        var result = JObject.Parse(save.downloadHandler.text);
                        if (result.Value<bool>("exists"))
                            FirebaseCloudSave.Restore(result["snapshot"].ToObject<FirebaseCloudSnapshot>());
                    }
                    catch (Exception) { failure?.Invoke(); yield break; }
                }
                PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.nickname, ownData.nickname);
                PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.profileIndex, ownData.profileindex);
                PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.highScore,
                    Math.Max(ownData.highscore, PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.highScore)));
                PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.isFirstTime, true);
                GameManager.instance.heartManager.ReloadCloudData();
                ApplyCloudGiftCount(body.Value<int>("claimedHeartCount"));
                GameManager.instance.friendManager.LoadData();
                GameManager.instance.lobbyManager.nicknamePanel.SetActive(false);
                var ui = FindFirstObjectByType<LobbySceneUIManager>();
                if (ui != null) ui.countinueButton.interactable = PlayerPrefsManager.Instance.GetBoolSetting(PlayerPrefsData.hasLastGame);
                LanguageManager.Instance.SetLanguage(PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.languageCode));
            }
            else
            {
                // A local nickname is not proof of ownership of an old server account.
                ownData = new User();
                GameManager.instance.lobbyManager.nicknamePanel.SetActive(true);
            }
            ProtectedPlayerPrefs.SetString("FirebaseLocalDataOwner", FirebaseSession.UserId);
            ProtectedPlayerPrefs.Save();
            GameManager.instance.lobbyManager.LoadMySavedData();
        }
        yield return VersionCheck(success, () =>
        {
            if (versionDidntMatchPanel != null) versionDidntMatchPanel.SetActive(true);
        }, failure);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && onlineMode && FirebaseSession.Enabled && cloudDirty && !cloudSaveInFlight)
            StartCoroutine(SaveCloudSnapshot());
    }


    [Serializable]
    private class ServerConfiguration
    {
        public string baseUrl;
        public int timeoutSeconds = 15;
    }

    private void LoadServerConfiguration()
    {
        TextAsset file = Resources.Load<TextAsset>("Network/server");
        if (file != null)
        {
            var config = JsonUtility.FromJson<ServerConfiguration>(file.text);
            if (config != null)
            {
                if (!string.IsNullOrWhiteSpace(config.baseUrl)) baseURL = config.baseUrl;
                requestTimeoutSeconds = Mathf.Clamp(config.timeoutSeconds, 5, 60);
            }
        }
        baseURL = (baseURL ?? string.Empty).Trim().TrimEnd('/');
        if (!Uri.TryCreate(baseURL, UriKind.Absolute, out Uri uri) || uri.Scheme != "https")
        {
            Debug.LogError("Configure an HTTPS API URL in Resources/Network/server.json.");
            baseURL = string.Empty;
        }
    }

    public static bool IsNicknameAvailable(string response)
    {
        string status = response?.Trim();
        if (string.Equals(status, "available", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Nickname is available.", StringComparison.OrdinalIgnoreCase)) return true;
        try
        {
            JObject body = JObject.Parse(response);
            return body["available"]?.Type == JTokenType.Boolean
                ? body["available"].Value<bool>()
                : string.Equals((string)(body["status"] ?? body["message"]), "available", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException) { return false; }
        catch (ArgumentException) { return false; }
    }

    public static bool TryReadCompatibility(string response, out bool compatible)
    {
        compatible = false;
        try
        {
            JToken token = JObject.Parse(response)["isCompatible"];
            if (token == null || token.Type != JTokenType.Boolean) return false;
            compatible = token.Value<bool>();
            return true;
        }
        catch (JsonException) { return false; }
        catch (ArgumentException) { return false; }
    }

    public User ownData;

    public LoadingPanel loadingPanel;

    public Transform toastRoot;
    // public CanvasGroup rankSuccessPanel;

    public bool onlineMode = false;

    public string currentVersion = "1.0.0";

    public GameObject versionDidntMatchPanel;

    private void Awake()
    {
        gameObject.transform.SetParent(null);
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        LoadServerConfiguration();
        if (FirebaseSession.Enabled)
            baseURL = (FirebaseSession.Config.apiBaseUrl ?? "").Trim().TrimEnd('/');
       

        DontDestroyOnLoad(this);
    }
    
    // Start is called before the first frame update
    public void Init()
    {
        UpdateOwnData();
        OnlineTest();
        Utils.DelayCall(()=>{ GameManager.instance.lobbyManager.LoadMySavedData();},0.05f);
    }
    public void NetworkSuccess()
    {
        GetRank();
        FetchReceivedHeartList();
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
        EnrollUser(ownData);
    }
    public void CheckNicknameAsync(string nickname, Action success, Action fail)
    {
        StartCoroutine(CheckNicknameToServer(nickname,success,fail));
    }

    public IEnumerator CheckNicknameToServer(string nickname, Action success, Action fail)
    {
        string url = $"{baseURL}/check?nickname={UnityWebRequest.EscapeURL(nickname)}";
        using UnityWebRequest request = UnityWebRequest.Get(url);

        Debug.Log("request");
        request.timeout = requestTimeoutSeconds;
        yield return PrepareRequest(request);
        yield return request.SendWebRequest(); // 요청 완료될 때까지 대기

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (IsNicknameAvailable(request.downloadHandler.text))
            {
                success?.Invoke(); // 성공 콜백 호출
            }
            else
            {
                fail?.Invoke(); // 실패 콜백 호출
            }
        }
        else
        {
            Debug.LogError($"Error checking nickname: {request.error}");
            fail?.Invoke(); // 실패 콜백 호출
        }
    }

    public void EnrollUser(User userData, Action action = null, Action failAction = null)
    {

        ownData = userData;
        loadingPanel.gameObject.SetActive(true);
        //rankingData.highscore = int.Parse(Utils.EncryptScore(rankingData.highscore.ToString(), key));


        action += () => {
            loadingPanel.gameObject.SetActive(false);
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.nickname,userData.nickname);
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.highScore, Math.Max(userData.highscore,
                PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.highScore)));
            MarkCloudDirty();
        };

        failAction += () => loadingPanel.gameObject.SetActive(false);
        //failAction += () => ToastText(LangManager.IsEng()? "Could't Upload": "랭킹을 등록할 수 없어요.");
        StartCoroutine(EnrollUserDataToServer(userData, action, failAction));
    }

    IEnumerator EnrollUserDataToServer(User data, Action successAction,Action failAction)
    {

        // RankingData 객체를 JSON 형식의 문자열로 변환합니다.
        string jsonData = JsonUtility.ToJson(data);

        // JSON 데이터를 바이트 배열로 변환합니다.
        byte[] jsonDataBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // POST 요청을 생성합니다.
        using UnityWebRequest www = new UnityWebRequest($"{baseURL}/enroll/", "POST");
        www.uploadHandler = new UploadHandlerRaw(jsonDataBytes);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("insert");

        // 요청을 보냅니다.
        www.timeout = requestTimeoutSeconds;
        yield return PrepareRequest(www);
        yield return www.SendWebRequest();

        // 응답을 확인합니다.
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            failAction?.Invoke();
        }
        else
        {
            // 서버로부터 받은 응답을 출력합니다.
            if (FirebaseSession.Enabled)
            {
                ownData = JsonConvert.DeserializeObject<User>(www.downloadHandler.text);
                data.highscore = ownData.highscore;
            }
            successAction?.Invoke();
            GameManager.instance.ToastText("성공적으로 로그인했어요!");
        }
    }

    public void OnlineTest(Action action = null, Action failAction = null)
    {
        loadingPanel.gameObject.SetActive(true);

        action += () => 
        {
            onlineMode = true;
            GameManager.instance.ToastText("온라인으로 접속합니다.");
            loadingPanel.gameObject.SetActive(false);
            NetworkSuccess();
        };

        failAction += () => 
        {
            onlineMode = false;
            loadingPanel.gameObject.SetActive(false);
            GameManager.instance.ToastText("오프라인으로 접속합니다.");
        };
        
        currentVersion = Application.version;

        Action VersionDidntMatchAction = () =>
        {
            if(versionDidntMatchPanel != null) versionDidntMatchPanel.SetActive(true);
        };
        onlineMode = false;
        if (string.IsNullOrEmpty(baseURL))
        {
            failAction?.Invoke();
            return;
        }
        if (FirebaseSession.Enabled) StartCoroutine(FirebaseLogin(action, failAction));
        else StartCoroutine(VersionCheck(action, VersionDidntMatchAction, failAction));
    }
    public void HeartReceiveCheck(string _code)
    {
        Debug.Log($"{_code}로 request보냄");
        StartCoroutine(FetchReceivedHeartListToServer(_code));
    }

    public void SendHeart(string target,Action success,bool noPopup = false)
    {
        StartCoroutine(SendHeartToServer(ownData.nickname, target,success,noPopup));
    }
    public IEnumerator SendHeartToServer(string senderNickName, string targetNickName,Action success, bool noPopup)
    {
        string url = $"{baseURL}/sendHeart"; // 서버 주소를 적절히 변경하세요.

        // POST 요청에 필요한 데이터를 JSON으로 만듭니다.
        HeartData heartData = new HeartData
        {
            senderNickName = senderNickName,
            targetNickName = targetNickName
        };

        string jsonData = JsonUtility.ToJson(heartData);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // 요청 설정
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = requestTimeoutSeconds;

        yield return PrepareRequest(request);

        yield return request.SendWebRequest();

        // 결과 확인
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Heart sent successfully: " + request.downloadHandler.text);
            if(noPopup == false) GameManager.instance.ToastText("성공적으로 하트를 보냈어요!");
            success?.Invoke();
        }
        else
        {
            GameManager.instance.ToastText("네트워크 연결을 확인하세요.");
            Debug.LogError($"Failed to send heart: {request.error}");
        }
    }


    IEnumerator VersionCheck(Action successAction, Action incompatibleAction, Action networkFailureAction)
    {
        WWWForm form = new WWWForm();
        form.AddField("version", currentVersion);
        using (UnityWebRequest request = UnityWebRequest.Post($"{baseURL}/versionCheck", form))
        {
            request.timeout = requestTimeoutSeconds;
            yield return PrepareRequest(request);
            yield return request.SendWebRequest();
            loadingPanel.gameObject.SetActive(false);
            if (request.result != UnityWebRequest.Result.Success ||
                !TryReadCompatibility(request.downloadHandler.text, out bool compatible))
            {
                networkFailureAction?.Invoke();
                yield break;
            }
            if (compatible) successAction?.Invoke();
            else incompatibleAction?.Invoke();
        }
    }

    public void FetchReceivedHeartList()
    {
        StartCoroutine(FetchReceivedHeartListToServer(ownData.nickname));
    }
    IEnumerator FetchReceivedHeartListToServer(string nickname)
    {
        string url = $"{baseURL}/checkMyHeartList?nickname={UnityWebRequest.EscapeURL(nickname)}";
        using UnityWebRequest request = UnityWebRequest.Get(url);

        request.timeout = requestTimeoutSeconds;

        yield return PrepareRequest(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Response: {request.downloadHandler.text}");

            // JSON 데이터를 Heart 리스트로 변환
            List<HeartData> heartList = JsonConvert.DeserializeObject<List<HeartData>>(request.downloadHandler.text);
            
            GameManager.instance.mailManager.MakeUiFromHeartList(heartList);
        }
        else
        {
            List<HeartData> heartList = new List<HeartData>();
            GameManager.instance.mailManager.MakeUiFromHeartList(heartList);
            // Debug.LogError($"Error fetching heart list: {request.error}");
        }
    }

    public void HeartReceive(string heartHash, Action success = null, Action fail = null)
    {
        StartCoroutine(HeartReceiveToServer(heartHash,success,fail));
    }
    IEnumerator HeartReceiveToServer(string heartHash, Action success = null, Action fail = null)
    {
        string jsonData = JsonConvert.SerializeObject(new { heartHash });

        // UnityWebRequest로 POST 요청 생성
        using (UnityWebRequest www = new UnityWebRequest($"{baseURL}/receiveHeart", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // 응답을 기다림
            www.timeout = requestTimeoutSeconds;
            yield return PrepareRequest(www);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Heart received successfully: " + www.downloadHandler.text);
                if (FirebaseSession.Enabled)
                    ApplyCloudGiftCount(JObject.Parse(www.downloadHandler.text).Value<int>("claimedHeartCount"));
                GameManager.instance.ToastText("성공적으로 하트를 받았어요!");
                success?.Invoke();
                // 성공 처리 로직 추가
            }
            else
            {
                Debug.LogError("Failed to receive heart: " + www.error);
                GameManager.instance.ToastText("네트워크 환경을 확인해주세요.");
                fail?.Invoke();
                // 오류 처리 로직 추가
            }
        }
    }

    // 테스트용 메서드
    public void StartUpdateTest()
    {
        StartCoroutine(UpdateHighscore(ownData.nickname, GameLogicManager.instance.currentLevel));
    }

    public IEnumerator UpdateHighscore(string nickname, int highscore)
    {
        // JSON 데이터 생성
        var formData = new WWWForm();
        formData.AddField("nickname", nickname);
        formData.AddField("highscore", highscore);

        using (UnityWebRequest request = UnityWebRequest.Post($"{baseURL}/update", formData))
        {
            // 서버로 요청 보내기
            request.timeout = requestTimeoutSeconds;
            yield return PrepareRequest(request);
            yield return request.SendWebRequest();

            // 요청 결과 확인
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                Debug.LogError($"Server Response: {request.downloadHandler.text}");
                GameManager.instance.ToastText("네트워크 연결 상태를 확인해주세요.");
            }
            else
            {
                Debug.Log($"Success: {request.downloadHandler.text}");
                GameManager.instance.ToastText("랭킹에 등록했어요!");
            }
        }
    }



    [ContextMenu("rank")]
    public void GetRank()
    {
        StartCoroutine(GetRanking());
    }
    public IEnumerator GetRanking()
    {
        string url = $"{baseURL}/getRanking";
        Debug.Log(url);

        using UnityWebRequest request = UnityWebRequest.Get(url);
        // request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = requestTimeoutSeconds;

        yield return PrepareRequest(request);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Ranking: {request.downloadHandler.text}");

            List<User> userList = JsonConvert.DeserializeObject<List<User>>(request.downloadHandler.text);
            // RankingList에 저장된 순위를 출력
            GameManager.instance.rankingManager.ShowUiList(userList);
        }
        else
        {
            Debug.LogError($"Error fetching ranking: {request.error}");
        }
    }
    public void GoldChange(int value)
    {
        var gold = int.Parse(PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.gold));

        gold  += value;

        if(gold < 0) gold = 0;

        PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.gold,gold);

        if(FindFirstObjectByType<LobbyManager>().goldText != null)
        {
            StartCoroutine(AnimateGoldChange(value,FindFirstObjectByType<LobbyManager>().goldText));
        }
    }

    public void GoldChange(int value,TMP_Text goldText = null)
    {
        Debug.Log(PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.gold));
        var gold = int.Parse(PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.gold));

        gold  += value;

        if(gold < 0) gold = 0;

        PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.gold,gold);
        if(goldText != null) StartCoroutine(AnimateGoldChange(value,goldText));
    }

    private IEnumerator AnimateGoldChange(int value,TMP_Text goldText)
    {
        int startGold = int.Parse(goldText.text); // 현재 UI의 골드 값
        int targetGold = startGold + value;      // 목표 골드 값

        if (targetGold < 0) targetGold = 0;

        float duration = 1.5f;                   // 애니메이션 시간
        float elapsed = 0f;                      // 경과 시간

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 등속으로 숫자 증가/감소
            int currentGold = Mathf.RoundToInt(Mathf.Lerp(startGold, targetGold, elapsed / duration));
            goldText.text = currentGold.ToString();

            yield return null; // 다음 프레임까지 대기
        }

        // 마지막 값 보정
        goldText.text = targetGold.ToString();
    }


    public int GetGold()
    {
        return PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.gold);
    }


}