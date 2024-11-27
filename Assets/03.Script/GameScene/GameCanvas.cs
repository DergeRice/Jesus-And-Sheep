using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public class GameCanvas : CanvasBase
{
    public Button getBallDownButton;
    public Action getBallDownAction;

    public SelectPanel selectPanel;

    public Image greatImage;
    public GameObject gameOverPanel;
    public Button retryButton;

    private void Start()
    {
        getBallDownButton.onClick.AddListener(()=>getBallDownAction.Invoke());
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
        GameLogicManager.instance.isPlayerTurn = false;
        retryButton.onClick.AddListener(()=>
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        });
    }
}
