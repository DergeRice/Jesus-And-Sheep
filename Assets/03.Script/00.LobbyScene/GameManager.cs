using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isLastGameRemain;

    public bool isContinueMode = false;
    public bool isLastGameSpeed = false;
    
    public Transform toastRoot;

    public GameObject toastUIPrefeb;

    public RankingManager rankingManager;

    public FriendManager friendManager;
    public LobbyManager lobbyManager;

    public BadWordManager badWordManager;
    public MailManager mailManager;
    public HeartManager heartManager;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        isLastGameRemain = PlayerPrefsManager.Instance.GetBoolSetting(PlayerPrefsData.hasLastGame);
    }



    public void ToastText(string text)
    {
        var toastText = Instantiate(toastUIPrefeb, toastRoot).GetComponent<ToastUI>();

        toastText.toastText.text = text;
        Destroy(toastText.gameObject, 2f);
    }
    
}
