using Firebase.Extensions;
using Firebase.Firestore;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

namespace TheDoor.Main {

    public partial class FirebaseManager {
        public static void GetToken(Action<string> _cb) {
            FirebaseUser user = MyAuth.CurrentUser;
            user.TokenAsync(true).ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    WriteLog.LogError("TokenAsync was canceled.");
                    _cb?.Invoke(null);
                    return;
                }

                if (task.IsFaulted) {
                    WriteLog.LogError("TokenAsync encountered an error: " + task.Exception);
                    _cb?.Invoke(null);
                    return;
                }

                string idToken = task.Result;

                _cb?.Invoke(idToken);
                // Send token to your backend via HTTPS
                // ...
            });
        }
        public static void GetThirdPartIcon(UsePlatformIcon _type, Action<Sprite> _cb) {
            MyDownloader.GetSpriteFromUrl_Coroutine(GetUserPhotoURL(_type), _cb);
        }
        public static string GetUserPhotoURL(UsePlatformIcon _type) {

            switch (_type) {
                case UsePlatformIcon.Facebook:
                    return GetFBUserIconURL();
                /*
            case UsePlatformIcon.Google:
                List<IUserInfo> providerData = FirebaseManager.MyUser.ProviderData as List<IUserInfo>;
                for (int i = 0; i < providerData.Count; i++) {
                    if (providerData[i].ProviderId.Contains("google"))
                        return providerData[i].PhotoUrl.ToString();
                }
                return "";
                */
                default:
                    return "";
            }
        }
        public static string GetFBUserIconURL() {
            string userID = "";
            List<IUserInfo> providerData = MyUser.ProviderData as List<IUserInfo>;
            for (int i = 0; i < providerData.Count; i++) {
                if (providerData[i].ProviderId.Contains("facebook"))
                    userID = providerData[i].UserId;
            }
            if (userID == "")
                return "";
            string photoUrl = "https://graph.facebook.com/" + userID + "/picture?height=256";
            return photoUrl;
        }
    }
}
