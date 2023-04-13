using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Functions;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Extensions;
using Scoz.Func;
using System;

namespace TheDoor.Main {

    public partial class FirebaseManager {
        enum ResultTypes {
            UnknownError,//回傳只會是Fail或Success，否則都是UnknownError
            Fail,//Fail時Data通常是回傳給Client顯示的文字內容
            Success,//Success時Data通常是回傳給Client需要的資料
        }

        /// <summary>
        /// 處理Cloud Function回傳的資料
        /// </summary>
        static void CFCallbackHandle(object _obj, Action<object> _successCB, Action<string> _failCB) {

            var iDic = (IDictionary)_obj;
            Dictionary<string, object> cbData = DictionaryExtension.ConvertToStringKeyDic(iDic);
            object value;
            ResultTypes resultType = cbData.TryGetValue("Result", out value) ? MyEnum.ParseEnum<ResultTypes>(value.ToString()) : ResultTypes.UnknownError;
            if (resultType == ResultTypes.Success) {
                object data = cbData.TryGetValue("Data", out value) ? value : null;
                _successCB?.Invoke(data);
            } else if (resultType == ResultTypes.Fail) {
                string failStr = cbData.TryGetValue("Data", out value) ? value.ToString() : null;
                _failCB?.Invoke(failStr);
            } else {
                WriteLog.LogError("Cloud Function回傳的類型錯誤:" + resultType);
            }
        }


        /// <summary>
        /// 取得Server DateTime
        /// </summary>
        public static void GetServerTime(Action<DateTime> _cb) {
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable("GetServerTime");
            function.CallAsync(null).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        _cb?.Invoke(DateTime.Now);
                        return;
                    } else {
                        CFCallbackHandle(task.Result.Data, dataObj => {
                            DateTime time = TextManager.GetDateTimeFormScozTimeStr(dataObj.ToString());
                            _cb?.Invoke(time);
                        }, null);
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                    _cb?.Invoke(default(DateTime));
                }
            });
        }
        /// <summary>
        /// 註冊送LOG
        /// </summary>
        public static void PlayerSign_SignUp() {
            string funcName = "SignUp";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            function.CallAsync(null).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        return;
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                }
            });
        }
        /// <summary>
        /// 登入送LOG
        /// </summary>
        public static void PlayerSign_SignIn() {
            string funcName = "SignIn";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            function.CallAsync(null).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        return;
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                }
            });
        }
        /// <summary>
        /// 設定裝置UID(只有登入帳號會執行一次)
        /// </summary>
        public static void PlayerSign_SetDevice(Action _cb) {
            string funcName = "SetDevice";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("DeviceUID", SystemInfo.deviceUniqueIdentifier);

            function.CallAsync(data).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        return;
                    } else {
                        _cb?.Invoke();
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                }
            });
        }




        /// <summary>
        /// 玩家更名
        /// </summary>
        public static void PlayerSign_ChangeName(string _nameBefore, string _nameAfter, Action<string> _cb) {
            if (string.IsNullOrEmpty(_nameAfter)) return;
            string funcName = "ChangeName";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("NameBefore", _nameBefore);
            data.Add("NameAfter", _nameAfter);
            function.CallAsync(data).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        _cb?.Invoke("UnknownError");
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                        if (task.Result.Data == null) {
                            _cb?.Invoke("UnknownError");
                        } else {
                            CFCallbackHandle(task.Result.Data, successData => {
                                _cb?.Invoke("Success");
                            }, failStr => {
                                _cb?.Invoke(failStr);
                            });
                        }
                    }
                } catch (Exception _e) {
                    _cb?.Invoke(null);
                    WriteLog.LogError(_e);
                    PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }
        /// <summary>
        /// 玩家更改自介
        /// </summary>
        public static void PlayerSign_ChangeIntro(string _introBefore, string _introAfter, Action<string> _cb) {
            if (string.IsNullOrEmpty(_introAfter)) return;
            string funcName = "ChangeIntro";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("IntroBefore", _introBefore);
            data.Add("IntroAfter", _introAfter);
            function.CallAsync(data).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        _cb?.Invoke("UnknownError");
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                        if (task.Result.Data == null) {
                            _cb?.Invoke("UnknownError");
                        } else {
                            CFCallbackHandle(task.Result.Data, successData => {
                                _cb?.Invoke("Success");
                            }, failStr => {
                                _cb?.Invoke(failStr);
                            });
                        }
                    }
                } catch (Exception _e) {
                    _cb?.Invoke(null);
                    WriteLog.LogError(_e);
                    PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }
        /// <summary>
        /// 1. 玩家上線時，每間隔一段時間會送更新時間戳
        /// 2. 玩家背景作業回來也會送更新時間戳
        /// 送的頻率參考GameData - Setting -> Timer -> OnlineTimeStampCD
        /// </summary>
        public static void PlayerSign_UpdateOnlineTimestamp() {
            if (MyUser == null) return;//登出時Timer還是會跑，所以加判斷避免跳紅字
            string funcName = "UpdateOnlineTimestamp";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            function.CallAsync(null).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        return;
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                }
            });
        }
        /// <summary>
        /// 初始化帳號資料
        /// </summary>
        public static void SignUp(AuthType _authType, Action<Dictionary<string, object>> _cb) {

            string funcName = "SignUp";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("AuthType", _authType.ToString());
            function.CallAsync(data).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                        if (task.Result.Data == null) {
                            _cb?.Invoke(null);
                        } else {
                            CFCallbackHandle(task.Result.Data, dataObj => {
                                if (dataObj == null) {
                                    _cb?.Invoke(null);
                                } else {
                                    Dictionary<string, object> data = DictionaryExtension.ConvertToStringKeyDic((IDictionary)dataObj);
                                    _cb?.Invoke(data);
                                }
                            }, null);
                        }
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                    PopupUI_Local.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }
        /// <summary>
        /// 商城購買
        /// </summary>
        public static void Shop_Buy(string _shopUID, BuyCount _buyCount, Action<object> _cb) {
            string funcName = "Shop_Buy";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("ShopUID", _shopUID);
            data.Add("BuyCount", _buyCount.ToString());
            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
            function.CallAsync(data).ContinueWithOnMainThread(task => {
                PopupUI.HideLoading();
                try {
                    if (task.IsFaulted) {
                        _cb?.Invoke(null);
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                        CFCallbackHandle(task.Result.Data, dataObj => {
                            if (dataObj == null) {
                                _cb?.Invoke(null);
                            } else {
                                _cb?.Invoke(dataObj);
                            }
                        }, null);
                    }
                } catch (Exception _e) {
                    _cb?.Invoke(null);
                    WriteLog.LogError(_e);
                    PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }
        /// <summary>
        /// 儲值
        /// </summary>
        public static void Purchase(string _purchaseUID, object _receipt, Action<object> _cb) {
            string funcName = "Purchase";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("PurchaseUID", _purchaseUID);
            data.Add("Receipt", _receipt);
            data.Add("Version", Application.version);
            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
            function.CallAsync(data).ContinueWithOnMainThread(task => {
                PopupUI.HideLoading();
                try {
                    if (task.IsFaulted) {
                        _cb?.Invoke(null);
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                        CFCallbackHandle(task.Result.Data, dataObj => {
                            if (dataObj == null) {
                                _cb?.Invoke(null);
                            } else {
                                _cb?.Invoke(dataObj);
                            }
                        }, null);
                    }
                } catch (Exception _e) {
                    _cb?.Invoke(null);
                    WriteLog.LogError(_e);
                    PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }


        /// <summary>
        /// 設定玩家最後購買的PurchaseUID
        /// </summary>
        public static void SetBougthShopUID(string _bougthShopUID, Action<bool> _cb) {
            string funcName = "SetBougthShopUID";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("BougthShopUID", _bougthShopUID);

            function.CallAsync(data).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        _cb?.Invoke(false);
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                        _cb?.Invoke(true);
                    }
                } catch (Exception _e) {
                    _cb?.Invoke(false);
                    WriteLog.LogError(_e);
                    PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }


        /// <summary>
        /// 紀錄觸發事件
        /// </summary>
        public static void TriggerEvent(TriggerEvent _type) {
            string funcName = "TriggerEvent";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();
            data.Add("Type", _type.ToString());
            function.CallAsync(data).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        WriteLog.LogError("Error:" + task.Exception.ToString());
                        PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                        return;
                    } else {
                    }
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                    PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("Firebase_UnexpectedError"), funcName), null);
                }
            });
        }





        /*
        /// <summary>
        /// 暫時用 送全服玩家信
        /// </summary>
        public static void NewsTicker_TmpSendAllMail() {
            Debug.LogError("NewsTicker_TmpSendAllMail");
            string funcName = "NewsTicker_TmpSendAllMail";
            var function = FirebaseFunctions.GetInstance(MyFirebaseApp, Region).GetHttpsCallable(funcName);
            var data = new Dictionary<string, object>();

            function.CallAsync(null).ContinueWithOnMainThread(task => {
                try {
                    if (task.IsFaulted) {
                        DebugLogger.LogError("Error:" + task.Exception.ToString());
                        return;
                    } else {
                        Debug.LogError("成功");
                    }
                } catch (Exception _e) {
                    DebugLogger.LogError(_e);
                }
            });
        }
        */

    }
}