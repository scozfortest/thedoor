using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Facebook.Unity;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using Scoz.Func;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheDoor.Main {

    public partial class FirebaseManager {

        static IAppleAuthManager appleAuthManager;

        public static IAppleAuthManager MyAppleAuthManager {
            get {
                if (appleAuthManager != null) return appleAuthManager;
                // Apple 登入
                // If the current platform is supported
                if (AppleAuthManager.IsCurrentPlatformSupported) {
                    // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                    var deserializer = new PayloadDeserializer();
                    // Creates an Apple Authentication manager with the deserializer
                    var appleManager = new AppleAuthManager(deserializer);

                    appleAuthManager = appleManager;
                    return appleManager;
                }
                return null;
            }
        }
        void AppleRun() {
#if UNITY_IOS
            if (MyAppleAuthManager != null) {
                MyAppleAuthManager.Update();
            }
#endif
        }

        public static void SignInWithApple(Action<bool> _cb) {
            var appleAuthManager = MyAppleAuthManager;
            if (appleAuthManager == null) {
                Debug.LogError("SigninFail");
                PopupUI_Local.ShowClickCancel(StringData.GetUIString("SigninFail"), null);
                _cb?.Invoke(false);
                return;
            }
            var rawNonce = GenerateRandomString(32);
            var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
            appleAuthManager.LoginWithAppleId(loginArgs,
                credential => {
                    var appleIdCredential = credential as IAppleIDCredential;
                    Debug.Log(String.Format("appleIdCredential:{0} | rawNonce:{1}", appleIdCredential, rawNonce));

                    if (appleIdCredential != null) {
                        Debug.Log(String.Format("appleIdCredential:{0} | rawNonce:{1}", appleIdCredential, rawNonce));
                        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                        var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                        Credential firebaseCredential = OAuthProvider.GetCredential("apple.com", identityToken, rawNonce, authorizationCode);
                        if (firebaseCredential.IsValid())
                            SignInWithCredential(firebaseCredential, _cb);
                        else {
                            Debug.LogError("Apple credential is not valid");
                            _cb?.Invoke(false);
                        }
                    } else {
                        Debug.LogError("appleIdCredential is null");
                        _cb?.Invoke(false);
                    }
                },
                error => {
                    //取消也會走這裡
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    if (authorizationErrorCode == AuthorizationErrorCode.Canceled)
                        Debug.LogError("Cancel Login");
                    //PopupUI_Local.ShowClickCancel("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString(), () => {
                    //    _cb?.Invoke(false);
                    //});
                    Debug.LogError("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                    _cb?.Invoke(false);
                });
        }

        public static void LinkWithApple(Action<bool> _cb) {
            /*改不確認玩家是否有三方登入了，因為規劃上玩家要可以同時綁定多個不同三方平台
             if (!CheckAccountCanLink())
             {
                 _cb?.Invoke(false);
                 return;
             }
            */
            var appleAuthManager = MyAppleAuthManager;
            if (appleAuthManager == null) {
#if !Release
                PopupUI_Local.ShowClickCancel("appleAuthManager is null", () => {
                    _cb?.Invoke(false);
                });
#else
                PopupUI_Local.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                _cb?.Invoke(false);
#endif
                return;
            }
            var rawNonce = GenerateRandomString(32);
            var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
            appleAuthManager.LoginWithAppleId(loginArgs, credential => {
                var appleIdCredential = credential as IAppleIDCredential;
                Debug.Log(String.Format("appleIdCredential:{0} | rawNonce:{1}", appleIdCredential, rawNonce));

                if (appleIdCredential != null) {
                    Debug.Log(String.Format("appleIdCredential:{0} | rawNonce:{1}", appleIdCredential, rawNonce));
                    var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                    var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                    Credential firebaseCredential = OAuthProvider.GetCredential("apple.com", identityToken, rawNonce, authorizationCode);
                    if (firebaseCredential.IsValid())
                        LinkAccountWithCredential(firebaseCredential, (bool _result) => {
                            _cb?.Invoke(_result);
                        });
                    else {
#if !Release
                        PopupUI_Local.ShowClickCancel("Apple credential is not valid", () => {
                            _cb?.Invoke(false);
                        });
#else
                        PopupUI_Local.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                        _cb?.Invoke(false);
#endif
                        Debug.LogError("Apple credential is not valid");
                    }
                } else {
#if !Release
                    PopupUI_Local.ShowClickCancel("appleIdCredential is null", () => {
                        _cb?.Invoke(false);
                    });
#else
                    PopupUI_Local.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                    _cb?.Invoke(false);
#endif
                }
            },
            error => {
                PopupUI_Local.ShowClickCancel(StringData.GetUIString("ThridPartLinkError"), null);
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                _cb?.Invoke(false);
            });
        }

        /* YingYi: 這功能不應該用到，不用讓玩家解除 他解除了也不能用該第三方再創帳號  !Google可以再第三方再創帳號!
        */
        public static void UnlinkApple(Action<bool> _cb) {
            UnLinkAccountWithProvider("apple.com", _cb);
        }

        private static string GenerateRandomString(int length) {
            if (length <= 0) {
                throw new Exception("Expected nonce to have positive length");
            }

            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0) {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++) {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++) {
                    if (remainingLength == 0) {
                        break;
                    }

                    var randomNumber = randomNumbers[randomNumberIndex];
                    if (randomNumber < charset.Length) {
                        result += charset[randomNumber];
                        remainingLength--;
                    }
                }
            }
            return result;
        }

        private static string GenerateSHA256NonceFromRawNonce(string rawNonce) {
            var sha = new SHA256Managed();
            var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
            var hash = sha.ComputeHash(utf8RawNonce);

            var result = string.Empty;
            for (var i = 0; i < hash.Length; i++) {
                result += hash[i].ToString("x2");
            }
            return result;
        }
    }
}