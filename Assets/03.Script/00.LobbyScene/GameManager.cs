using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isLastGameRemain;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        isLastGameRemain = PlayerPrefsManager.Instance.GetBoolSetting(PlayerPrefsData.hasLastGame);
        if(isLastGameRemain == true)
        {
            FindFirstObjectByType<LobbySceneUIManager>().countinueButton.interactable = false;
        }
    }
    
}
