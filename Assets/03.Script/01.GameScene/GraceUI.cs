using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraceUI : MonoBehaviour
{
    public Image ballImage;
    public TMP_Text expainText, indexText;
    public void Init(Sprite sprite, string v,int index)
    {
        ballImage.sprite = sprite;
        expainText.text = v;
        indexText.text = index.ToString();
    }
}
