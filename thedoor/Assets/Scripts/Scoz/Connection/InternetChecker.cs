using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Scoz.Func {
    public class InternetChecker : MonoBehaviour {
        static InternetChecker Instance;
        Action OnConnectedAction;
        Coroutine CheckInternetCoroutine;
        public void Init() {
            Instance = this;
        }
        public static void SetOnConnectedAction(Action _action) {
            Instance.OnConnectedAction = _action;
        }
        public static bool InternetConnected {
            get {
                if (Application.internetReachability == 0) //Not reachable at all
                {
                    DebugLogger.LogError("No internet connection");
                    return false;
                }
                return true;
            }
        }
        public static void StartCheckInternet() {
            Instance.CheckInternetCoroutine = Instance.StartCoroutine(Instance.CheckInternet());
        }


        IEnumerator CheckInternet() {
            if (!InternetConnected) {
                PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("NoInternetReTry"), "Disconnected"), () => {
                    CheckInternetCoroutine = StartCoroutine(CheckInternet());
                });
                if (CheckInternetCoroutine != null)
                    StopCoroutine(CheckInternetCoroutine);
            }
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://www.google.com/")) {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                switch (webRequest.result) {
                    case UnityWebRequest.Result.ConnectionError:
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("NoInternetReTry"), webRequest.result), () => {
                            CheckInternetCoroutine = StartCoroutine(CheckInternet());
                        });
                        if (CheckInternetCoroutine != null)
                            StopCoroutine(CheckInternetCoroutine);
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("NoInternetReTry"), webRequest.result), () => {
                            CheckInternetCoroutine = StartCoroutine(CheckInternet());
                        });
                        if (CheckInternetCoroutine != null)
                            StopCoroutine(CheckInternetCoroutine);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("NoInternetReTry"), webRequest.result), () => {
                            CheckInternetCoroutine = StartCoroutine(CheckInternet());
                        });
                        if (CheckInternetCoroutine != null)
                            StopCoroutine(CheckInternetCoroutine);
                        break;
                    case UnityWebRequest.Result.Success:
                        if (CheckInternetCoroutine != null)
                            StopCoroutine(CheckInternetCoroutine);
                        OnConnectedAction?.Invoke();
                        break;
                    default:
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("NoInternetReTry"), webRequest.result), () => {
                            CheckInternetCoroutine = StartCoroutine(CheckInternet());
                        });
                        if (CheckInternetCoroutine != null)
                            StopCoroutine(CheckInternetCoroutine);
                        break;
                }
            }
        }
    }
}
