using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbySceneUIManager : MonoBehaviour
{
    public Button startButton,countinueButton;

    public GameObject topBar, underBar, title, fence, btns;
    private AsyncOperation asyncLoad;

    private void Start()
    {
        startButton.onClick.AddListener(() => { StartGame(); });
        countinueButton.onClick.AddListener(() => { ContinueGame(); });

        countinueButton.interactable = PlayerPrefsManager.Instance.GetBoolSetting(PlayerPrefsData.hasLastGame);

    }
    public void StartGame()
    {
        GameManager.instance.isContinueMode = false;
        PreloadGameScene();
        StartAnimation();
    }

    public void ContinueGame()
    {
        GameManager.instance.isContinueMode = true;
        PreloadGameScene();
        StartAnimation();
    }

    public void StartAnimation()
    {
        topBar.transform.DOBlendableMoveBy(new Vector3(0, 500, 0), 1f);
        title.transform.DOBlendableMoveBy(new Vector3(0, 1300, 0), 1f).SetDelay(0.8f);
        underBar.transform.DOBlendableMoveBy(new Vector3(0, -500, 0), 1f);
        btns.transform.DOBlendableMoveBy(new Vector3(0, -1000, 0), 1f).SetDelay(0.8f);
        fence.transform.DOBlendableMoveBy(new Vector3(0, -1600, 0), 1f).SetDelay(1.2f); // 애니메이션 끝난 후 씬 활성화
    }

    private void PreloadGameScene()
    {
        // 씬 로딩 시작
        asyncLoad = SceneManager.LoadSceneAsync("02.GameScene");
        asyncLoad.allowSceneActivation = false; // 씬 활성화를 지연

        Utils.DelayCall(() => { ActivateGameScene(); }, 1.14f);
    }

    private void ActivateGameScene()
    {
        // 애니메이션 완료 후 씬 활성화
        asyncLoad.allowSceneActivation = true;
    }
}
