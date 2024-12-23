using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;



public class RankingManager : MonoBehaviour
{
    public GameObject uiPrefeb;
    public Sprite[] rankingUiSprites = new Sprite[3];
    public  List<RankingUI> rankingUIs = new List<RankingUI>();

    public Transform uiContainer;

    // public List<GameObject> profileImgs = new List<GameObject>();
    public List<User> rankDatas;
    // List<User> survivalRankingDatas;


    // public Button churchTab, globalTab;

    [Header("Mydata")]
    public TMP_Text myScore, myRankIndex;


    private void Awake()
    {
        // disabledColor = churchTab.GetComponent<Image>().color;
        // enableColor = globalTab.GetComponent<Image>().color;
    }

    private void Start()
    {
        GameManager.instance.rankingManager = this;
        // churchTab.onClick.AddListener(ShowChurchRanking);
        // globalTab.onClick.AddListener(ShowGlobalRanking);
    }

    public void ShowUiList(List<User> datas)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            int index = i;
            var temp = Instantiate(uiPrefeb,uiContainer).GetComponent<RankingUI>();
            temp.SettingUi(datas[index]);
            temp.rankNumText.text = (index+1).ToString();

            if(index < 3)
            {
                temp.medalImg.sprite = rankingUiSprites[index];
            }else
            {
                temp.medalImg.gameObject.SetActive(false);
            }
        }
    }

    public void SetMyData(List<User> datas, bool isChurchRanking)
    {
        var matchingData = datas.FirstOrDefault(data => data.nickname == NetworkManager.instance.ownData.nickname);
        // profileImgs[NetworkManager.instance.ownData.profileindex].SetActive(true);

        if(matchingData != null)
        {
            myScore.text = matchingData.highscore.ToString();
            int rankingIndex = datas.IndexOf(matchingData) +1;
            string additionText;
            
            if(rankingIndex == 1) additionText = "st";
            else if(rankingIndex == 1) additionText = "nd";
            else additionText = "th";

            myRankIndex.text = rankingIndex.ToString()+additionText;

            //CanvasManager.DisableChild(profileImgs[0].transform.parent.gameObject);
            // profileImgs[NetworkManager.instance.ownData.profileindex].SetActive(true);
        }else 
        {    // 매칭되는게 없을대 한번 더 검색
            // 교회랭킹일텐데 확인하기

            // 아예 없으면 no score, 교회 탭에서 교회같으면 교회 이름 출력

            // if(isChurchRanking == true)
            // {
            //     var ourChurchData = datas.FirstOrDefault(data => data.churchName == NetworkManager.instance.ownData.churchName);
            //     if(ourChurchData != null)
            //     {
            //         int rankingIndex = datas.IndexOf(ourChurchData) +1;
            //         string additionalText;
                    
            //         if(rankingIndex == 1) additionalText = "st";
            //         else if(rankingIndex == 1) additionalText = "nd";
            //         else additionalText = "th";

            //         myRankIndex.text = rankingIndex.ToString()+additionalText;
            //         myScore.text = ourChurchData.churchName;
            //     }
            // }else  // global ranking이고, 값이 일치하는게 없을 때,
            {
                myRankIndex.text = "no rank";
                myScore.text = "no Score";
            }
        }
    }


    public void CleanBoard()
    {
        for (int i = 0; i < uiContainer.childCount; i++)
        {
            Destroy(uiContainer.GetChild(i).gameObject);
        }
    }
    private void OnEnable()
    {
        // NetworkManager.instance.GetData(()=>BringDatas(),()=>{gameObject.SetActive(false);});
        
    }

}
