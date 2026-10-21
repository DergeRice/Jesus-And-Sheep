# Firebase backend migration

Implementation is ready for configuration and staging deployment. Production traffic has NOT moved. `Assets/Resources/Network/firebase.json` stays disabled until actual Firebase configuration is supplied.

## Setup

1. Select a Firebase project and enable Authentication → Anonymous and Google.
2. Register the Unity Android package and iOS bundle ID from Player Settings. Add `google-services.json` and `GoogleService-Info.plist` to Assets. These are app configuration, NOT service-account credentials. Add Android debug/release/Play App Signing SHA-1 and SHA-256 fingerprints for Google login.
3. Enable Firestore in production mode, choose its location deliberately, and link Blaze billing for Functions. The server runs in `asia-northeast3` with zero minimum instances and a maximum of two instances. Free quotas are not a hard billing cap.
4. In `firebase/functions`, run `npm ci` with Node 22. In `firebase`, run `functions/node_modules/.bin/firebase login`, then `functions/node_modules/.bin/firebase deploy --project YOUR_PROJECT_ID --only firestore,functions`. Never include a service-account private key in Unity.
5. Set `apiBaseUrl` in `Assets/Resources/Network/firebase.json` to the deployed `gameApi` HTTPS URL (typically `https://asia-northeast3-YOUR_PROJECT_ID.cloudfunctions.net/gameApi`). Use the exact URL reported by Firebase. Set `googleWebClientId` to the Web OAuth client ID, not the Android OAuth client ID. For iOS set `googleIosClientId` and `googleIosReversedClientId` from the plist. Then set `enabled` to true.
6. Run EDM4U Android Resolver → Force Resolve and iOS CocoaPods resolution. Build and test both supported devices. Native Google sign-in cannot be tested through a desktop Editor account picker.
7. Verify guest restart, guest → Google linking, reinstall → Google restore, two-account separation, rankings, gifts, and offline saves on staging before releasing. Existing installed apps continue to use the old server. Switching to this backend requires a new app build.

## Implemented

- Firebase Auth SDK 13.17.0 manages the persisted guest/Google session and token refresh. Unity obtains an ID token in memory for authenticated HTTP calls; it does not write tokens to PlayerPrefs or save files.
- Every API verifies the Firebase ID token. Player identity comes from the token UID, never a supplied nickname.
- `players/{uid}` holds profile and best score. Atomic `nicknames/{normalized-hash}` ownership prevents concurrent duplicate registration. Nickname changes are intentionally disabled until their gold cost can be enforced server-side.
- Monthly leaderboards live at `leaderboards/YYYY-MM/entries/{uid}` (UTC month boundary). Only the month's best score is kept; profile all-time score remains. Queries return at most 100 rows.
- Gifts are scoped to the receiving UID. Server-side cooldown is six hours per sender/recipient. Claim transactions allow one success only. A server claim count and local restored claim marker reconcile a lost response without replaying an already-applied gift.
- Private snapshots back up current local settings, hearts, gameplay files and friend data. Saves are debounced and retried; login loads the cloud snapshot before enabling uploads. Restored account data replaces the local account cache, while a guest snapshot is backed up locally before switching.
- Guest-to-Google linking preserves UID. If Google already owns another Firebase account, the UI asks whether to load that account instead, with a local guest backup. It does NOT silently merge accounts or transfer currency.
- A Google login row is created in the settings panel when Firebase is enabled. Android uses Credential Manager; iOS uses GoogleSignIn. No email or Apple login is enabled.
- Firestore rules deny direct client reads/writes; the authenticated API controls access.

## Limits before production

This is not an EC2 database import. Existing nicknames and scores must be imported only after account ownership is established; a nickname alone cannot prove ownership. Production project IDs, config files, OAuth client IDs and signing fingerprints are still required.

The existing game maintains gold, purchased entitlements, regenerating hearts, and gameplay on-device. Private snapshots back those up, but they are **client-provided data**, not trusted payment evidence. Score submission verifies ownership, bounds and monotonic best scores; it does not verify that the level was honestly played. Receipt validation, rewarded-ad verification and replay/server-authoritative score validation are separate work. Do not describe this implementation as cheat-proof or a server-authoritative economy.

Cloud saves are best effort: suspension/force quit may interrupt an upload. A guest account alone cannot recover an unlinked anonymous identity after uninstall. Firebase SDK persistence and real Google OAuth still need device verification with the actual project. iOS builds with Google login should also be checked against the store's applicable login requirements before submission.

## Tests

```sh
cd firebase/functions
npm test
PATH="/path/to/JDK/bin:$PATH" node_modules/.bin/firebase emulators:exec \
  --config ../firebase.json --project demo-jesus-sheep --only auth,firestore \
  'node test/emulator.cjs'
```

The emulator uses synthetic Google tokens only in tests, never in production. It verifies UID preservation on linking, restore through Google login, refresh-token continuity, private save isolation, forged nicknames, duplicate claims, cooldowns and direct Firestore denial. `FirebaseBackendChecks.Run` in Unity checks the snapshot defaults and settings row; `ReleaseRegressionChecks.Run` runs the existing game regressions.

References:
- https://firebase.google.com/docs/unity/setup
- https://firebase.google.com/docs/auth/unity/manage-users
- https://firebase.google.com/docs/auth/unity/google-signin
- https://developer.android.com/identity/sign-in/credential-manager-siwg
- https://developers.google.com/identity/sign-in/ios/sign-in
