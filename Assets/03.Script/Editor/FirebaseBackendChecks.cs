using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FirebaseBackendChecks
{
    public static void Run()
    {
        if (!Application.isBatchMode) throw new Exception("Batch only.");
        var defaults = FirebaseCloudSave.NewAccount();
        if (defaults.hearts != 5 || defaults.settings[PlayerPrefsData.gold.ToString()] != "1000")
            throw new Exception("New account defaults invalid.");
        var roundtrip = JsonConvert.DeserializeObject<FirebaseCloudSnapshot>(JsonConvert.SerializeObject(defaults));
        if (roundtrip.files.Count != 0 || roundtrip.appliedGiftCount != 0 || roundtrip.isShot)
            throw new Exception("New account inherited previous account data.");
        if (FirebaseSession.Enabled) throw new Exception("Unconfigured Firebase backend should remain disabled.");
        EditorSceneManager.OpenScene("Assets/01.Scenes/01.LobbySceneSheep.unity");
        var settings = UnityEngine.Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
        FirebaseSession.Config.enabled = true;
        try
        {
            GoogleLoginButton.AddToSettings(settings);
            settings.rectTransform.anchoredPosition = Vector2.zero;
            if (settings.contentRoot.GetComponentInChildren<GoogleLoginButton>(true) == null)
                throw new Exception("Google login row not created.");
        }
        finally { FirebaseSession.Config.enabled = false; }
        var capture = typeof(ReleaseRegressionChecks).GetMethod("CaptureScene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        capture.Invoke(null, new object[] { "firebase-google-settings", 1280, new System.Collections.Generic.List<string>() });
        Debug.Log("Firebase Unity checks passed: defaults, snapshot roundtrip, disabled deployment gate, Google settings row.");
    }
}
