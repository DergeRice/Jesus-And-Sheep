using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LocalizedText : MonoBehaviour
{
    [TextArea(1, 4)]
    [SerializeField] private string sourceKey;

    private TMP_Text tmpText;
    private Text legacyText;
    private bool initialized;
    private string lastAppliedText;
    private object[] formatArguments;

    public void SetSource(string value)
    {
        InitializeIfNeeded();
        sourceKey = value;
        formatArguments = null;
        lastAppliedText = GetCurrentText();
        ApplyLocalizedValue();
    }

    public void SetFormat(string key, params object[] arguments)
    {
        InitializeIfNeeded();
        sourceKey = key;
        formatArguments = arguments;
        lastAppliedText = GetCurrentText();
        ApplyLocalizedValue();
    }

    private void LateUpdate()
    {
        if (GetComponent<NonTranslatableText>() == null && GetCurrentText() != lastAppliedText)
            ApplyLocalizedValue();
    }

    public static LocalizedText Attach(Component textComponent)
    {
        if (textComponent == null)
        {
            return null;
        }

        LocalizedText localizedText = textComponent.GetComponent<LocalizedText>();
        if (localizedText == null)
        {
            localizedText = textComponent.gameObject.AddComponent<LocalizedText>();
            localizedText.CaptureCurrentTextAsSource();
        }

        localizedText.InitializeIfNeeded();
        LanguageManager.Instance.Register(localizedText);
        localizedText.ApplyLocalizedValue();

        return localizedText;
    }

    public void InitializeIfNeeded()
    {
        if (initialized)
        {
            return;
        }

        tmpText = GetComponent<TMP_Text>();
        legacyText = GetComponent<Text>();

        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            sourceKey = GetCurrentText();
        }

        sourceKey = LanguageManager.Instance.NormalizeLookupKey(sourceKey);

        LocalizeStringEvent unityLocalizedEvent = GetComponent<LocalizeStringEvent>();
        if (unityLocalizedEvent != null)
        {
            unityLocalizedEvent.enabled = false;
        }

        lastAppliedText = GetCurrentText();
        initialized = true;
    }

    public void CaptureCurrentTextAsSource()
    {
        tmpText = GetComponent<TMP_Text>();
        legacyText = GetComponent<Text>();
        sourceKey = GetCurrentText();

        sourceKey = LanguageManager.Instance.NormalizeLookupKey(sourceKey);
    }

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();

        LanguageManager.Instance.Register(this);
        ApplyLocalizedValue();
    }

    private void OnDisable()
    {
        if (LanguageManager.HasInstance) LanguageManager.Instance.Unregister(this);
    }

    public void ApplyLocalizedValue()
    {
        InitializeIfNeeded();

        if (!enabled || GetComponent<NonTranslatableText>() != null) return;
        // Gameplay code may have replaced a counter, description or pooled label.
        if (GetCurrentText() != lastAppliedText)
        {
            sourceKey = GetCurrentText();
            formatArguments = null;
        }
        string value = LanguageManager.Instance.Localize(sourceKey);
        if (formatArguments != null) value = string.Format(value, formatArguments);
        SetCurrentText(value);
        lastAppliedText = value;
        FitTranslatedText(value);
    }

    private void FitTranslatedText(string value)
    {
        if (LanguageManager.Instance.CurrentLanguageCode != "en") return;
        if (legacyText != null && !legacyText.resizeTextForBestFit)
        {
            legacyText.resizeTextMaxSize = legacyText.fontSize;
            legacyText.resizeTextMinSize = Mathf.Min(legacyText.fontSize, Mathf.Max(12, legacyText.fontSize / 2));
            legacyText.resizeTextForBestFit = true;
            legacyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }
        if (tmpText == null) return;
        // Preserve authored font limits when present; only shrink to fit, never enlarge.
        if (!tmpText.enableAutoSizing)
        {
            tmpText.fontSizeMax = tmpText.fontSize;
            tmpText.fontSizeMin = Mathf.Min(tmpText.fontSize, Mathf.Max(14f, tmpText.fontSize * 0.55f));
            tmpText.enableAutoSizing = true;
        }
        tmpText.textWrappingMode = TextWrappingModes.Normal;
    }

    private string GetCurrentText()
    {
        if (tmpText != null)
        {
            return tmpText.text;
        }

        return legacyText != null ? legacyText.text : string.Empty;
    }

    private void SetCurrentText(string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
        }
        else if (legacyText != null)
        {
            legacyText.text = value;
        }
    }
}
