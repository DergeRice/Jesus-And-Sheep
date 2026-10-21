using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankingUI : MonoBehaviour
{
    public int rankingNum;
    public Image medalImg;
    public TMP_Text rankNumText,rankScore, rankNickName;

    private void Awake()
    {
        if (rankNickName != null && rankNickName.GetComponent<NonTranslatableText>() == null)
        {
            rankNickName.gameObject.AddComponent<NonTranslatableText>();
        }
    }

    public void SettingUi(User rankingData)
    {
        // profileObj[rankingData.profileindex].SetActive(true);
        rankNumText.text = rankingData.id.ToString();
        rankScore.text = rankingData.highscore.ToString();

        rankNickName.text = rankingData.nickname.ToString();
    }

}
