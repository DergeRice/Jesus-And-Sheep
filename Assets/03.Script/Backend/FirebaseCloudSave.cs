using System;
using System.Collections.Generic;
using System.IO;
using GUPS.AntiCheat.Protected.Prefs;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class FirebaseCloudSnapshot
{
    public Dictionary<string, string> settings = new Dictionary<string, string>();
    public Dictionary<string, string> files = new Dictionary<string, string>();
    public int hearts;
    public bool isShot;
    public int appliedGiftCount;
    public string lastHeartChargeTime;
}

public static class FirebaseCloudSave
{
    // Legacy files are backed up privately. They are not trusted purchase/ranking proof.
    private static readonly string[] FileNames = { "SavedGameData.json", "ShootData.json", "FriendData.json" };
    public static bool Restoring { get; private set; }
    public static FirebaseCloudSnapshot NewAccount()
    {
        var snapshot = new FirebaseCloudSnapshot { hearts = 5, lastHeartChargeTime = DateTime.UtcNow.ToString("o") };
        foreach (PlayerPrefsData key in Enum.GetValues(typeof(PlayerPrefsData))) snapshot.settings[key.ToString()] = "0";
        foreach (var key in new[] { PlayerPrefsData.hasLastGame, PlayerPrefsData.isBgmOff, PlayerPrefsData.isSfxOff,
            PlayerPrefsData.isFirstTime, PlayerPrefsData.lastGameUnlockSpeedMode, PlayerPrefsData.firstItemPurchased })
            snapshot.settings[key.ToString()] = "False";
        snapshot.settings[PlayerPrefsData.nickname.ToString()] = "";
        snapshot.settings[PlayerPrefsData.languageCode.ToString()] = "ko";
        snapshot.settings[PlayerPrefsData.gold.ToString()] = "1000";
        return snapshot;
    }

    public static void BackupGuest(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return;
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "GuestBackup-" + uid + ".json"), JsonConvert.SerializeObject(Capture()));
    }

    public static FirebaseCloudSnapshot Capture()
    {
        var snapshot = new FirebaseCloudSnapshot();
        foreach (PlayerPrefsData key in Enum.GetValues(typeof(PlayerPrefsData)))
            snapshot.settings[key.ToString()] = PlayerPrefsManager.Instance.GetSetting(key) ?? "";
        foreach (string name in FileNames)
        {
            string path = Path.Combine(Application.persistentDataPath, name);
            if (File.Exists(path)) snapshot.files[name] = File.ReadAllText(path);
        }
        snapshot.appliedGiftCount = ProtectedPlayerPrefs.GetInt("FirebaseAppliedGiftCount", 0);
        snapshot.isShot = PlayerPrefs.GetInt("IsShot", 0) == 1;
        snapshot.hearts = ProtectedPlayerPrefs.GetInt("HeartCount", 5);
        snapshot.lastHeartChargeTime = ProtectedPlayerPrefs.GetString("LastHeartChargeTime", DateTime.UtcNow.ToString("o"));
        return snapshot;
    }
    public static void Restore(FirebaseCloudSnapshot snapshot)
    {
        if (snapshot == null) return;
        Restoring = true;
        try
        {
            // Validate the complete snapshot before touching existing local data.
            if (snapshot.hearts < 0 || snapshot.hearts > 100000 ||
                !DateTime.TryParse(snapshot.lastHeartChargeTime, out _)) throw new InvalidDataException("Invalid cloud heart data.");
            foreach (PlayerPrefsData key in Enum.GetValues(typeof(PlayerPrefsData)))
                if (snapshot.settings != null && snapshot.settings.TryGetValue(key.ToString(), out string value))
                    PlayerPrefsManager.Instance.SetSetting(key, value);
            foreach (string name in FileNames)
            {
                string path = Path.Combine(Application.persistentDataPath, name);
                if (snapshot.files != null && snapshot.files.TryGetValue(name, out string data)) File.WriteAllText(path, data);
                else if (File.Exists(path)) File.Delete(path);
            }
            ProtectedPlayerPrefs.SetInt("FirebaseAppliedGiftCount", snapshot.appliedGiftCount);
            PlayerPrefs.SetInt("IsShot", snapshot.isShot ? 1 : 0);
            PlayerPrefs.Save();
            ProtectedPlayerPrefs.SetInt("HeartCount", snapshot.hearts);
            ProtectedPlayerPrefs.SetString("LastHeartChargeTime", snapshot.lastHeartChargeTime);
            ProtectedPlayerPrefs.Save();
        }
        finally { Restoring = false; }
    }
}
