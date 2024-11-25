using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemPanel : MonoBehaviour
{
    public GameObject spotLight;
    public TMP_Text currentCoin;
    // Start is called before the first frame update
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        spotLight.SetActive(true);
        currentCoin.text =  NetworkManager.instance.GetCurGold().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        spotLight.SetActive(false);
    }
}
