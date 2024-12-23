using UnityEngine;
using System.Collections.Generic;
using GUPS.AntiCheat.Protected.Prefs;

public class PlayerPrefsManager : MonoBehaviour
{

    // 싱글톤 인스턴스
    private static PlayerPrefsManager _instance;

    public static PlayerPrefsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlayerPrefsManager");
                _instance = go.AddComponent<PlayerPrefsManager>();
                DontDestroyOnLoad(go); // 씬이 바뀌어도 유지
            }
            return _instance;
        }
    }

    // 설정값을 저장하는 딕셔너리
    private Dictionary<PlayerPrefsData, string> settings = new Dictionary<PlayerPrefsData, string>();

    // 게임 시작 시 PlayerPrefs에서 값을 로드
    private void Awake()
    {
        LoadSettings();
    }

    // PlayerPrefs에서 모든 설정값들을 한 번에 로드
    private void LoadSettings()
    {
        // PlayerPrefsData enum을 순회하여 모든 값 로드
        foreach (PlayerPrefsData key in System.Enum.GetValues(typeof(PlayerPrefsData)))
        {
            string value = ProtectedPlayerPrefs.GetString(key.ToString(), string.Empty); // 값을 불러오고 기본값을 빈 문자열로 설정
            settings[key] = value; // 딕셔너리에 저장
        }
    }

    public bool HasSetting(PlayerPrefsData key)
    {
        return settings.ContainsKey(key);
    }
    // 설정값을 저장하는 메서드
    public void SetSetting(PlayerPrefsData key, object value)
    {
        string valueString = value.ToString();
        settings[key] = valueString;
        ProtectedPlayerPrefs.SetString(key.ToString(), valueString);
        ProtectedPlayerPrefs.Save();
    }

    // 설정값을 가져오는 메서드 (string으로 반환)
    public string GetSetting(PlayerPrefsData key)
    {
        if (settings.ContainsKey(key))
        {
            return settings[key];
        }
        return null; // 설정값이 없을 경우 null 반환
    }

    // 설정값을 bool로 반환
    public bool GetBoolSetting(PlayerPrefsData key)
    {
        bool value = false;

        if(GetSetting(key)!= string.Empty && GetSetting(key) == "True") value = true;
        return value;
    }

    // 설정값을 int로 반환
    public int GetIntSetting(PlayerPrefsData key)
    {
        int result = 0;
        int.TryParse(GetSetting(key), out result);
        return result;
    }

    // 설정값을 float로 반환
    public float GetFloatSetting(PlayerPrefsData key)
    {
        float result = 0f;
        float.TryParse(GetSetting(key), out result);
        return result;
    }

    [ContextMenu("DeleleAll")]
     public void DeleteAllSettings()
    {
        // 딕셔너리와 PlayerPrefs 모두 초기화
        foreach (var key in settings.Keys)
        {
            ProtectedPlayerPrefs.DeleteKey(key.ToString());
        }
        settings.Clear();
        ProtectedPlayerPrefs.Save();
        Debug.Log("All settings have been deleted.");
    }
}
