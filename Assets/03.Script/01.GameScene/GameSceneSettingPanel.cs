using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AssetKits.ParticleImage.Enumerations;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameSceneSettingPanel : MonoBehaviour
{
    private Vector3 orignPos;
    private RectTransform rectTransform;
    public Button settingButton, closeButton;

    public TMP_Text activeStateText , speedText;
    public Button homeButton, speedButton, soundButton;
    public Button homeConfirm, speedBuyConfirm;
    public Button closeHomePanel, closeSpeedBuyPanel;

    public GameObject speedPopup, homePopup, speedModeEnable;

    public bool speedmodeUnlock, isSpeedMode;

    public Image soundImg;
    public Sprite soundOffImg,soundOnImg;

    private BallListShowPanel ballListShowPanel;

    public bool cantBallInteract;

    public GameObject goldIndicator;

    public TMP_Text goldCount;

    private void Start()
    {
        ballListShowPanel = GetComponent<BallListShowPanel>();
        ///init datas
        activeStateText.text = "비활성화됨";
        soundImg.sprite = SoundManager.instance.isBgmOff?  soundOffImg : soundOnImg;

        speedText.text = "x1";
        Time.timeScale = 1.0f;
        isSpeedMode = false;

        rectTransform = GetComponent<RectTransform>();
        orignPos = rectTransform.position;

        if(PlayerPrefsManager.Instance.GetBoolSetting(PlayerPrefsData.lastGameUnlockSpeedMode))
        {
            speedmodeUnlock = true; 
            speedModeEnable.SetActive(true);
            activeStateText.text = "활성화됨";
        }


        /////init buttons;
        settingButton.onClick.AddListener(()=>
        {
            rectTransform.position = Vector3.zero;
            GameLogicManager.instance.isPlayerTurn = false;
            cantBallInteract = true;
            ballListShowPanel.MakeUiFromDatas(GameLogicManager.instance.shootingBallDatas);
            });
        closeButton.onClick.AddListener(()=>
        {
            rectTransform.position = orignPos;
            if(GameLogicManager.instance.ballList.Count == 0)
            {
                GameLogicManager.instance.isPlayerTurn = false;
                cantBallInteract = false;
                Utils.DelayCall(()=>{ GameLogicManager.instance.isPlayerTurn = true;},0.05f);
            }

            });
        soundButton.onClick.AddListener(()=>
        {
            SoundManager.instance.ToggleBgm();
            soundImg.sprite = SoundManager.instance.isBgmOff?  soundOffImg : soundOnImg;
        });
        homeButton.onClick.AddListener(()=>{ShowHomePopup(true);});
        closeHomePanel.onClick.AddListener(()=>{ShowHomePopup(false);});
        speedButton.onClick.AddListener(()=>{ShowSpeedPopup(true);});
        closeSpeedBuyPanel.onClick.AddListener(()=>{ShowSpeedPopup(false);});

        speedBuyConfirm.onClick.AddListener(()=>
        {
            if(NetworkManager.instance.GetGold() < 200)
            {
                GameManager.instance.ToastText("골드가 부족해요!");
                return;
            }
            NetworkManager.instance.GoldChange(-200,goldCount);
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.lastGameUnlockSpeedMode,true);
            speedmodeUnlock = true; 
            speedModeEnable.SetActive(true);
            speedPopup.SetActive(false);
            activeStateText.text = "활성화됨";
            ShowGoldIndicator(true,false);
        });
        homeConfirm.onClick.AddListener(()=>
        {
            GoHome();
        });
        
    }

    public void ShowSpeedPopup(bool enable)
    {
        if(speedmodeUnlock == false)
        {
            speedPopup.SetActive(enable);
            ShowGoldIndicator(enable,true);

        }else
        {
            ToggleSpeed();
        }
    }

    public void ShowHomePopup(bool enable)
    {
        homePopup.SetActive(enable);
    }

    public void GoHome()
    {
        GameLogicManager.instance.gameDataSaveManager.SaveShotStatus();
        SceneManager.LoadScene("01.LobbySceneSheep");
    }

    public void ToggleSpeed()
    {
        isSpeedMode = !isSpeedMode;
        Time.timeScale = isSpeedMode ? 2.0f : 1.0f;
        speedText.text = isSpeedMode? "x2" : "x1";
    }

    private void Update()
    {
        if(cantBallInteract) GameLogicManager.instance.isPlayerTurn = false;
    }

    public void ShowGoldIndicator(bool enable,bool isShowStay = false, int changedValue = 0)
    {
        goldIndicator.SetActive(enable);
        goldCount.text = NetworkManager.instance.GetGold().ToString();

        if(changedValue != 0) NetworkManager.instance.GoldChange(changedValue,goldCount);

        if(enable == true && isShowStay == false) Utils.DelayCall(()=>
        {
            goldIndicator.transform.DOBlendableMoveBy(new Vector3(0, 300, 0), 2f);
            
        },2.5f);
        if(enable == true && isShowStay == false) Utils.DelayCall(()=>
        {
            goldIndicator.SetActive(false);
        },3.5f);
        
    }
} 
