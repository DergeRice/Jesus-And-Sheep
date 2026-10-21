using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsLanguageButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>();
        }

        // Prevent LanguageManager's auto-binding from overriding this label.
        if (label != null)
        {
            if (label.GetComponent<NonTranslatableText>() == null)
            {
                label.gameObject.AddComponent<NonTranslatableText>();
            }

            LocalizedText localizedText = label.GetComponent<LocalizedText>();
            if (localizedText != null)
            {
                localizedText.enabled = false;
            }
        }
    }

    private void OnEnable()
    {
        _ = LanguageManager.Instance;
        button.onClick.AddListener(HandleClick);
        LanguageManager.Instance.LanguageChanged += UpdateLabel;
        UpdateLabel(LanguageManager.Instance.CurrentLanguageCode);
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }

        if (LanguageManager.HasInstance)
        {
            LanguageManager.Instance.LanguageChanged -= UpdateLabel;
        }
    }

    private void HandleClick()
    {
        LanguageManager.Instance.ToggleLanguage();
    }

    private void UpdateLabel(string languageCode)
    {
        bool isEnglish = languageCode == "en";

        if (label != null)
        {
            label.text = isEnglish ? "한글" : "Eng";
        }
    }
}
