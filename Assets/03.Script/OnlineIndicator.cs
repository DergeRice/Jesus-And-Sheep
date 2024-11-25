using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnlineIndicator : MonoBehaviour
{
    public Color red,green;

    // public TMP_Text indicatorText;
    public Image icon;

    public Sprite onIcon, offIcon;

    private void Start()
    {
        SetOnlineState(NetworkManager.instance.onlineMode);
    }
    public void SetOnlineState(bool isOnline)
    {
        // indicatorText.text = isOnline ? "Online":"Offline";
        icon.color = isOnline ? green : red ;
        icon.sprite = isOnline ? onIcon : offIcon;
    }
}
