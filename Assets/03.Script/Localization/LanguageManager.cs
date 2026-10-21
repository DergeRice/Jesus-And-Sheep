using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    private const string SourceLanguageCode = "ko";
    private const string LocalizationResourcePath = "Localization/localization";

    private static LanguageManager instance;

    private readonly HashSet<LocalizedText> bindings = new HashSet<LocalizedText>();
    private readonly Dictionary<string, LocalizedEntry> localizedEntries = new Dictionary<string, LocalizedEntry>();
    private readonly Dictionary<string, LocalizedEntry> localizedEntriesLoose = new Dictionary<string, LocalizedEntry>();
    private readonly Dictionary<string, string> reverseLocalizedEntries = new Dictionary<string, string>();
    private readonly Dictionary<string, string> reverseLocalizedEntriesLoose = new Dictionary<string, string>();
    private readonly List<LanguageOption> supportedLanguages = new List<LanguageOption>
    {
        new LanguageOption("ko", "한국어"),
        new LanguageOption("en", "English"),
    };

    public static LanguageManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LanguageManager");
                instance = go.AddComponent<LanguageManager>();
                DontDestroyOnLoad(go);
            }

            return instance;
        }
    }

    public static bool HasInstance => instance != null;

    public event Action<string> LanguageChanged;

    public string CurrentLanguageCode { get; private set; } = SourceLanguageCode;

    public IReadOnlyList<LanguageOption> SupportedLanguages => supportedLanguages;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        CurrentLanguageCode = GetSavedLanguageCode();
        LoadLocalizationEntries();
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        ApplyUnityLocale(CurrentLanguageCode);
        PrepareSceneLocalization();
        RefreshAllTexts();
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    public void ToggleLanguage()
    {
        SetLanguage(CurrentLanguageCode == "ko" ? "en" : "ko");
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            languageCode = SourceLanguageCode;
        }

        if (languageCode != "ko" && languageCode != "en")
        {
            languageCode = SourceLanguageCode;
        }

        CurrentLanguageCode = languageCode;
        PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.languageCode, languageCode);
        ApplyUnityLocale(languageCode);

        PrepareSceneLocalization();
        LanguageChanged?.Invoke(CurrentLanguageCode);
        RefreshAllTexts();
    }

    public string GetCurrentLanguageLabel()
    {
        LanguageOption option = supportedLanguages.Find(x => x.Code == CurrentLanguageCode);
        return option == null ? CurrentLanguageCode : option.DisplayName;
    }

    public string Localize(string sourceText)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return sourceText;
        }

        string normalizedKey = NormalizeLookupKey(sourceText);
        if (reverseLocalizedEntries.TryGetValue(normalizedKey, out string reversedSource))
        {
            sourceText = reversedSource;
        }
        else
        {
            string looseKey = NormalizeLookupKeyLoose(sourceText);
            if (reverseLocalizedEntriesLoose.TryGetValue(looseKey, out string reversedLooseSource))
            {
                sourceText = reversedLooseSource;
            }
        }

        if (CurrentLanguageCode == SourceLanguageCode)
        {
            return sourceText;
        }

        if (!localizedEntries.TryGetValue(NormalizeLookupKey(sourceText), out LocalizedEntry entry))
        {
            localizedEntriesLoose.TryGetValue(NormalizeLookupKeyLoose(sourceText), out entry);
            if (entry == null)
            {
                return sourceText;
            }
        }

        return string.IsNullOrWhiteSpace(entry.en) ? sourceText : entry.en;
    }

    public void Register(LocalizedText localizedText)
    {
        if (localizedText == null)
        {
            return;
        }

        bindings.Add(localizedText);
    }

    public void Unregister(LocalizedText localizedText)
    {
        if (localizedText == null)
        {
            return;
        }

        bindings.Remove(localizedText);
    }

    public void RefreshAllTexts()
    {
        bindings.RemoveWhere(binding => binding == null);

        foreach (LocalizedText binding in bindings)
        {
            if (binding.isActiveAndEnabled) binding.ApplyLocalizedValue();
        }
    }

    private string GetSavedLanguageCode()
    {
        string savedCode = PlayerPrefsManager.Instance.GetSetting(PlayerPrefsData.languageCode);
        if (savedCode == "en")
        {
            return "en";
        }

        return SourceLanguageCode;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        ApplyUnityLocale(CurrentLanguageCode);
        PrepareSceneLocalization();
        RefreshAllTexts();
    }

    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        SyncLanguageFromUnityLocale();
        PlayerPrefsManager.Instance.SetSetting(PlayerPrefsData.languageCode, CurrentLanguageCode);
        PrepareSceneLocalization();
        LanguageChanged?.Invoke(CurrentLanguageCode);
        RefreshAllTexts();
    }

    private void PrepareSceneLocalization()
    {
        DisableUnityLocalizationComponents();
        BootstrapLocalizedTexts(Resources.FindObjectsOfTypeAll<TMP_Text>());
        BootstrapLocalizedTexts(Resources.FindObjectsOfTypeAll<Text>());
    }

    private void DisableUnityLocalizationComponents()
    {
        LocalizeStringEvent[] components = Resources.FindObjectsOfTypeAll<LocalizeStringEvent>();
        foreach (LocalizeStringEvent component in components)
        {
            if (component == null || !component.gameObject.scene.IsValid())
            {
                continue;
            }

            component.enabled = false;
        }
    }

    private void BootstrapLocalizedTexts<T>(T[] texts) where T : Component
    {
        foreach (T textComponent in texts)
        {
            if (textComponent == null || !textComponent.gameObject.scene.IsValid())
            {
                continue;
            }

            // Never translate player input (the placeholder can still be translated).
            TMP_InputField tmpInput = textComponent.GetComponentInParent<TMP_InputField>();
            InputField input = textComponent.GetComponentInParent<InputField>();
            if ((tmpInput != null && tmpInput.textComponent == textComponent) ||
                (input != null && input.textComponent == textComponent) ||
                textComponent.GetComponent<NonTranslatableText>() != null)
            {
                continue;
            }

            LocalizedText localizedText = textComponent.GetComponent<LocalizedText>();
            if (localizedText == null)
            {
                localizedText = textComponent.gameObject.AddComponent<LocalizedText>();
            }

            localizedText.InitializeIfNeeded();
            if (localizedText.isActiveAndEnabled) Register(localizedText);
        }
    }

    private void LoadLocalizationEntries()
    {
        localizedEntries.Clear();
        localizedEntriesLoose.Clear();
        reverseLocalizedEntries.Clear();
        reverseLocalizedEntriesLoose.Clear();

        TextAsset json = Resources.Load<TextAsset>(LocalizationResourcePath);
        if (json == null || string.IsNullOrWhiteSpace(json.text))
        {
            Debug.LogWarning("Localization database not found. Falling back to Korean source strings.");
            return;
        }

        LocalizationCollection collection = JsonUtility.FromJson<LocalizationCollection>(json.text);
        if (collection == null || collection.entries == null)
        {
            Debug.LogWarning("Localization database is empty or malformed.");
            return;
        }

        for (int i = 0; i < collection.entries.Count; i++)
        {
            LocalizedEntry entry = collection.entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.source))
            {
                continue;
            }

            string sourceKey = NormalizeLookupKey(entry.source);
            localizedEntries[sourceKey] = entry;
            localizedEntriesLoose[NormalizeLookupKeyLoose(entry.source)] = entry;

            if (!string.IsNullOrWhiteSpace(entry.en))
            {
                reverseLocalizedEntries[NormalizeLookupKey(entry.en)] = entry.source;
                reverseLocalizedEntriesLoose[NormalizeLookupKeyLoose(entry.en)] = entry.source;
            }
        }
    }

    public string NormalizeLookupKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    private string NormalizeLookupKeyLoose(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Treat whitespace/newline variations as equivalent for lookup fallback.
        value = value.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] parts = value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts).Trim();
    }

    private void SyncLanguageFromUnityLocale()
    {
        string localeCode = LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : string.Empty;

        if (localeCode == "en")
        {
            CurrentLanguageCode = "en";
            return;
        }

        CurrentLanguageCode = "ko";
    }

    private void ApplyUnityLocale(string languageCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
        if (locale != null && LocalizationSettings.SelectedLocale != locale)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }

    [Serializable]
    public class LanguageOption
    {
        public string Code;
        public string DisplayName;

        public LanguageOption(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }
    }

    [Serializable]
    private class LocalizationCollection
    {
        public List<LocalizedEntry> entries;
    }

    [Serializable]
    private class LocalizedEntry
    {
        public string source;
        public string en;
        public string ja;
    }
}
