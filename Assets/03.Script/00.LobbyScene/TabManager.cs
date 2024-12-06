using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public Sprite defaultImg, chosenImg;
    public List<GameObject> tabs = new List<GameObject>(); 
    private void Start()
    {
        Debug.Log("Ddas");
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[index].GetComponent<Button>().onClick.AddListener(()=>EnableTab(index));
        }   
        EnableTab(2);
    }

    public void EnableTab(int index)
    {
        Debug.Log("Dd");
        SetAllDisable();
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
