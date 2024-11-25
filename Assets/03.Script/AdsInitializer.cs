using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Advertisements;
 
public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    public static AdsInitializer instance;
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;

    public bool isRemovedAds = false;

    public BannerAd bannerAd;
    public InterstitialAd interstitialAd;

    public bool isLoadedAds;
 
    [ContextMenu("DeleteInfo")]
    public void RemovePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    void Awake()
    {
        transform.SetParent(null);

        if(instance != null)
        {
            Destroy(instance.gameObject);
            instance = this;
        } 
        
        if(instance == null ) instance = this;                      

        DontDestroyOnLoad(gameObject);
        InitializeAds();

        isRemovedAds = PlayerPrefs.GetInt("isRemovedAds") == 1 ? true : false;

        
        
    }

    public void RemoveAd(bool isRemoved)
    {
        isRemovedAds = isRemoved;
        bannerAd.HideBannerAd();
        Debug.Log("ad" + isRemoved);
        PlayerPrefs.SetInt("isRemovedAds",isRemoved? 1 : 0);

        //string removeText = LangManager.IsEng() ? "Removed Ads!" : "광고제거 성공";
        //NetworkManager.instance.ToastText(removeText);
    }
 
    public void InitializeAds()
    {
    #if UNITY_IOS
            _gameId = _iOSGameId;
    #elif UNITY_ANDROID
            _gameId = _androidGameId;
    #elif UNITY_EDITOR
            _gameId = _androidGameId; //Only for testing the functionality in the Editor
    #endif
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

 
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        interstitialAd = GetComponent<InterstitialAd>();

        interstitialAd.LoadAd();
        bannerAd.LoadBanner();
    }
 
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}