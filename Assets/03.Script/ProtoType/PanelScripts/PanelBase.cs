using UnityEngine;
using UnityEngine.UI;

public class PanelBase : MonoBehaviour
{
    public GameObject root;
    public Button close;


    public void Awake()
    {
        if(close != null) close.onClick.AddListener(()=> { SetActive(false); });
    }

    public void SetActive(bool _active)
    {
        root.SetActive(_active);
    }
}
