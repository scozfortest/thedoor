using Firebase.Extensions;
using Firebase.Firestore;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheDoor.Main {
    public partial class FirebaseManager {
        static Dictionary<string, Dictionary<string, ListenerRegistration>> ListeningPlayerOwnedDataList = new Dictionary<string, Dictionary<string, ListenerRegistration>>();//玩家擁有資料偵聽
        static Dictionary<string, ListenerRegistration> ListeningGameSettingList = new Dictionary<string, ListenerRegistration>();//遊戲設定偵聽
        static ListenerRegistration ListeningShop;//商店設定偵聽
        static ListenerRegistration ListeningPurchase;//儲值商店設定偵聽


        /// <summary>
        /// 取消所有偵聽
        /// </summary>
        static void StopAllListener() {
            //取消偵聽-遊戲設定
            foreach (var listener in ListeningGameSettingList.Values) {
                listener.Stop();
            }
            ListeningGameSettingList.Clear();
            //取消偵聽-商店
            if (ListeningShop != null) {
                ListeningShop.Stop();
                ListeningShop = null;
            }
            //取消偵聽-儲值
            if (ListeningPurchase != null) {
                ListeningPurchase.Stop();
                ListeningPurchase = null;
            }

            //取消偵聽-玩家擁有資料
            StopListenPlayerOwnedDatas();

        }
        public static void StopListenPlayerOwnedDatas() {
            //取消偵聽-玩家擁有資料
            foreach (var listenerDic in ListeningPlayerOwnedDataList.Values) {
                if (listenerDic == null)
                    continue;
                foreach (var listener in listenerDic.Values) {
                    if (listener == null)
                        continue;
                    listener.Stop();
                }
                listenerDic.Clear();
            }
            ListeningPlayerOwnedDataList.Clear();
        }

        /// <summary>
        /// 註冊偵聽-玩家擁有資料
        /// </summary>
        public static void RegisterListener_OwnedData(ColEnum _col, string _playerUID) {
            if (string.IsNullOrEmpty(_playerUID)) {
                WriteLog.LogError("RegisterListener_OwnedData傳入的PlayerUID為null或空值");
                return;
            }
            //已經註冊過此資料偵聽就不處理
            if (ListeningPlayerOwnedDataList.ContainsKey(_col.ToString())) {
                if (ListeningPlayerOwnedDataList[_col.ToString()].ContainsKey(_playerUID))
                    return;
            }

            //設定偵聽
            ListenerRegistration listener = null;
            string colName = ColNames.GetValueOrDefault(_col);
            if (string.IsNullOrEmpty(colName)) {
                WriteLog.LogErrorFormat("ColNames尚未定義 {0}", _col);
                return;
            }
            if (PerPlayerOwnedOneDocCols.Contains(_col.ToString())) {//一個玩家只會擁有一個doc類型的偵聽跑這裡(例如PlayerData-Player，PlayerData-Item集合中玩家只會擁有一個 Doc)

                DocumentReference docRef = Store.Collection(colName).Document(_playerUID);
                listener = docRef.Listen(snapshot => {
                    Dictionary<string, object> data = snapshot.ToDictionary();
                    SetReturnOwnedData(_col, data);//根據集合類型來處理取回的資料
                });
            } else {//一個玩家會擁有多個獨立doc類型的偵聽跑這裡(例如PlayerData-Mail，PlayerData-Mail集合中玩家會擁有不只一個 Doc)
                Query query = Store.Collection(colName).WhereEqualTo(OwnedEnum.OwnerUID.ToString(), _playerUID);
                listener = query.Listen(snapshot => {
                    if (snapshot.Count != 0) {
                        List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                        foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                            Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                            dataList.Add(datas);
                        }
                        SetReturnOwnedDatas(_col, dataList);
                    } else
                        SetReturnOwnedDatas(_col, null);
                });
            }

            //加入已偵聽清單
            if (listener == null) return;
            if (ListeningPlayerOwnedDataList.ContainsKey(_col.ToString())) {
                if (!ListeningPlayerOwnedDataList[_col.ToString()].ContainsKey(_playerUID))
                    ListeningPlayerOwnedDataList[_col.ToString()][_playerUID] = listener;
            } else {
                Dictionary<string, ListenerRegistration> dic = new Dictionary<string, ListenerRegistration>();
                dic.Add(_playerUID, listener);
                ListeningPlayerOwnedDataList.Add(_col.ToString(), dic);
            }
            WriteLog.LogFormat("<color=#9b791d>[Firebase] 註冊偵聽: {0} > {1} </color>", colName, _playerUID);
        }

        /// <summary>
        /// 設定回傳資料並刷新介面
        /// </summary>
        public static void SetReturnOwnedData(ColEnum _col, Dictionary<string, object> _data) {
            object obj;

            switch (_col) {
                case ColEnum.Player:
                    if (_data == null || !_data.TryGetValue("UID", out obj)) return;
                    string playerUID = obj.ToString();
                    if (playerUID == GamePlayer.Instance.Data.UID) {//是玩家自己
                        GamePlayer.Instance.SetMainPlayerData(_data);
                    } else {//其他玩家
                        GamePlayer.Instance.SetOtherPlayerData(_data);
                    }
                    break;
                case ColEnum.Item:
                    GamePlayer.Instance.SetOwnedData<OwnedItemData>(ColEnum.Item, _data);//更新資料
                    break;
                case ColEnum.History:
                    GamePlayer.Instance.SetOwnedData<OwnedHistoryData>(ColEnum.History, _data);//更新資料
                    break;
                default:
                    WriteLog.LogErrorFormat("SetReturnOwnedData未加入處理偵聽 {0} 類型的回傳方法", _col);
                    break;
            }
        }

        /// <summary>
        /// 設定回傳資料並刷新介面
        /// </summary>
        public static void SetReturnOwnedDatas(ColEnum _col, List<Dictionary<string, object>> _datas) {
            bool inLobby = SceneManager.GetActiveScene().name == MyScene.LobbyScene.ToString();//是否在大廳
            switch (_col) {
                case ColEnum.Role:
                    GamePlayer.Instance.SetOwnedDatas<OwnedRoleData>(ColEnum.Role, _datas);
                    break;
                case ColEnum.Supply:
                    GamePlayer.Instance.SetOwnedDatas<OwnedSupplyData>(ColEnum.Supply, _datas);
                    var roleInfo = RoleInfoUI.GetInstance<RoleInfoUI>();
                    if (roleInfo != null && roleInfo.gameObject.activeInHierarchy)
                        roleInfo.RefreshSupply();
                    break;
                default:
                    WriteLog.LogErrorFormat("SetReturnOwnedData未加入處理偵聽 {0} 類型的回傳方法", _col);
                    break;
            }
        }

        /// <summary>
        /// 取消偵聽-玩家擁有資料
        /// </summary>
        static void StopOwnedDataListener(ColEnum _col, string _playerUID) {
            if (ListeningPlayerOwnedDataList.ContainsKey(_col.ToString())) {
                if (ListeningPlayerOwnedDataList[_col.ToString()].ContainsKey(_playerUID))
                    ListeningPlayerOwnedDataList[_col.ToString()][_playerUID].Stop();
            }
        }

        /// <summary>
        /// 註冊偵聽-遊戲設定
        /// </summary>
        static void RegisterListener_GameSetting() {
            string[] docNames = Enum.GetNames(typeof(GameDataDocEnum));
            string registerDocNameStr = "";
            for (int i = 0; i < docNames.Length; i++) {
                DocumentReference docRef = Store.Collection(ColNames.GetValueOrDefault(ColEnum.GameSetting)).Document(docNames[i]);
                ListenerRegistration listener = docRef.Listen(snapshot => {
                    if (snapshot == null)
                        return;
                    Dictionary<string, object> doc = snapshot.ToDictionary();
                    if (doc == null)
                        return;
                    FirestoreGameSetting.UpdateSetting(doc);
                });
                if (registerDocNameStr != "")
                    registerDocNameStr += ", ";
                registerDocNameStr += docNames[i] + "表";
                if (!ListeningGameSettingList.ContainsKey(docNames[i]))
                    ListeningGameSettingList.Add(docNames[i], listener);
            }
            WriteLog.LogFormat("<color=#9b791d>[Firebase] 註冊GameSetting偵聽: {0}</color>", registerDocNameStr);
        }


        /// <summary>
        /// 註冊偵聽-商店
        /// </summary>
        static void RegisterListener_Shop() {
            string colName = ColNames.GetValueOrDefault(ColEnum.Shop);
            Query query = Store.Collection(colName);
            ListeningShop = query.Listen(snapshot => {
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    }
                    //GameData.SetShopDatas(dataList);
                    //LuckyBagUI.GetInstance<LuckyBagUI>()?.SpawnItems();
                    //LobbyUISelector.GetInstance<LobbyUISelector>()?.RefreshRedDot();//刷新紅點
                }
            });

            WriteLog.LogFormat("<color=#9b791d>[Firebase] 註冊偵聽: {0} </color>", colName);
        }


        /// <summary>
        /// 註冊偵聽-儲值
        /// </summary>
        static void RegisterListener_Purchase() {
            string colName = ColNames.GetValueOrDefault(ColEnum.Purchase);
            string platform = "Google";
#if UNITY_ANDROID
            platform = "Google";
#elif UNITY_IOS
            platform = "Apple";
#elif UNITY_STANDALONE_OSX
            platform = "Apple";
#elif UNITY_STANDALONE_WIN
            platform = "Google";
#endif

            Query query = Store.Collection(colName).WhereEqualTo("Platform", platform);
            ListeningPurchase = query.Listen(snapshot => {
                if (snapshot.Count != 0) {
                    List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot documentSnapshot in snapshot.Documents) {
                        Dictionary<string, object> datas = documentSnapshot.ToDictionary();
                        dataList.Add(datas);
                    }
                    //GameData.SetPurchaseDatas(dataList);
                    //PurchaseUI.GetInstance<PurchaseUI>()?.SpawnItems();
                }
            });

            WriteLog.LogFormat("<color=#9b791d>[Firebase] 註冊偵聽: {0} </color>", colName);
        }


    }
}
