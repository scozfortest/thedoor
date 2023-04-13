using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheDoor.Main {
    public partial class FirebaseManager {
        private static void SignInWithCredential(Credential credential, Action<bool> _cb, bool showError = true) {
            MyAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                try {
                    if (task == null) {
                        PopupUI.ShowClickCancel("SignInWithCredential task i s null", null);
                        WriteLog.LogError("SignInWithCredential task i s null");
                        _cb?.Invoke(false);
                        return;
                    }
                    if (task.IsCanceled) {
                        PopupUI.ShowClickCancel("SignInWithCredential Canceled", null);
                        Debug.Log("SignInWithCredential Canceled");

                        _cb?.Invoke(false);
                        return;
                    }
                    if (task.IsFaulted) {
                        try {
                            WriteLog.LogError("SignInWithCredentialAsync encountered a error " + task.Exception);
                            if (showError)
                                DisplayError((task.Exception.InnerException as FirebaseException).ErrorCode);
                        } finally {
                            _cb?.Invoke(false);
                        }
                        return;
                    }
                    Debug.LogFormat("User signed in successfully: {0} ({1})", MyUser.DisplayName, MyUser.UserId);
                    _cb?.Invoke(true);
                } catch (Exception _e) {
                    WriteLog.LogError("SignInWithCredential Error:" + _e);
                }

            });
        }

        private static void LinkAccountWithCredential(Credential credential, Action<bool> _cb, bool showError = true) {
            /*不確認玩家是否有三方登入，因為規劃上玩家要可以同時綁定多個不同三方平台
            if (!CheckAccountCanLink())
            {
                _cb?.Invoke(false);
                return;
            }
            */
            MyAuth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
                if (task == null) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
                    return;
                }
                if (task.IsCanceled) {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
                    return;
                }
                if (task.IsFaulted) {
                    try {
                        WriteLog.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                        if (showError) {
                            DisplayError((task.Exception.InnerException as FirebaseException).ErrorCode);
                        }
                    } finally {
                        _cb?.Invoke(false);
                    }
                    return;
                }
                _cb?.Invoke(true);
            });
        }

        private static void UnLinkAccountWithProvider(string provider, Action<bool> _cb, bool showError = true) {
            if (MyAuth.CurrentUser == null) {
                if (showError) {
                    PopupUI.ShowClickCancel("帳號並未登入", null);
                }
                _cb?.Invoke(false);
                return;
            }
            MyAuth.CurrentUser.UnlinkAsync(provider).ContinueWithOnMainThread(task => {
                if (task == null) {
                    _cb?.Invoke(false);
                    return;
                }
                if (task.IsCanceled) {
                    _cb?.Invoke(false);
                    return;
                }
                if (task.IsFaulted) {
                    try {
                        WriteLog.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                        if (showError) {
                            DisplayError((task.Exception.InnerException as FirebaseException).ErrorCode);
                        }
                    } finally {
                        _cb?.Invoke(false);
                    }
                    return;
                }
                _cb?.Invoke(true);
            });
        }

        private static bool CheckAccountCanLink(bool showError = true) {
            if (MyAuth.CurrentUser == null) {
                if (showError) {
                    PopupUI.ShowClickCancel("帳號並未登入", null);
                }
                return false;
            }
            IEnumerator<IUserInfo> provider = MyAuth.CurrentUser.ProviderData.GetEnumerator();
            bool hasThirdPart = false;
            while (provider.MoveNext()) {
                if (provider.Current.ProviderId != "firebase") {
                    hasThirdPart = true;
                    break;
                }
            }
            if (hasThirdPart) {
                if (showError) {
                    PopupUI.ShowClickCancel("該帳號已綁定其他登入方式", null);
                }
                Debug.Log("已經有綁定帳號了 " + provider.Current.ProviderId);
                return false;
            }
            return true;
        }

        private static void DisplayError(int _errorCode) {
            switch (_errorCode) {
                case 8:
                    PopupUI.ShowClickCancel("An account already exists with the same email address but different sign-in credentials", null);
                    break;
                case 10:
                    PopupUI.ShowClickCancel(StringData.GetUIString("ThridPartAlreadyBeingUsed"), null);
                    break;
                default:
                    PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("ThridPartLinkErrorCode"), _errorCode), null);
                    break;
            }
        }
    }
}