using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;


public static class LinqExtensions
{
    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
    {
        return source.OrderByDescending(selector).FirstOrDefault();
    }
}

public class RankingManager : MonoBehaviour
{
    public GameObject uiPrefeb;
    public Sprite[] rankingUiSprites = new Sprite[3];
    public  List<RankingUI> rankingUIs = new List<RankingUI>();

    public Transform uiContainer;

    public List<GameObject> profileImgs = new List<GameObject>();
    List<RankingData> classicRankingDatas;
    List<RankingData> survivalRankingDatas;


    public Button churchTab, globalTab;

    [Header("Mydata")]
    public TMP_Text myScore, myRankIndex;

    Color disabledColor, enableColor;

    private void Awake()
    {
        disabledColor = churchTab.GetComponent<Image>().color;
        enableColor = globalTab.GetComponent<Image>().color;
    }

    private void Start()
    {
        churchTab.onClick.AddListener(ShowChurchRanking);
        globalTab.onClick.AddListener(ShowGlobalRanking);
    }

    public void ShowChurchRanking()
    {
        CleanBoard();
        churchTab.GetComponent<Image>().color = enableColor;
        globalTab.GetComponent<Image>().color = disabledColor;

        SetMyData(survivalRankingDatas,true);
        ShowUiList(survivalRankingDatas);
    }
    public void ShowGlobalRanking()
    {
        CleanBoard();
        churchTab.GetComponent<Image>().color = disabledColor;
        globalTab.GetComponent<Image>().color = enableColor;


        SetMyData(classicRankingDatas,false);
        ShowUiList(classicRankingDatas);
    }

    public void ShowUiList(List<RankingData> datas)
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

    public void SetMyData(List<RankingData> datas, bool isChurchRanking)
    {
        var matchingData = datas.FirstOrDefault(data => data.name == NetworkManager.instance.ownData.name && data.churchName == NetworkManager.instance.ownData.churchName);
        profileImgs[NetworkManager.instance.ownData.profileIndex].SetActive(true);

        if(matchingData != null)
        {
            myScore.text = matchingData.score.ToString();
            int rankingIndex = datas.IndexOf(matchingData) +1;
            string additionText;
            
            if(rankingIndex == 1) additionText = "st";
            else if(rankingIndex == 1) additionText = "nd";
            else additionText = "th";

            myRankIndex.text = rankingIndex.ToString()+additionText;

            //CanvasManager.DisableChild(profileImgs[0].transform.parent.gameObject);
            profileImgs[NetworkManager.instance.ownData.profileIndex].SetActive(true);
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
        NetworkManager.instance.GetData(()=>BringDatas(),()=>{gameObject.SetActive(false);});
        
    }

    private void BringDatas()
    {
        classicRankingDatas = NetworkManager.instance.classicRankingDatas
        .Where(data => data.gameMode == "classic" || data.gameMode == null)  
            .ToList();


        survivalRankingDatas = NetworkManager.instance.classicRankingDatas
        .Where(data => data.gameMode == "survival")  
            .ToList();

        SetMyData(classicRankingDatas,true);
        ShowUiList(classicRankingDatas);
        ShowGlobalRanking();
    }
}
