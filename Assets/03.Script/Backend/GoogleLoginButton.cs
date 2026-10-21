using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoogleLoginButton : MonoBehaviour
{
    public Button button;
    public TMP_Text label;
    private bool busy;
    private bool confirmRestore;
    private float timeoutAt;
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SheepGoogleSignIn(string clientId, string serverClientId, string callbackObject);
#endif
    private void Awake() { button = GetComponent<Button>(); if (button != null) button.onClick.AddListener(SignIn); }
    private void Update()
    {
        if (busy && Time.realtimeSinceStartup > timeoutAt) OnGoogleError("Google sign-in timed out.");
    }
    public void SignIn()
    {
        if (busy) return;
        if (!FirebaseSession.Enabled || string.IsNullOrEmpty(FirebaseSession.Config.googleWebClientId))
        {
            GameManager.instance.ToastText("Google login is not configured yet.");
            return;
        }
        busy = true;
        button.interactable = false;
        timeoutAt = Time.realtimeSinceStartup + 120f;
#if UNITY_ANDROID && !UNITY_EDITOR
        byte[] random = new byte[32];
        using (var generator = RandomNumberGenerator.Create()) generator.GetBytes(random);
        using (var bridge = new AndroidJavaClass("com.baragames.sheep.SheepGoogleSignIn"))
            bridge.CallStatic("signIn", FirebaseSession.Config.googleWebClientId, Convert.ToBase64String(random), gameObject.name);
#elif UNITY_IOS && !UNITY_EDITOR
        SheepGoogleSignIn(FirebaseSession.Config.googleIosClientId, FirebaseSession.Config.googleWebClientId, gameObject.name);
#else
        OnGoogleError("Google sign-in is available in the Android/iOS build.");
#endif
    }
    public void OnGoogleToken(string token) { StartCoroutine(CompleteLogin(token)); }
    private IEnumerator CompleteLogin(string token)
    {
        yield return NetworkManager.instance.FlushCloudSave();
        if (NetworkManager.instance.CloudSavePending && !string.IsNullOrEmpty(NetworkManager.instance.ownData.nickname))
        {
            OnGoogleError("Please reconnect and save your guest progress before linking Google.");
            yield break;
        }
        NetworkManager.instance.onlineMode = false;
        var login = FirebaseSession.SignInOrLinkGoogleAsync(token);
        token = null;
        yield return new WaitUntil(() => login.IsCompleted);
        busy = false;
        button.interactable = true;
        if (login.IsFaulted || login.IsCanceled)
        {
            // Linking an already owned Google account does not silently discard guest progress.
            NetworkManager.instance.OnlineTest();
            GameManager.instance.ToastText("Google login failed. Your guest progress is still saved.");
            yield break;
        }
        if (!login.Result)
        {
            confirmRestore = true;
            yield break;
        }
        NetworkManager.instance.ReloadFirebaseAccount();
        if (label != null) label.text = "Google Connected";
    }
    private void OnGUI()
    {
        if (!confirmRestore) return;
        float width = Mathf.Min(Screen.width - 40, 640);
        GUILayout.BeginArea(new Rect((Screen.width - width) / 2, Screen.height / 2 - 100, width, 240), GUI.skin.box);
        GUILayout.Label("This Google account already has saved progress. Load it? Your guest progress will be backed up on this device.");
        if (GUILayout.Button("Load Google Progress", GUILayout.Height(60)))
        {
            confirmRestore = false;
            StartCoroutine(RestoreAccount());
        }
        if (GUILayout.Button("Keep Guest Progress", GUILayout.Height(60)))
        {
            confirmRestore = false;
            FirebaseSession.CancelGoogleRestore();
            NetworkManager.instance.OnlineTest();
        }
        GUILayout.EndArea();
    }
    private IEnumerator RestoreAccount()
    {
        busy = true;
        button.interactable = false;
        timeoutAt = Time.realtimeSinceStartup + 120f;
        var task = FirebaseSession.RestoreGoogleAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        busy = false;
        button.interactable = true;
        if (task.IsFaulted || task.IsCanceled) { OnGoogleError("Could not restore Google progress."); yield break; }
        NetworkManager.instance.ReloadFirebaseAccount();
        if (label != null) label.text = "Google Connected";
    }

    public void OnGoogleError(string message)
    {
        busy = false;
        button.interactable = true;
        GameManager.instance.ToastText(message);
    }

    public static void AddToSettings(SettingPanel settings)
    {
        if (!FirebaseSession.Enabled) return;
        if (settings.contentRoot == null && settings.rectTransform != null)
            settings.contentRoot = settings.rectTransform.Find("Content");
        if (settings.contentRoot == null && settings.settingButtons.Count > 0)
            settings.contentRoot = settings.settingButtons[0].transform.parent;
        if (settings.contentRoot == null) return;
        // Reuse the settings row style while fitting the extra row inside the existing panel.
        Button template = settings.settingButtons.Count > 0 ? settings.settingButtons[0].GetComponent<Button>() : null;
        if (template == null) return;
        var row = Instantiate(template.gameObject, settings.contentRoot);
        row.name = "FirebaseGoogleLogin";
        foreach (var component in row.GetComponents<SettingButton>())
        {
            if (Application.isPlaying) Destroy(component);
            else DestroyImmediate(component);
        }
        foreach (Transform child in row.transform)
            if (child.GetComponent<TMP_Text>() == null) child.gameObject.SetActive(false);
        var login = row.AddComponent<GoogleLoginButton>();
        // AddComponent invokes Awake, so assign button before use by resolving it there.
        login.button = row.GetComponent<Button>();
        login.button.onClick = new Button.ButtonClickedEvent();
        login.button.onClick.AddListener(login.SignIn);
        login.label = row.GetComponentInChildren<TMP_Text>(true);
        if (login.label != null)
        {
            login.label.text = "Sign in with Google";
            login.label.alignment = TextAlignmentOptions.Center;
            login.label.enableAutoSizing = true;
            login.label.fontSizeMax = 48;
            login.label.fontSizeMin = 24;
            var textRect = login.label.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16, 4);
            textRect.offsetMax = new Vector2(-16, -4);
            if (login.label.GetComponent<NonTranslatableText>() == null) login.label.gameObject.AddComponent<NonTranslatableText>();
        }
        row.SetActive(true);
        var grid = settings.contentRoot.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = new Vector2(grid.cellSize.x, grid.cellSize.y * 6f / 7f);
            grid.spacing = new Vector2(grid.spacing.x, grid.spacing.y * 6f / 7f);
        }
        var layout = settings.contentRoot.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing *= 6f / 7f;
            foreach (var rect in settings.contentRoot.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.parent != settings.contentRoot) continue;
                float height = rect.rect.height * 6f / 7f;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                var element = rect.GetComponent<LayoutElement>();
                if (element == null) element = rect.gameObject.AddComponent<LayoutElement>();
                element.preferredHeight = height;
                element.minHeight = height;
            }
        }
        if (layout == null && grid == null)
        {
            var rows = settings.contentRoot.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < rows.Length; i++)
            {
                var rect = rows[i].GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 390 - i * 125);
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Min(rect.sizeDelta.y, 105));
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(settings.contentRoot.GetComponent<RectTransform>());
    }
}
