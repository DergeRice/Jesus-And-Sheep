using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using AutoLetterbox;


public class IntroScreen : MonoBehaviour
{
    public CanvasGroup icon;
    public AudioSource audioSource;
    private void Start()
    {
        Utils.DelayCall(()=> SceneManager.LoadScene("01.LoadingSecne"),4f);

        Utils.DelayCall(()=> SceneManager.LoadScene("01.LoadingSecne"),4f);
        icon.DOFade(0,1).SetDelay(3f).SetEase(Ease.Linear);
        
        Utils.DelayCall(()=> audioSource.Play(),1f);
    }
}
