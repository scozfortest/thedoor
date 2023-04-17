using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;

namespace TheDoor.Main {

    public partial class GamePlayer : MyPlayer {
        public new static GamePlayer Instance { get; private set; }
        public MainPlayerData Data { get; private set; }
        Dictionary<ColEnum, IDictionary> OwnedDatas = new Dictionary<ColEnum, IDictionary>();
        /// <summary>
        /// 取得歷史資料
        /// </summary>
        public OwnedHistoryData MyHistoryData {
            get {
                return GetOwnedData<OwnedHistoryData>(ColEnum.History, Data.UID);
            }
        }
        /// <summary>
        /// 取得道具資料
        /// </summary>
        public OwnedItemData MyItemData {
            get {
                return GetOwnedData<OwnedItemData>(ColEnum.Item, Data.UID);
            }
        }
        //登入後會先存裝置UID到DB，存好後AlreadSetDeviceUID會設為true，所以之後從DB取到的裝置的UID應該都跟目前的裝置一致，若不一致代表是有其他裝置登入同個帳號
        public bool AlreadSetDeviceUID { get; set; } = false;

        public GamePlayer()
        : base() {
            Instance = this;
        }
        public override void GetLocoData() {
            base.GetLocoData();
            LoadPlayerSettingFromLoco();
            LoadReDotFromLoco();
        }
        public void InitMainPlayerData(Dictionary<string, object> _data) {
            if (Data == null)
                Data = new MainPlayerData();

            SetMainPlayerData(_data);

        }

        public void SetMainPlayerData(Dictionary<string, object> _data) {
            if (Data == null) {
                WriteLog.LogError("尚未初始化MainPlayerData");
                return;
            }
            Data.SetData(_data);
        }

        /// <summary>
        /// 傳入資料類型與資料，設定玩家擁有的資料(單筆資料)
        /// </summary>
        public void SetOwnedData<T>(ColEnum _colName, Dictionary<string, object> _data) where T : OwnedData {
            if (_data == null)
                return;
            if (!OwnedDatas.ContainsKey(_colName) || OwnedDatas[_colName] == null)
                OwnedDatas[_colName] = new Dictionary<string, object>();

            try {
                //設定資料
                string uid = _data["UID"].ToString();
                T ownedData = (T)Activator.CreateInstance(typeof(T), _data);
                if (!OwnedDatas[_colName].Contains(uid)) {
                    OwnedDatas[_colName].Add(uid, ownedData);
                } else
                    OwnedDatas[_colName][uid] = ownedData;
            } catch (Exception _e) {
                WriteLog.LogError("SetOwnedData錯誤: " + _e);
            }

        }

        /// <summary>
        /// 移除玩家擁有資料
        /// </summary>
        public void RemoveOwnedData(ColEnum _colName, string _uid) {
            if (!OwnedDatas.ContainsKey(_colName) || OwnedDatas[_colName] == null)
                return;
            OwnedDatas[_colName].Remove(_uid);
        }

        /// <summary>
        /// 傳入資料類型與資料，設定玩家擁有的資料(多筆資料)
        /// </summary>
        public void SetOwnedDatas<T>(ColEnum _colName, List<Dictionary<string, object>> _datas) where T : OwnedData {
            //清空資料
            if (!OwnedDatas.ContainsKey(_colName) || OwnedDatas[_colName] == null)
                OwnedDatas[_colName] = new Dictionary<string, object>();
            else
                OwnedDatas[_colName].Clear();
            if (_datas == null)
                return;
            try {
                //設定資料
                for (int i = 0; i < _datas.Count; i++) {
                    var data = _datas[i];
                    string uid = data["UID"].ToString();
                    T ownedData = (T)Activator.CreateInstance(typeof(T), data);
                    if (!OwnedDatas[_colName].Contains(uid)) {
                        //DebugLogger.LogErrorFormat("新增{0}資料UID:{1} ID:{2}", _type, uid, data["ID"]);
                        OwnedDatas[_colName].Add(uid, ownedData);
                    } else
                        WriteLog.LogErrorFormat("{0}資料有重複的UID:" + uid);
                }
            } catch (Exception _e) {
                WriteLog.LogError("SetOwnedDatas錯誤: " + _e);
            }
        }

        /// <summary>
        /// 傳入資料類型與UID，取得玩家自己擁有的資料
        /// </summary>
        public T GetOwnedData<T>(ColEnum _colName, string _uid) where T : OwnedData {
            if (!OwnedDatas.ContainsKey(_colName))
                return null;
            if (OwnedDatas[_colName] == null)
                return null;
            return OwnedDatas[_colName][_uid] as T;
        }
        //下載並更新玩家擁有資料(單筆)
        public void DownloadAndUpdatePlayerOwnedData(ColEnum _colName, Action _cb) {
            if (FirebaseManager.MyUser == null) {
                _cb?.Invoke();
                return;
            }
            FirebaseManager.GetDataByDocID(_colName, FirebaseManager.MyUser.UserId, (col, data) => {
                FirebaseManager.SetReturnOwnedData(_colName, data);
                _cb?.Invoke();
            });
        }
        //下載並更新玩家擁有資料(多筆)
        public void DownloadAndUpdatePlayerOwnedDatas(ColEnum _colName, Action _cb) {
            if (FirebaseManager.MyUser == null) {
                _cb?.Invoke();
                return;
            }
            FirebaseManager.GetPersonalDatas(_colName, FirebaseManager.MyUser.UserId, (col, datas) => {
                FirebaseManager.SetReturnOwnedDatas(_colName, datas);
                _cb?.Invoke();
            });
        }
        /// <summary>
        /// 傳入資料類型與UID，刪除玩家擁有的資料
        /// </summary>
        public void RemoveOwnedData<T>(ColEnum _colName, string _uid) where T : OwnedData {
            if (!OwnedDatas.ContainsKey(_colName)) return;
            if (OwnedDatas[_colName] == null) return;
            if (!OwnedDatas[_colName].Contains(_uid)) return;
            OwnedDatas[_colName][_uid] = null;
            OwnedDatas[_colName].Remove(_uid);
        }
        /// <summary>
        /// 傳入資料類型與UID，取得玩家擁有的資料(無資料會回傳0長度的List<T> 不會回傳null)
        /// </summary>
        public List<T> GetOwnedDatas<T>(ColEnum _colName) where T : OwnedData {
            if (!OwnedDatas.ContainsKey(_colName))
                return null;
            if (OwnedDatas[_colName] == null)
                return new List<T>();
            return OwnedDatas[_colName].Values.Cast<T>().ToList();
        }

        /// <summary>
        /// 取得TypeItemDic(玩家擁有非獨立資料類道具的字典，格式參考為:
        /// [表格ID]:[數量]
        /// </summary>
        public Dictionary<int, int> GetOwneItemDic(NotUniqueItemTypes _type) {
            var ownedItemData = MyItemData;
            if (ownedItemData != null)
                return ownedItemData.GetItemDic(_type);
            return null;
        }

        /// <summary>
        /// 傳入道具類型與ID，取得玩家擁有此ID道具的數量
        /// </summary>
        public int GetItemCount(NotUniqueItemTypes _type, int _id) {
            var ownedItemDic = GetOwneItemDic(_type);
            if (ownedItemDic != null && ownedItemDic.ContainsKey(_id))
                return ownedItemDic[_id];
            return 0;
        }

    }
}