using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankingUI : MonoBehaviour
{
    public int rankingNum;

    public List<GameObject> profileObj = new List<GameObject>();
    public Image medalImg;
    public TMP_Text rankNumText,rankScore,rankChurchName, rankNickName;

    public void SettingUi(RankingData rankingData)
    {
        profileObj[rankingData.profileIndex].SetActive(true);
        rankNumText.text = rankingData.id.ToString();
        rankScore.text = rankingData.score.ToString();

        rankChurchName.text = rankingData.churchName.ToString();
        rankNickName.text = rankingData.name.ToString();
    }

}
