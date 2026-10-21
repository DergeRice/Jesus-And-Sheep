#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameEventLocalizationTools
{
    [MenuItem("Tools/Localization/Fill EventDefinition English From JSON")]
    public static void FillEventDefinitionEnglishFromJson()
    {
        TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Localization/localization.json");
        if (json == null)
        {
            Debug.LogError("Missing Assets/Resources/Localization/localization.json");
            return;
        }

        LocalizationCollection collection = JsonUtility.FromJson<LocalizationCollection>(json.text);
        if (collection == null || collection.entries == null)
        {
            Debug.LogError("Localization json is empty or malformed.");
            return;
        }

        Dictionary<string, string> englishBySource = new Dictionary<string, string>();
        for (int i = 0; i < collection.entries.Count; i++)
        {
            LocalizationEntry entry = collection.entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.source))
            {
                continue;
            }

            englishBySource[entry.source] = entry.en;
        }

        string[] guids = AssetDatabase.FindAssets("t:GameEventDefinition");
        int updated = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameEventDefinition definition = AssetDatabase.LoadAssetAtPath<GameEventDefinition>(path);
            if (definition == null)
            {
                continue;
            }

            bool dirty = false;

            if (!string.IsNullOrWhiteSpace(definition.eventName) &&
                englishBySource.TryGetValue(definition.eventName, out string eventNameEn) &&
                definition.eventNameEn != eventNameEn)
            {
                definition.eventNameEn = eventNameEn;
                dirty = true;
            }

            if (!string.IsNullOrWhiteSpace(definition.explanation) &&
                englishBySource.TryGetValue(definition.explanation, out string explanationEn) &&
                definition.explanationEn != explanationEn)
            {
                definition.explanationEn = explanationEn;
                dirty = true;
            }

            if (!dirty)
            {
                continue;
            }

            EditorUtility.SetDirty(definition);
            updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Updated GameEventDefinition english fields: {updated}");
    }

    [System.Serializable]
    private class LocalizationCollection
    {
        public List<LocalizationEntry> entries;
    }

    [System.Serializable]
    private class LocalizationEntry
    {
        public string source;
        public string en;
    }
}
#endif
