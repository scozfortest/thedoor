using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;

namespace TheDoor.Main {

    public partial class GamePlayer : MyPlayer {

        Dictionary<string, PlayerData> OtherPlayerDatas = new Dictionary<string, PlayerData>();//[PlayerUID][OtherPlayerData]其他玩家資料
        Dictionary<string, OwnedHistoryData> OtherHistoryDatas = new Dictionary<string, OwnedHistoryData>();//[PlayerUID][OwnedHistoryData]其他玩家歷史資料

        /// <summary>
        /// 偵聽到資料後更新其他玩家資料
        /// </summary>
        public void SetOtherPlayerData(Dictionary<string, object> _data) {
            if (_data == null)
                return;
            PlayerData otherPlayerData = new PlayerData();
            otherPlayerData.SetData(_data);
            OtherPlayerDatas[otherPlayerData.UID] = otherPlayerData;
        }

        /// <summary>
        /// 跟DB取得玩家資料
        /// </summary>
        public void GetNewestPlayerData(string _playerUID, Action<PlayerData> _cb) {
            FirebaseManager.GetDataByDocID(ColEnum.Player, _playerUID, (colEnum, data) => {
                if (data != null) {
                    var playerData = new PlayerData();
                    playerData.SetData(data);
                    OtherPlayerDatas[_playerUID] = playerData;
                    _cb?.Invoke(playerData);
                }
            });
        }

        /// <summary>
        /// (單筆)取得其他玩家的PlayerData資料，如果還沒有該資料就從Firebase上取
        /// </summary>
        public void GetOtherPlayerData(string _playerUID, int _index, Action<PlayerData, int> _cb) {
            if (_playerUID == Data.UID) {
                _cb?.Invoke(Data, _index);
                return;
            } else if (OtherPlayerDatas.ContainsKey(_playerUID)) {
                _cb?.Invoke(OtherPlayerDatas[_playerUID], _index);
                return;
            } else {
                FirebaseManager.GetDataByDocID(ColEnum.Player, _playerUID, (colEnum, data) => {
                    if (OtherPlayerDatas.ContainsKey(_playerUID)) {
                        _cb?.Invoke(OtherPlayerDatas[_playerUID], _index);
                        return;
                    }
                    if (data != null) {
                        var playerData = new PlayerData();
                        playerData.SetData(data);
                        OtherPlayerDatas.Add(_playerUID, playerData);
                        _cb?.Invoke(playerData, _index);
                        return;
                    }
                });
            }
        }
        /// <summary>
        /// (單筆)取得其他玩家的PlayerData資料，如果還沒有該資料就從Firebase上取
        /// </summary>
        public void GetOtherPlayerData(string _playerUID, Action<PlayerData> _cb) {
            GetOtherPlayerData(_playerUID, 0, (data, index) => { _cb?.Invoke(data); });
        }
        /// <summary>
        /// (單筆)確定已經有載入過此玩家資料的話，就從這裡取得其他玩家的PlayerData資料，如果該玩家資料可能還沒載下來過，就用GetOtherPlayerData(string _playerUID, Action<PlayerData> _cb)
        /// </summary>
        public PlayerData GetOtherPlayerData(string _playerUID) {
            if (OtherPlayerDatas.ContainsKey(_playerUID))
                return OtherPlayerDatas[_playerUID];
            return null;
        }
        /// <summary>
        /// 從Firebase上取得多個其他玩家的PlayerData資料
        /// </summary>
        public void GetOtherPlayerDatas(List<string> _playerUIDs, Action<List<PlayerData>> _cb) {
            if (_playerUIDs == null || _playerUIDs.Count == 0) return;
            FirebaseManager.GetMultipleDatas(ColEnum.Player, _playerUIDs, (colEnum, dataList) => {
                if (dataList == null || dataList.Count == 0) return;
                List<PlayerData> playerDatas = new List<PlayerData>();
                for (int i = 0; i < dataList.Count; i++) {
                    var playerData = new PlayerData();
                    playerData.SetData(dataList[i]);
                    string uid = playerData.UID;
                    if (OtherPlayerDatas.ContainsKey(uid))
                        OtherPlayerDatas[uid] = playerData;
                    else
                        OtherPlayerDatas.Add(uid, playerData);
                    playerDatas.Add(playerData);
                }
                _cb?.Invoke(playerDatas);
            });
        }







        /// <summary>
        /// (單筆)取得其他玩家的HistoryData資料，如果還沒有該資料就從Firebase上取
        /// </summary>
        public void GetOtherHistoryData(string _playerUID, bool _getNewest, Action<OwnedHistoryData> _cb) {
            if (_playerUID == Data.UID) {//自己的資料有偵聽所以直接取就可以
                var history = GetOwnedDataByUID<OwnedHistoryData>(ColEnum.History, Data.UID);
                _cb?.Invoke(history);
                return;
            } else if (OtherHistoryDatas.ContainsKey(_playerUID) && _getNewest == false) {
                _cb?.Invoke(OtherHistoryDatas[_playerUID]);
                return;
            } else {
                FirebaseManager.GetDataByDocID(ColEnum.History, _playerUID, (colEnum, data) => {
                    if (OtherHistoryDatas.ContainsKey(_playerUID)) {
                        _cb?.Invoke(OtherHistoryDatas[_playerUID]);
                        return;
                    }
                    if (data != null) {
                        var historyData = new OwnedHistoryData(data);
                        OtherHistoryDatas.Add(_playerUID, historyData);
                        _cb?.Invoke(historyData);
                        return;
                    }
                });
            }
        }




    }
}