using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailManager : MonoBehaviour
{
    private Vector3 initPos;
    private RectTransform rectTransform;
    public Button openMailBox, close;
    public HeartReceiveUI heartReceiveUIprefab;
    public Transform prefabParent;
    public GameObject alertMark;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initPos = rectTransform.transform.position;

        GameManager.instance.mailManager = this;
        openMailBox.onClick.AddListener(()=>
        {
            rectTransform.position = Vector3.zero;
            NetworkManager.instance.FetchReceivedHeartList();
        });
        close.onClick.AddListener(()=>rectTransform.position = initPos);
    }

    public void MakeUiFromHeartList(List<HeartData> heartDatas)
    {
        for (int i = 0; i < prefabParent.childCount; i++)
        {
            Destroy(prefabParent.GetChild(i));
        }

        for (int i = 0; i < heartDatas.Count; i++)
        {
            var temp = Instantiate(heartReceiveUIprefab,prefabParent);
            temp.Init(heartDatas[i]);
        }

        bool isAlert = heartDatas.Count > 0 ? true : false;
        alertMark.SetActive(isAlert);
    }

}
