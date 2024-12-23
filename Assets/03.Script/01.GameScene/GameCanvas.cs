using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameCanvas : CanvasBase
{
    public Button getBallDownButton;
    public Action getBallDownAction;

    public SelectPanel selectPanel;

    public Image greatImage;
    public GameObject gameOverPanel;
    public Button retryButton;

    public List<GameObject> backImgs;

    public TMP_Text highScoreText;

    private void Start()
    {
        getBallDownButton.onClick.AddListener(()=>getBallDownAction.Invoke());

        AdsInitializer.instance.bannerAd.ShowBannerAd();

        highScoreText.text = PlayerPrefsManager.Instance.GetIntSetting(PlayerPrefsData.highScore).ToString();
    }

    internal void ShowGreat()
    {
        greatImage.gameObject.SetActive(true);
        greatImage.gameObject.transform.DOShakeScale(1.3f);
        
        Utils.DelayCall(() => { greatImage.gameObject.SetActive(false); }, 1.3f);
    }

    internal void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        GameLogicManager.instance.cantBallInteract = true;


        
        retryButton.onClick.AddListener(()=>
        {
            // string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("01.LobbySceneSheep");
            PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.hasLastGame,false);
        });
    }

    public void SetBackImgsSetting(int curLevel)
    {
        // curLevel에 따라 끌 인덱스 계산
        int disableCount = curLevel / 50; // 50마다 하나씩 끄기

        if(curLevel > 199) return;
        // backImgs 배열을 순회하며 끔
        for (int i = 0; i < backImgs.Count; i++)
        {
            if (i < disableCount)
            {
                backImgs[i].SetActive(false); // 끄기
            }
            else
            {
                backImgs[i].SetActive(true); // 켜기 (필요하면 포함)
            }
        }
    }
}
