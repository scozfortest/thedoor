using Facebook.Unity;
using Firebase.Auth;
using Scoz.Func;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TheDoor.Main {
    public partial class FirebaseManager {
        public static void InitFacebook() {
            if (FB.IsInitialized) {
                FB.ActivateApp();
                FB.Mobile.SetAdvertiserTrackingEnabled(true);
            } else {
                //Handle FB.Init
                FB.Init(() => {
                    var eventSystems = GameObject.FindObjectsOfType<EventSystem>();
                    foreach (var eventSystem in eventSystems) {
                        SceneManager.MoveGameObjectToScene(eventSystem.gameObject, SceneManager.GetActiveScene());
                    }
                    FB.ActivateApp();
                    FB.Mobile.SetAdvertiserTrackingEnabled(true);
                });
            }
        }

        public static void SendFBLogEvent(string _name, float _value, Dictionary<string, object> _data) {
            Debug.Log("送FB事件:" + _name);
            FB.LogAppEvent(_name, _value, _data);
        }

        private static List<string> FB_PERMISSIONS = new List<string> {
                    "public_profile",
                    //"email"
                };
        public static void SignInWithFacebook(Action<bool> _cb) {

            if (FB.IsLoggedIn)
                FB.LogOut();
            FB.LogInWithReadPermissions(permissions: FB_PERMISSIONS, callback: (ILoginResult result) => {
                if (result == null) {
                    WriteLog.LogError("LogInWithReadPermissions result is null");
                    PopupUI.ShowClickCancel("LogInWithReadPermissions result is null", null);
                    _cb?.Invoke(false);
                    return;
                }
                // Some platforms return the empty string instead of null.
                if (!string.IsNullOrEmpty(result.Error)) {
                    PopupUI.ShowClickCancel(result.Error, null);
                    WriteLog.LogError(result.Error);
                    _cb?.Invoke(false);
                } else if (result.Cancelled) {
                    PopupUI.ShowClickCancel("Login cancel", null);
                    WriteLog.LogError(result.Cancelled);
                    _cb?.Invoke(false);
                } else if (!string.IsNullOrEmpty(result.RawResult)) {
                    WriteLog.Log("Start Get FB Credential: " + result.RawResult);
                    Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(result.AccessToken.TokenString);
                    if (credential.IsValid())
                        SignInWithCredential(credential, _cb);
                    else {
                        WriteLog.LogError("Facebook credential is not valid");
                        _cb?.Invoke(false);
                    }
                } else {
                    _cb?.Invoke(false);
                }
            });
        }

        public static void LinkWithFacebook(Action<bool> _cb) {
            /*改不確認玩家是否有三方登入了，因為規劃上玩家要可以同時綁定多個不同三方平台
             if (!CheckAccountCanLink())
             {
                 _cb?.Invoke(false);
                 return;
             }
            */
            if (FB.IsLoggedIn)
                FB.LogOut();
            FB.LogInWithReadPermissions(permissions: FB_PERMISSIONS, callback: (ILoginResult result) => {
                if (result == null) {
                    _cb?.Invoke(false);
                    return;
                }
                // Some platforms return the empty string instead of null.
                if (!string.IsNullOrEmpty(result.Error)) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    WriteLog.LogError(result.Error);
                    _cb?.Invoke(false);
                } else if (result.Cancelled) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
                } else if (!string.IsNullOrEmpty(result.RawResult)) {
                    Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(result.AccessToken.TokenString);
                    LinkAccountWithCredential(credential, (bool _result) => {
                        if (!_result)
                            FB.LogOut();
                        _cb?.Invoke(_result);
                    });
                } else {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
                }
            });
        }

        /* YingYi: 這功能不應該用到，不用讓玩家解除 他解除了也不能用該第三方再創帳號  !Google可以再第三方再創帳號!
         */
        public static void UnlinkFacebook(Action<bool> _cb) {
            UnLinkAccountWithProvider("facebook.com", _cb);
        }

        private static void CheckFacebookStatus(Action<bool> _cb) {
            FB.Mobile.RefreshCurrentAccessToken((IAccessTokenRefreshResult result) => {
                if (result != null && string.IsNullOrEmpty(result.Error) && FB.IsLoggedIn) {
                    AccessToken accessToken = result.AccessToken;
                    _cb?.Invoke(true);
                    return;
                }
                //重新登入FB
                SignInWithFacebook(_cb);
            });
        }
    }
}