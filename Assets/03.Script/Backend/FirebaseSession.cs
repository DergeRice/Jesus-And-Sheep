using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

// The native Firebase SDK persists the auth session and refreshes ID tokens.
// Never persist ID/refresh tokens in PlayerPrefs, save files or logs.
public static class FirebaseSession
{
    [Serializable]
    public class Configuration
    {
        public bool enabled;
        public string apiBaseUrl;
        public bool allowGuestSignIn = true;
        public string googleWebClientId;
        public string googleIosClientId;
        public string googleIosReversedClientId;
    }
    public static Configuration Config { get; private set; }
    private static Task initialization;
    public static bool Enabled
    {
        get
        {
            if (Config == null)
            {
                var asset = Resources.Load<TextAsset>("Network/firebase");
                Config = asset == null ? new Configuration() : JsonUtility.FromJson<Configuration>(asset.text);
            }
            return Config != null && Config.enabled;
        }
    }
    public static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
    public static Task InitializeAsync()
    {
        if (initialization == null || initialization.IsFaulted || initialization.IsCanceled)
            initialization = InitializeCoreAsync();
        return initialization;
    }
    private static async Task InitializeCoreAsync()
    {
        if (!Enabled) throw new InvalidOperationException("Firebase backend is not configured.");
        if (!Uri.TryCreate(Config.apiBaseUrl, UriKind.Absolute, out var url) || url.Scheme != "https")
            throw new InvalidOperationException("Configure the Firebase HTTPS API URL first.");
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available) throw new InvalidOperationException("Firebase dependencies unavailable: " + status);
        var auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null)
        {
            if (!Config.allowGuestSignIn) throw new InvalidOperationException("Sign in before accessing cloud data.");
            await auth.SignInAnonymouslyAsync();
        }
    }
    public static async Task<string> GetIdTokenAsync(bool forceRefresh = false)
    {
        await InitializeAsync();
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) throw new InvalidOperationException("Firebase session signed out.");
        return await user.TokenAsync(forceRefresh);
    }
    private static Credential pendingGoogleCredential;
    public static void CancelGoogleRestore() { pendingGoogleCredential = null; }
    public static async Task RestoreGoogleAccountAsync()
    {
        if (pendingGoogleCredential == null) throw new InvalidOperationException("Choose a Google account first.");
        var credential = pendingGoogleCredential;
        pendingGoogleCredential = null;
        FirebaseCloudSave.BackupGuest(UserId);
        await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
    }

    public static async Task<bool> SignInOrLinkGoogleAsync(string googleIdToken)
    {
        await InitializeAsync();
        var auth = FirebaseAuth.DefaultInstance;
        var credential = GoogleAuthProvider.GetCredential(googleIdToken, null);
        if (auth.CurrentUser != null && auth.CurrentUser.IsAnonymous)
        {
            try { await auth.CurrentUser.LinkWithCredentialAsync(credential); }
            catch (FirebaseAccountLinkException error) when (error.ErrorCode == (int)AuthError.CredentialAlreadyInUse)
            {
                pendingGoogleCredential = error.UserInfo?.UpdatedCredential ?? credential;
                return false;
            }
        }
        else await auth.SignInWithCredentialAsync(credential);
        return true;
    }
}
