using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    // public Button mainMenu;
    public List<GameObject> tabCanvas = new List<GameObject>();
    public List<Vector3> tabCanvasInitPos = new List<Vector3>();
    public Sprite defaultImg, chosenImg;
    public List<GameObject> tabs = new List<GameObject>();

    private int curIndex = 6;

    private void Start()
    {
        Debug.Log("Ddas");
        for (int i = 0; i < tabCanvas.Count; i++)
        {
            tabCanvasInitPos.Add(tabCanvas[i].GetComponent<RectTransform>().position);
        }

        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[index].GetComponent<Button>().onClick.AddListener(() => EnableTab(index));
        }
        // mainMenu.set
        EnableTab(2);
    }

    public void EnableTab(int index)
    {
        if(index == curIndex) return;
        if(index == 2)
        {

        }
        if (index == 4)
        {
            GameManager.instance.ToastText("다음 업데이트를 기대해 주세요!");
            return;
        }

        // 현재 인덱스와 비교
        int previousIndex = curIndex;
        curIndex = index;

        Debug.Log("Dd");
        SetAllDisable();

        // 애니메이션 처리
        for (int i = 0; i < tabCanvas.Count; i++)
        {
            if (i == index) // 활성화된 캔버스
            {
                tabCanvas[i].GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.3f); // 중앙으로 이동
            }
            else if (i == previousIndex) // 이전 캔버스
            {
                // 왼쪽에서 오른쪽으로 밀기 (활성화된 탭보다 작음)
                if (index > previousIndex)
                {
                    tabCanvas[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(-Screen.width * 1.5f, 0), 0.3f);
                }
                // 오른쪽으로 밀기 (활성화된 탭보다 큼)
                else
                {
                    tabCanvas[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width * 1.5f, 0), 0.3f);
                }
            }
            else // 다른 캔버스들 원래 위치로
            {
                tabCanvas[i].GetComponent<RectTransform>().DOAnchorPos3D(tabCanvasInitPos[i], 0.3f);
            }
        }

        // 선택된 탭 설정
        tabs[index].GetComponent<Image>().sprite = chosenImg;
        tabs[index].transform.localScale = Vector3.one * 1.2f;
        tabs[index].GetComponent<Canvas>().sortingOrder = 2;
    }

    public void SetAllDisable()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].transform.localScale = Vector3.one;
            tabs[i].GetComponent<Image>().sprite = defaultImg;
            tabs[i].GetComponent<Canvas>().sortingOrder = 1;
        }
    }
}
