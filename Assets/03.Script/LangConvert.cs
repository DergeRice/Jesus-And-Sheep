using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LangConvert : MonoBehaviour
{
    
    public TMP_Text text;
    public TMP_InputField textField;
    public string korText, engText;
    public bool haveToCheckDefault;

    public bool isText, isField;

    private void Awake()
    {
        // textField  = GetComponent<TMP_InputField>();
    }

    public void SetText(bool isEng)
    {
    //    bool isDefault = false;

        if (haveToCheckDefault == true && isText && text.text != korText)
            return;

        if (haveToCheckDefault == true && isField && textField.text != korText)
            return;

        
        if (isField)
        {
            textField.text = isEng ? engText : korText;
        }
        else if (isText)
        {
            text.text = isEng ? engText : korText;
        }
        
        
    }
}
