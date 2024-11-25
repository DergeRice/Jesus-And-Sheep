using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class GameCanvas : CanvasBase
{
    public Button getBallDownButton;
    public Action getBallDownAction;

    private void Start()
    {
        getBallDownButton.onClick.AddListener(()=>getBallDownAction.Invoke());
    }
}
