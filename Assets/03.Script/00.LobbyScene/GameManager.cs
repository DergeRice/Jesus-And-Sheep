using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isLastGameRemain;

    public bool isContinueMode = false;


    private TMP_Text toastText;

    public GameObject toastUIPrefeb;

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
        var toastPanel = Instantiate(toastUIPrefeb, toastRoot).GetComponent<CanvasGroup>();

        toastText = toastPanel.transform.GetChild(0).GetComponent<TMP_Text>();

        toastText.text = text;
        Destroy(toastPanel.gameObject, 2f);
    }
}
