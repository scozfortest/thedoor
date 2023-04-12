using Firebase.Extensions;
using Firebase.Firestore;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheDoor.Main {
    public partial class FirebaseManager {
        public static void AnonymousSignup(Action _cb) {

            MyAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI_Local.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", "AnonymousSignup")), null);
                    DebugLogger.LogError("Anonymous Signup Error:" + task.Exception.ToString());
                    return;
                }
                _cb?.Invoke();
            });
        }

        /// <summary>
        /// 取得遊戲設定(會抓取GameData-Setting中所有Doc的資料(但要有UID欄位的Doc才會抓))
        /// </summary>
        public static void GetGameSettingDatas(ColEnum _col, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }

            CollectionReference colRef = Store.Collection(colName);
            Query query = colRef.WhereNotEqualTo(OwnedEnum.UID.ToString(), null);
            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("GetGameSettingDatas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }



        /// <summary>
        /// (單筆)取得某個資料，傳入資料表名稱與文件UID
        /// </summary>
        public static void GetDataByDocID(ColEnum _col, string _uid, Action<ColEnum, Dictionary<string, object>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }

            DocumentReference docRef = Store.Collection(colName).Document(_uid);
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Data {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                    _cb?.Invoke(_col, snapshot.ToDictionary());
                else
                    _cb?.Invoke(_col, null);
            });
        }





        /// <summary>
        /// (多筆)取得某個玩家多筆個人資料
        /// </summary>
        public static void GetPersonalDatas(ColEnum _col, string _playerUID, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (colName == null) {
                DebugLogger.LogErrorFormat("ColNames Error ");
                return;
            }
            if (MyUser == null) {
                _cb?.Invoke(_col, null);
                return;
            }
            CollectionReference colRef = Store.Collection(colName);
            Query query = colRef.WhereEqualTo(OwnedEnum.OwnerUID.ToString(), _playerUID);
            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Datas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }
        /// <summary>
        /// (多筆)取得多個玩家的個人資料
        /// </summary>
        public static void GetMultipleDatas(ColEnum _col, List<string> _playerUIDs, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (colName == null) {
                DebugLogger.LogErrorFormat("ColNames Error ");
                return;
            }
            if (MyUser == null) {
                _cb?.Invoke(_col, null);
                return;
            }

            CollectionReference colRef = Store.Collection(colName);
            Query query = colRef.WhereIn(OwnedEnum.UID.ToString(), _playerUIDs);
            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Datas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }

        /// <summary>
        /// 更新玩家個人資料
        /// </summary>
        public static void UpdatePersonalData(ColEnum _col, string _playerUID, Dictionary<string, object> _data, Action<bool> _cb) {
            if (_data == null) {
                DebugLogger.LogErrorFormat("Data is null ");
                return;
            }

            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }

            DocumentReference docRef = Store.Collection(colName).Document(_playerUID);
            docRef.UpdateAsync(_data).ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Update Personal Data {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                _cb?.Invoke(true);
            });
        }

        /// <summary>
        /// (多筆)根據條件取得資料
        /// </summary>
        public static void GetDatas(ColEnum _col, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }

            Query query = Store.Collection(colName);


            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Datas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }
        /// <summary>
        /// (多筆)根據條件取得資料
        /// </summary>
        public static void GetDatas_WhereIn(ColEnum _col, string _fieldName, object _value, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }

            Query query = Store.Collection(colName).WhereArrayContains(_fieldName, _value);


            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Datas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }



        /// <summary>
        /// (多筆)根據條件取得資料
        /// </summary>
        public static void GetDatas_WhereOperation(ColEnum _col, string _fieldName, Operation _operation, object _value, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }

            Query query = null;
            switch (_operation) {
                case Operation.EqualTo:
                    query = Store.Collection(colName).WhereEqualTo(_fieldName, _value);
                    break;
                case Operation.GreaterThan:
                    query = Store.Collection(colName).WhereGreaterThan(_fieldName, _value);
                    break;
                case Operation.GreaterThanOrEqualTo:
                    query = Store.Collection(colName).WhereGreaterThanOrEqualTo(_fieldName, _value);
                    break;
                case Operation.LessThan:
                    query = Store.Collection(colName).WhereLessThan(_fieldName, _value);
                    break;
                case Operation.LessThanOrEqualTo:
                    query = Store.Collection(colName).WhereLessThanOrEqualTo(_fieldName, _value);
                    break;
            }
            if (query == null) {
                DebugLogger.LogErrorFormat("Query錯誤");
                return;
            }


            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Datas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }


        /// <summary>
        /// (多筆)根據資料排序取得限定比數的資料
        /// </summary>
        public static void GetDatas_SortLimit(ColEnum _col, string _fieldName, OrderType _orderType, int _limit, Action<ColEnum, List<Dictionary<string, object>>> _cb) {
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                DebugLogger.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }
            Query query = null;
            switch (_orderType) {
                case OrderType.Descend:
                    query = Store.Collection(colName).OrderByDescending(_fieldName).Limit(_limit);
                    break;
                case OrderType.Ascend:
                    query = Store.Collection(colName).OrderBy(_fieldName).Limit(_limit);
                    break;
            }
            if (query == null) {
                DebugLogger.LogErrorFormat("Query錯誤");
                return;
            }

            query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    PopupUI.ShowClickCancel(StringData.GetUIString(string.Format("Firebase_UnexpectedError", colName)), null);
                    DebugLogger.LogErrorFormat("Get Personal Datas {0} Error: {1}", colName, task.Exception.ToString());
                    return;
                }
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    };
                    _cb?.Invoke(_col, dataList);
                } else {
                    _cb?.Invoke(_col, null);
                }
            });
        }




    }
}
