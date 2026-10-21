#import <GoogleSignIn/GoogleSignIn.h>
#import "UnityAppController.h"
#import "AppDelegateListener.h"
#import "UnityInterface.h"

extern "C" void SheepGoogleSignIn(const char *clientId, const char *serverClientId, const char *callbackObject) {
    NSString *callback = [NSString stringWithUTF8String:callbackObject];
    NSString *client = [NSString stringWithUTF8String:clientId];
    NSString *server = [NSString stringWithUTF8String:serverClientId];
    dispatch_async(dispatch_get_main_queue(), ^{
        GIDSignIn.sharedInstance.configuration = [[GIDConfiguration alloc] initWithClientID:client serverClientID:server];
        [GIDSignIn.sharedInstance signInWithPresentingViewController:UnityGetGLViewController()
            completion:^(GIDSignInResult *result, NSError *error) {
                if (error || !result.user.idToken.tokenString) {
                    UnitySendMessage(callback.UTF8String, "OnGoogleError", "Google sign-in was cancelled or unavailable.");
                } else {
                    UnitySendMessage(callback.UTF8String, "OnGoogleToken", result.user.idToken.tokenString.UTF8String);
                }
            }];
    });
}

// Unity forwards URLs to subscribers; no replacement of the app delegate is needed.
@interface SheepGoogleURLHandler : NSObject
@end
@implementation SheepGoogleURLHandler
+ (void)load {
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(openURL:) name:kUnityOnOpenURL object:nil];
}
+ (void)openURL:(NSNotification *)notification {
    NSURL *url = notification.userInfo[@"url"];
    if (url) [GIDSignIn.sharedInstance handleURL:url];
}
@end
