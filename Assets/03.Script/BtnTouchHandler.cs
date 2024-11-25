using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnTouchHandler : MonoBehaviour , IPointerDownHandler
{
    public SpotLightBall spotLightBall;

    public void OnPointerDown(PointerEventData eventData)
    {
        spotLightBall.actionBtnTouched = true;
    }
    private void OnDisable()
    {
        spotLightBall.actionBtnTouched = false;
    }
}
