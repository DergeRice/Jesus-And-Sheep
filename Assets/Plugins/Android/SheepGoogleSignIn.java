package com.baragames.sheep;

import android.app.Activity;
import androidx.annotation.Keep;
import android.os.CancellationSignal;
import androidx.credentials.CredentialManager;
import androidx.credentials.CredentialManagerCallback;
import androidx.credentials.GetCredentialRequest;
import androidx.credentials.GetCredentialResponse;
import androidx.credentials.exceptions.GetCredentialException;
import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption;
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;
import com.unity3d.player.UnityPlayer;

@Keep
public final class SheepGoogleSignIn {
    public static void signIn(String serverClientId, String nonce, String callbackObject) {
        Activity activity = UnityPlayer.currentActivity;
        activity.runOnUiThread(() -> {
            try {
                GetSignInWithGoogleOption option = new GetSignInWithGoogleOption.Builder(serverClientId).setNonce(nonce).build();
                GetCredentialRequest request = new GetCredentialRequest.Builder().addCredentialOption(option).build();
                CredentialManager.create(activity).getCredentialAsync(activity, request, new CancellationSignal(),
                    command -> activity.runOnUiThread(command), new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>() {
                        @Override public void onResult(GetCredentialResponse result) {
                            try {
                                GoogleIdTokenCredential credential = GoogleIdTokenCredential.createFrom(result.getCredential().getData());
                                UnityPlayer.UnitySendMessage(callbackObject, "OnGoogleToken", credential.getIdToken());
                            } catch (Exception error) {
                                UnityPlayer.UnitySendMessage(callbackObject, "OnGoogleError", "Google credential could not be read.");
                            }
                        }
                        @Override public void onError(GetCredentialException error) {
                            UnityPlayer.UnitySendMessage(callbackObject, "OnGoogleError", "Google sign-in was cancelled or unavailable.");
                        }
                    });
            } catch (Exception error) {
                UnityPlayer.UnitySendMessage(callbackObject, "OnGoogleError", "Google sign-in is unavailable on this device.");
            }
        });
    }
}
