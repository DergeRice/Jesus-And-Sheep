using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardPanel : MonoBehaviour
{
    public GameObject root;

    public GameObject particleObj;
    public TMP_Text coinCountText, havingGoldAmount;
    

    int coinGainAmount;
    [ContextMenu("gain80")]
    public void ShowPanel()
    {
        coinGainAmount = 80;
        root.SetActive(true);
        coinCountText.text = "x"+"80";
    }
    public void ShowPanel(int coinCount)
    {
        coinGainAmount = coinCount;
        root.SetActive(true);
        coinCountText.text = "x"+coinCount.ToString();
    }

    public void ClosePanel()
    {
        particleObj.SetActive(true);
        StartCoroutine(IncreaseValueCoroutine(coinGainAmount));
        root.SetActive(false);
    }

    private void OnDisable()
    {
        //particleObj.SetActive(false);
    }

    IEnumerator IncreaseValueCoroutine(int coinGainAmount)
    {
        int startInt = int.Parse(havingGoldAmount.text); // 현재 골드 금액을 정수로 파싱
        int goal = startInt + coinGainAmount; // 목표 금액 설정
        float elapsedTime = 0f; // 경과 시간 초기화

        while (elapsedTime < 2.5f)
        {
            elapsedTime += Time.deltaTime; // 경과 시간 업데이트
            float progress = Mathf.Clamp01(elapsedTime / 2.5f); // 0에서 1 사이의 진행률 계산
            int currentGold = (int)Mathf.Lerp(startInt, goal, progress); // 현재 금액 계산
            havingGoldAmount.text = currentGold.ToString(); // UI 업데이트

            yield return null; // 다음 프레임까지 대기
        }

        // 최종적으로 목표 금액을 정확하게 설정
        havingGoldAmount.text = goal.ToString();
        NetworkManager.instance.GoldChange(coinGainAmount); // 골드 변경 요청
        particleObj.SetActive(false); // 파티클 비활성화
    }
    
}
