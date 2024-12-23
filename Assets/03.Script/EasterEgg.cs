using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class EasterEgg : MonoBehaviour
{
    public GameObject root;
    public int currentIndex;

    PlayableDirector playableDirector;

    public List<GameObject> msgs = new List<GameObject>();

    private void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        if(PlayerPrefs.HasKey("EasterEgg") == false)
        {
            PlayerPrefs.SetInt("EasterEgg",0);
        }
        currentIndex = PlayerPrefs.GetInt("EasterEgg");

        SetMsgs(currentIndex);
    }

    public void SetMsgs(int index)
    {
        msgs[index].SetActive(true);
    }

    public void PlayEasterEgg()
    {
        playableDirector.Play();
        SoundManager.instance.bgmAudioSource.Pause();
        NetworkManager.instance.SendHeart("이상진",()=>{},true);
    }

    public void EndEasterEgg()
    {
        SoundManager.instance.bgmAudioSource.Play();

        Debug.Log("end");
        root.SetActive(false);
        currentIndex ++;
        if(currentIndex >= msgs.Count) currentIndex = 0;
        PlayerPrefs.SetInt("EasterEgg",currentIndex);
    }
}
