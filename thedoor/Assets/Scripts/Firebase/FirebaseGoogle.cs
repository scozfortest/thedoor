using Facebook.Unity;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using Scoz.Func;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TheDoor.Main {
    public partial class FirebaseManager {
        // Copy this value from the google-service.json file.
        // oauth_client with type == 3
#if Dev
        private const string WEB_CLIENT_ID = "759778984097-ifh063ko0urhsr244cjcgi4du589qna4.apps.googleusercontent.com";
#elif Test
        private const string WEB_CLIENT_ID = "547956738293-8sb69hl66k1ptt1qtd9sg7mh3d38jlrv.apps.googleusercontent.com";
#elif Release
        private const string WEB_CLIENT_ID = "727159051252-93rlc5it2uaetsltkav146q341dpvhud.apps.googleusercontent.com";
#endif
        public static void SignInWithGoogle(Action<bool> _cb) {
            if (GoogleSignIn.Configuration == null)
                GoogleSignIn.Configuration = new GoogleSignInConfiguration {
                    RequestIdToken = true,
                    WebClientId = WEB_CLIENT_ID
                };

            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

            TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
            signIn.ContinueWithOnMainThread(task => {
                try {
                    if (task.IsCanceled) {
                        Debug.Log("Google Auth Cancel");
                        signInCompleted.SetCanceled();
                        _cb?.Invoke(false);
                    } else if (task.IsFaulted) {
                        Debug.Log("Google Auth Faulted : " + task.Exception.ToString());
                        signInCompleted.SetException(task.Exception);
                        _cb?.Invoke(false);
                    } else {
                        Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

                        if (credential.IsValid()) {
                            Debug.Log("Get Valid credential");
                            SignInWithCredential(credential, _cb);
                        } else {
                            DebugLogger.LogError("Google credential is not valid");
                            _cb?.Invoke(false);
                        }
                    }
                } catch (Exception _e) {
                    DebugLogger.LogError("SignInWithGoogle Error:" + _e);
                }
            });
        }

        public static void LinkWithGoogle(Action<bool> _cb) {
            /*改不確認玩家是否有三方登入了，因為規劃上玩家要可以同時綁定多個不同三方平台
             if (!CheckAccountCanLink())
             {
                 _cb?.Invoke(false);
                 return;
             }
            */
            if (GoogleSignIn.Configuration == null)
                GoogleSignIn.Configuration = new GoogleSignInConfiguration {
                    RequestIdToken = true,
                    WebClientId = WEB_CLIENT_ID
                };

            Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

            signIn.ContinueWithOnMainThread(task => {
                Debug.Log(task.Status + task.IsFaulted.ToString());
                if (task.IsCanceled) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
                } else if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
                } else {
                    Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
                    LinkAccountWithCredential(credential, (bool _result) => {
                        if (!_result)
                            GoogleSignIn.DefaultInstance.SignOut();
                        _cb?.Invoke(_result);
                    });
                }
            });
        }

        /* YingYi: 這功能不應該用到，不用讓玩家解除 他解除了也不能用該第三方再創帳號  !Google可以再第三方再創帳號!
         */
        public static void UnlinkGoogle(Action<bool> _cb) {
            UnLinkAccountWithProvider("google.com", _cb);
        }
    }
}