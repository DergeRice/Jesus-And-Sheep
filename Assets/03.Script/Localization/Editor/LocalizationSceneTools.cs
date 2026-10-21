#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LocalizationSceneTools
{
    [MenuItem("Tools/Localization/Attach LocalizedText In Open Scenes")]
    public static void AttachLocalizedTextInOpenScenes()
    {
        int addedCount = 0;
        int disabledUnityLocalizationCount = 0;

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.isLoaded)
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                TMP_Text[] tmpTexts = roots[i].GetComponentsInChildren<TMP_Text>(true);
                addedCount += AttachBindings(tmpTexts, ref disabledUnityLocalizationCount);

                Text[] legacyTexts = roots[i].GetComponentsInChildren<Text>(true);
                addedCount += AttachBindings(legacyTexts, ref disabledUnityLocalizationCount);
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"LocalizedText added: {addedCount}, Unity LocalizeStringEvent disabled: {disabledUnityLocalizationCount}");
    }

    private static int AttachBindings<T>(T[] texts, ref int disabledUnityLocalizationCount) where T : Component
    {
        int addedCount = 0;

        for (int i = 0; i < texts.Length; i++)
        {
            T textComponent = texts[i];
            if (textComponent == null)
            {
                continue;
            }

            if (textComponent.GetComponent<NonTranslatableText>() != null)
            {
                continue;
            }

            string currentText = textComponent is TMP_Text tmp ? tmp.text : ((Text)(object)textComponent).text;
            if (string.IsNullOrWhiteSpace(currentText))
            {
                continue;
            }

            if (textComponent.GetComponent<LocalizedText>() == null)
            {
                LocalizedText localizedText = Undo.AddComponent<LocalizedText>(textComponent.gameObject);
                localizedText.CaptureCurrentTextAsSource();
                EditorUtility.SetDirty(localizedText);
                addedCount++;
            }

            LocalizeStringEvent unityLocalizedEvent = textComponent.GetComponent<LocalizeStringEvent>();
            if (unityLocalizedEvent != null && unityLocalizedEvent.enabled)
            {
                Undo.RecordObject(unityLocalizedEvent, "Disable LocalizeStringEvent");
                unityLocalizedEvent.enabled = false;
                disabledUnityLocalizationCount++;
            }
        }

        return addedCount;
    }
}
#endif
