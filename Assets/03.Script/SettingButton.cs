using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SettingButton : MonoBehaviour
{
    [HideInInspector] public Button button;
    public Image icon;

    [HideInInspector] public Action action, initAction;

    public bool disabled;


    public void Init()
    {
        button = GetComponent<Button>();
    }
}
