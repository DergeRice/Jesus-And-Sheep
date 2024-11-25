using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpotLightBall : MonoBehaviour
{
    public bool actionBtnTouched = false;
    // Start is called before the first frame update
    void Start()
    {
        actionBtnTouched = false;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.touchCount > 0)
        {
            // 첫 번째 터치 입력을 가져옵니다.
            Touch touch = Input.GetTouch(0);

            // 터치 위치를 가져옵니다.
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));


            if(actionBtnTouched == true) return;

            if ( touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // z 값을 현재 z 값으로 유지하여 2D 평면에서의 위치를 지정합니다.
                touchPosition.z = transform.position.z;

                // 오브젝트의 위치를 터치 위치로 설정합니다.
                transform.position = touchPosition;
            }
        }
    }
    
}
