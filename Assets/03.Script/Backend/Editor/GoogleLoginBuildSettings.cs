#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class GoogleLoginBuildSettings
{
    [PostProcessBuild(100)]
    public static void Configure(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS || !FirebaseSession.Enabled) return;
        var config = FirebaseSession.Config;
        if (string.IsNullOrEmpty(config.googleIosClientId) || string.IsNullOrEmpty(config.googleIosReversedClientId))
            throw new System.Exception("Configure Google iOS OAuth client and reversed client IDs before building.");
        string path = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(path);
        plist.root.SetString("GIDClientID", config.googleIosClientId);
        plist.root.SetString("GIDServerClientID", config.googleWebClientId);
        var schemes = plist.root.values.ContainsKey("CFBundleURLTypes")
            ? plist.root["CFBundleURLTypes"].AsArray() : plist.root.CreateArray("CFBundleURLTypes");
        schemes.AddDict().CreateArray("CFBundleURLSchemes").AddString(config.googleIosReversedClientId);
        File.WriteAllText(path, plist.WriteToString());
    }
}
#endif
