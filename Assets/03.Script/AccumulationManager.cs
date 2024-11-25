using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccumulationData
{
    public int charIndex;
    public int mergeCount;
}
public class AccumulationManager : MonoBehaviour
{
    public List<AccumulationData> accumulationDatas = new List<AccumulationData>();

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
