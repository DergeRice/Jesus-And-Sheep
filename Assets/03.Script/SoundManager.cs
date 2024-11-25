using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public List<AudioClip> popSounds;
    public AudioClip dropSound, endSound,startSound , jesusSound, magicSound;
    public AudioClip tick, tickEnd;
    public List<AudioClip> bgms = new List<AudioClip>();

    public AudioSource bgmAudioSource, sfxAudioSource;


    [Header("PlayerSettingPrefs")]
    public bool isBgmOff, isSfxOff, isVibrateOff;

    public SettingPanel settingPanel;

    public AudioMixer audioMixer;

     #if UNITY_IOS
    [DllImport("__Internal")] public static extern void Vibrate(int _n);
    #endif
    private void Awake()
    {
        if(instance != null) Destroy(gameObject);
        if(instance == null ) instance = this;
        gameObject.transform.SetParent(null);
        DontDestroyOnLoad(this);

        isBgmOff = PlayerPrefs.GetInt("isBgmOff") == 1 ? true : false;
        isSfxOff = PlayerPrefs.GetInt("isSfxOff") == 1 ? true : false;
        isVibrateOff = PlayerPrefs.GetInt("isVibrateOff") == 1 ? true : false;


    }
    private void Start()
    {
        if(isBgmOff == true) audioMixer.SetFloat("Bgm",-80);
        else bgmAudioSource.Play();
        if(isSfxOff == true) audioMixer.SetFloat("Sfx",-80);
        
    }

    public void PlayPopSound()
    {
        sfxAudioSource.PlayOneShot(popSounds[UnityEngine.Random.Range(0,popSounds.Count)]);
    }
    public void PlayMagicSound()
    {
        sfxAudioSource.PlayOneShot(magicSound);
    }

    internal void PlayDropSound()
    {
        sfxAudioSource.PlayOneShot(dropSound);
    }

    public static void VibrateGame(EVibrate eVibrate)
    {
        if(SoundManager.instance.isVibrateOff == true) return; 
        bool isIOS = false;

        #if UNITY_IOS && !UNITY_EDITOR
        isIOS = true;
        #endif

        if(isIOS)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("Ios"+eVibrate.ToString());
            switch (eVibrate)
            {
                case EVibrate.weak:
                Vibrate(1519);
                    break;
                case EVibrate.strong:
                Vibrate(1520);
                    break;
                case EVibrate.nope:
                Vibrate(1521);
                    break;
            }
            #endif

        }else
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("android"+eVibrate.ToString());
            switch (eVibrate)
            {
                case EVibrate.weak:
                Vibration.Vibrate((long)80);
                    break;
                case EVibrate.strong:

                Vibration.Vibrate((long)500);
                    break;
                case EVibrate.nope:

                long[] pattern = new long[6];
                pattern[0] = 00; // wait
                pattern[1] = 100; // boom
                pattern[2] = 50; // wait
                pattern[3] = 100; //boom
                pattern[4] = 50; // wait
                pattern[5] = 100; //boom
                Vibration.Vibrate(pattern,-1);
                    break;
            }
            #endif
        }
    }

    public void ToggleBgm()
    {
        isBgmOff = !isBgmOff;
        PlayerPrefs.SetInt("isBgmOff",isBgmOff? 1 : 0);
    
        audioMixer.SetFloat("Bgm",isBgmOff ? -80 : 0);

        if(isBgmOff == false) bgmAudioSource.Play();
        
    }
    public void ToggleSfx()
    {
        isSfxOff = !isSfxOff;
        PlayerPrefs.SetInt("isSfxOff",isSfxOff? 1 : 0);
        
        audioMixer.SetFloat("Sfx",isSfxOff ? -80 : 0);
    }
    public void ToggleVibrate()
    {
        isVibrateOff = !isVibrateOff;
        PlayerPrefs.SetInt("isVibrateOff",isVibrateOff? 1 : 0);
        if(isVibrateOff == false) VibrateGame(EVibrate.weak);
    }

    public void StartGame()
    {
        bgmAudioSource.PlayOneShot(startSound);
        StartCoroutine(WaitBgm(startSound));
    }
    IEnumerator WaitBgm(AudioClip audioClip)
    {
        // startSound의 길이만큼 기다립니다.
        yield return new WaitForSeconds(audioClip.length);

        // BGM을 재생합니다.
        bgmAudioSource.Play();
    }

    

    public void EndGame()
    {
        bgmAudioSource.Pause();
        bgmAudioSource.PlayOneShot(endSound);
        StartCoroutine(WaitBgm(endSound));
    }

    public void JesusCome()
    {  
        bgmAudioSource.Pause();
        sfxAudioSource.PlayOneShot(jesusSound);
        
        StartCoroutine(WaitBgm(jesusSound));
    }

    public void ChangeBgm(int index)
    {
        bgmAudioSource.clip = bgms[index];
        //if(index == 0) bgmAudioSource.Play();
        if(index != 0) StartGame();
    }

    public void Ticking(bool isEnd)
    {
        
        sfxAudioSource.PlayOneShot(isEnd? tick : tickEnd);
    }

}
