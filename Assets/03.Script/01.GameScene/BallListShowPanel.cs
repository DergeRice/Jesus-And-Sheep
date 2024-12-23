using System.Collections.Generic;
using UnityEngine;

public class BallListShowPanel : MonoBehaviour
{
    public GraceUI graceUIPrefab;

    public Transform prefabRoot;

    public List<string> ballExplainations;
    public List<Sprite> ballSprites;

    public void MakeUiFromDatas(List<BallType> ballTypes)
    {   
        RemoveAllUi();

        for (int i = 0; i < ballTypes.Count; i++)
        {
            if(ballTypes[i]!=BallType.Common)
            {
                var temp =  Instantiate(graceUIPrefab,prefabRoot);
                temp.Init(ballSprites[(int)ballTypes[i]],ballExplainations[(int)ballTypes[i]],i+1);
            }
        }
    }

    public void RemoveAllUi()
    {
        for (int i = 0; i < prefabRoot.childCount; i++)
        {
            Destroy(prefabRoot.GetChild(i).gameObject);
        }
    }


}
