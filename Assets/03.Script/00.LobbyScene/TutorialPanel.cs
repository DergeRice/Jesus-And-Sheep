using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : PanelBase
{    
    public int currentPage = 0;
    public Button nextButton;
    public List<GameObject> panels;

    private void Start()
    {
        nextButton.onClick.AddListener(()=>{GoNextPage();});
    }

    public void GoNextPage()
    {
        currentPage++;

        if(currentPage >= panels.Count)
        {
            root.SetActive(false);
            currentPage = -1;
            EnablePage(0);
            return;
        }

        EnablePage(currentPage);
    }

    public void EnablePage(int index)
    {
        DisableAllPages();
        panels[index].SetActive(true);
    }

    public void DisableAllPages()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }
    }
}
