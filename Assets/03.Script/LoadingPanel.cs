using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    public Transform loadingIcon;
    public TMP_Text loadingText;

    // Update is called once per frame
    private string[] loadingTexts = { "Lording.", "Lording..", "Lording..." };
    private int currentTextIndex = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(UpdateLoadingText());
    }

    // Update is called once per frame
    void Update()
    {
       // loadingIcon.Rotate(0f, 0f, -1f);
    }

    IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            loadingText.text = loadingTexts[currentTextIndex];
            currentTextIndex = (currentTextIndex + 1) % loadingTexts.Length;
            yield return new WaitForSeconds(0.8f);
        }
    }
}
