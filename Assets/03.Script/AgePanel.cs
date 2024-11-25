using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class AgePanel : MonoBehaviour
{
    public TMP_InputField yearInput, monthInput, dateInput;
    public Button checkAgeBtn;
    public GameObject quitPanel;

    // Start is called before the first frame update
    void Start()
    {
        
        checkAgeBtn.onClick.AddListener(()=>{

            int yearInt, monthInt, dateInt;
            int.TryParse(yearInput.text,out yearInt);
            int.TryParse(monthInput.text,out monthInt);
            int.TryParse(dateInput.text,out dateInt);


            if(IsOver13YearsOld(yearInt,monthInt,dateInt))
            {
                gameObject.SetActive(false);
            }else
            {
                quitPanel.SetActive(true);
                PlayerPrefs.SetInt("age",1);
            }
            } );

            if(PlayerPrefs.GetInt("age") == 1) 
            {
                // quitPanel.SetActive(true);
            }
    }

    public bool IsOver13YearsOld(int birthYear, int birthMonth, int birthDay)
    {
        // 현재 날짜를 가져옵니다.
        DateTime today = DateTime.Today;

        // 입력된 생년월일을 DateTime으로 변환합니다.
        DateTime birthDate = new DateTime(birthYear, birthMonth, birthDay);

        // 13년 전 날짜를 계산합니다.
        DateTime thirteenYearsAgo = today.AddYears(-13);

        // 13년 전 날짜보다 생년월일이 더 이전 날짜인지 확인합니다.
        return birthDate <= thirteenYearsAgo;
    }
}
