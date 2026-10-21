using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Run with Unity -batchmode -executeMethod ReleaseRegressionChecks.Run -quit.
// Opens scenes without saving them; run in a separate batch editor process.
public static class ReleaseRegressionChecks
{
    private static int assertions;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        assertions++;
    }

    [MenuItem("Tools/QA/Run Release Regression Checks (Batch Only)")]
    public static void Run()
    {
        if (!Application.isBatchMode) throw new Exception("Run in batch mode to protect unsaved scene edits.");
        var report = new List<string>();
        Check(NetworkManager.IsNicknameAvailable("Nickname is available."), "Live server availability rejected");
        Check(!NetworkManager.IsNicknameAvailable("Nickname is unavailable."), "Unavailable server message accepted");
        Check(!NetworkManager.IsNicknameAvailable("unavailable"), "Unavailable nickname accepted");
        Check(!NetworkManager.IsNicknameAvailable("{\"available\":false}"), "False nickname availability accepted");
        Check(NetworkManager.IsNicknameAvailable("{\"available\":true}"), "Available nickname rejected");
        Check(NetworkManager.IsNicknameAvailable("available"), "Legacy availability rejected");
        Check(!NetworkManager.TryReadCompatibility("<html>Error</html>", out _), "Malformed version accepted");
        Check(NetworkManager.TryReadCompatibility("{\"isCompatible\":false,\"other\":true}", out bool compatible) && !compatible,
            "Unrelated true bypassed required update");

        var logicObject = new GameObject("QA logic");
        logicObject.SetActive(false);
        var logic = logicObject.AddComponent<GameLogicManager>();
        logic.shootingBallDatas = Enumerable.Repeat(BallType.Common, 10).ToList();
        logic.ChangeBallCount(-3);
        Check(logic.GetBallCount == 7, "Three-heart cost did not subtract three");
        logic.ChangeBallCount(0);
        Check(logic.GetBallCount == 7, "Zero cost removed a heart");
        logic.ChangeBallCount(-100);
        Check(logic.GetBallCount == 1, "Cost removed the final playable heart");
        logic.shootingBallDatas.Add(BallType.Cross);
        logic.ChangeBallCount(-2);
        Check(logic.GetBallCount == 1 && logic.shootingBallDatas[0] == BallType.Cross, "Cost removed special heart");
        UnityEngine.Object.DestroyImmediate(logicObject);

        var blockObject = new GameObject("QA blocks");
        var blocks = blockObject.AddComponent<BlockManager>();
        var sheep = new GameObject("QA sheep").AddComponent<Block>();
        blocks.blockGrid[0, 1] = sheep;
        Check(blocks.GetRandomBlock(3).SequenceEqual(new[] { sheep }), "Single sheep selection failed");
        DG.Tweening.DOTween.Kill(sheep.transform);
        // The spawn rows are full, while top/bottom rows remain empty. Must return, not loop forever.
        for (int x = 0; x < blocks.blockGrid.GetLength(0); x++)
            for (int y = 1; y < 6; y++) blocks.blockGrid[x, y] = sheep;
        blocks.SpawnRandomBlock(false);
        Check(blocks.blockGrid[0, 0] == null, "Full spawn area altered an unrelated row");
        UnityEngine.Object.DestroyImmediate(sheep.gameObject);
        UnityEngine.Object.DestroyImmediate(blockObject);

        var manager = CreateTestLanguageManager();
        typeof(LanguageManager).GetMethod("LoadLocalizationEntries", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
        Action<string> language = code => typeof(LanguageManager).GetProperty("CurrentLanguageCode").SetValue(manager, code);
        language("en");
        var labelObject = new GameObject("QA text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var label = labelObject.GetComponent<TMP_Text>();
        label.text = "설정";
        var binding = LocalizedText.Attach(label);
        Check(label.text == "Settings", "English translation missing");
        label.text = "37";
        binding.ApplyLocalizedValue();
        language("ko");
        binding.ApplyLocalizedValue();
        Check(label.text == "37", "Language switch restored stale counter");
        binding.SetSource("하트가 부족해요!");
        language("en");
        binding.ApplyLocalizedValue();
        Check(label.text == "Not enough hearts!", "Pooled description was not updated");
        binding.SetFormat("남은 광고 횟수 :{0}/{1}", 2, 5);
        Check(label.text == "Ads left: 2/5", "Formatted translation failed");
        language("ko");
        binding.ApplyLocalizedValue();
        Check(label.text == "남은 광고 횟수 :2/5", "Formatted language switch failed");
        labelObject.AddComponent<NonTranslatableText>();
        label.text = "설정";
        language("en");
        binding.ApplyLocalizedValue();
        Check(label.text == "설정", "Nickname exclusion ignored");
        UnityEngine.Object.DestroyImmediate(labelObject);

        foreach (string guid in AssetDatabase.FindAssets("t:GameEventDefinition", new[] { "Assets/08.Data/GameEvents" }))
        {
            var definition = AssetDatabase.LoadAssetAtPath<GameEventDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            Check(definition != null, "Invalid event asset: " + guid);
            if (definition.eventName == "commonBall") continue; // Deliberate unused placeholder at special index 0.
            Check(definition.effects.Count > 0, "Empty event: " + guid);
            Check(!string.IsNullOrWhiteSpace(definition.explanationEn), "Missing English event: " + guid);
            if (!definition.isSpecialBallEvent && definition.name.StartsWith("02_"))
                Check(definition.effects[0].intValue == 15 && definition.effects[1].intValue == 6, "Heart/turn values swapped");
            if (definition.name.StartsWith("16_"))
                Check(definition.effects[1].effectType == GameEventEffectType.BallCountIncrease && definition.effects[1].intValue == -1, "Heart cost became healing");
            if (definition.name.StartsWith("17_"))
                Check(definition.effects[0].intValue == -5, "Incorrect five-heart cost");
        }

        // Each scene load is isolated. No serialized scene or prefab is saved.
        foreach (string path in new[] { "Assets/01.Scenes/00.LogoScene.unity", "Assets/01.Scenes/01.LoadingSecne.unity",
            "Assets/01.Scenes/01.LobbySceneSheep.unity", "Assets/01.Scenes/02.GameScene.unity" })
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            manager = CreateTestLanguageManager();
            typeof(LanguageManager).GetMethod("LoadLocalizationEntries", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
            typeof(LanguageManager).GetProperty("CurrentLanguageCode").SetValue(manager, "en");
            foreach (var events in UnityEngine.Object.FindObjectsByType<EventManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Check(events.eventDefinitions.All(e => e != null), "Missing normal event reference");
                Check(events.specialBallDefinitions.All(e => e != null), "Missing special event reference");
            }
            int checkedTexts = 0;
            foreach (TMP_Text text in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (!text.gameObject.scene.IsValid() || text.font == null || text.GetComponent<NonTranslatableText>() != null) continue;
                string original = text.text;
                string translated = manager.Localize(original);
                if (translated == original) continue;
                LocalizedText.Attach(text).SetSource(original);
                checkedTexts++;
                text.ForceMeshUpdate(true, true);
                var rect = text.rectTransform.rect;
                if (rect.width > 0 && rect.height > 0 && text.isTextOverflowing)
                    report.Add("OVERFLOW " + path + " / " + Hierarchy(text.transform) + " : " + translated.Replace('\n', ' '));
            }
            report.Add(path + ": translated TMP labels checked = " + checkedTexts);
            if (checkedTexts > 0)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                CaptureScene(name, 1280, report);
                CaptureScene(name, 1560, report);
                var settings = UnityEngine.Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
                if (settings != null)
                {
                    settings.rectTransform.anchoredPosition = Vector2.zero;
                    CaptureScene(name + "-settings", 1280, report);
                }
                var select = UnityEngine.Object.FindFirstObjectByType<SelectPanel>(FindObjectsInactive.Include);
                if (select != null)
                {
                    select.root.SetActive(true);
                    select.parent.alpha = 1;
                    var events = UnityEngine.Object.FindFirstObjectByType<EventManager>();
                    var longest = events.eventDefinitions.OrderByDescending(e => e.explanationEn.Length).Take(3).ToArray();
                    select.one.InitSetting(new GameEventSelectionOption(longest[0], null), select.GetRankSprite(longest[0]));
                    select.two.InitSetting(new GameEventSelectionOption(longest[1], null), select.GetRankSprite(longest[1]));
                    select.three.InitSetting(new GameEventSelectionOption(longest[2], null), select.GetRankSprite(longest[2]));
                    CaptureScene(name + "-choices", 1280, report);
                    CaptureScene(name + "-choices", 1560, report);
                }
            }
        }
        Directory.CreateDirectory("qa");
        report.Insert(0, "Regression assertions passed: " + assertions);
        File.WriteAllLines("qa/release-checks.txt", report);
        Debug.Log(string.Join("\n", report));
    }

    private static void CaptureScene(string name, int height, List<string> report)
    {
        Camera camera = Camera.main;
        if (camera == null) camera = new GameObject("QA camera").AddComponent<Camera>();
        float authoredSize = camera.orthographicSize;
        // Match ScreenSizeController.Awake; edit-mode previews do not execute Awake.
        if (UnityEngine.Object.FindFirstObjectByType<ScreenSizeController>() != null)
            camera.orthographicSize = authoredSize * (9f * height) / (16f * 720);
        foreach (Canvas canvas in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (!canvas.gameObject.scene.IsValid() || !canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10;
        }
        var target = new RenderTexture(720, height, 24);
        camera.targetTexture = target;
        Canvas.ForceUpdateCanvases();
        foreach (TMP_Text text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            text.ForceMeshUpdate();
            if (text.isTextOverflowing)
                report.Add("VISIBLE OVERFLOW " + name + " 720x" + height + " / " + Hierarchy(text.transform));
        }
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(720, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 720, height), 0, 0);
        image.Apply();
        Directory.CreateDirectory("qa");
        File.WriteAllBytes("qa/" + name + "-english-" + height + ".png", image.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        camera.orthographicSize = authoredSize;
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(image);
    }

    private static LanguageManager CreateTestLanguageManager()
    {
        var manager = new GameObject("QA language manager").AddComponent<LanguageManager>();
        typeof(LanguageManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
        return manager;
    }

    private static string Hierarchy(Transform t) => t.parent == null ? t.name : Hierarchy(t.parent) + "/" + t.name;
}
