using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedFriendshipData : OwnedData {
        public List<string> FriendUIDs { get; private set; } = new List<string>();//好友UID清單

        public OwnedFriendshipData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            List<object> objList;
            //房間內玩家UID清單
            FriendUIDs.Clear();
            objList = _data.TryGetValue("FriendUIDs", out value) ? value as List<object> : null;
            if (objList != null)
                FriendUIDs = objList.ConvertAll(a => a.ToString());
        }

        public bool IsMyFriend(string _playerUID) {
            if (string.IsNullOrEmpty(_playerUID)) return false;
            return FriendUIDs.Contains(_playerUID);
        }

        /// <summary>
        /// 本機新增好友用
        /// </summary>
        public void AddFriend(string _playerUID) {
            if (FriendUIDs.Contains(_playerUID)) return;
            FriendUIDs.Add(_playerUID);
        }

    }
}
