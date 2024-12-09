using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isLastGameRemain;

    public bool isContinueMode = false;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        isLastGameRemain = PlayerPrefsManager.Instance.GetBoolSetting(PlayerPrefsData.hasLastGame);
    }
    
}
