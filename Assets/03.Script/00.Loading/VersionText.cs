using UnityEngine;
using TMPro;

public class VersionText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<TMP_Text>().text = $"V {Application.version}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
