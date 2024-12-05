using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LobbySceneUIManager : MonoBehaviour
{
    public Button startButton,countinueButton;

    private void Start()
    {
        startButton.onClick.AddListener(() => { StartGame(); });
        countinueButton.onClick.AddListener(() => { CountinueGame(); });
    }
    public void StartGame()
    {

    }
    public void CountinueGame()
    {

    }
}
