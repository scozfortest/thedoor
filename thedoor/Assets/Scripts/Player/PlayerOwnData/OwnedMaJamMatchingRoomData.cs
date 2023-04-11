using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedMaJamMatchingRoomData : OwnedData {
        public string RoomID { get; private set; }
        public string RoomName { get; private set; }
        public List<string> PlayerUIDs { get; private set; } = new List<string>();//此配對房的玩家PlayerUID，第一筆是房主
        public bool Start { get; private set; }//是否開始遊戲了，true代表已經開始了，玩家無法點離開配對

        public OwnedMaJamMatchingRoomData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            string uid = _data.TryGetValue(OwnedEnum.UID.ToString(), out value) ? Convert.ToString(value) : default(string);
            RoomID = _data.TryGetValue("RoomID", out value) ? Convert.ToString(value) : default(string);
            RoomName = _data.TryGetValue("RoomName", out value) ? Convert.ToString(value) : default(string);
            Start = _data.TryGetValue("Start", out value) ? Convert.ToBoolean(value) : default(bool);
            List<object> objList;
            //房間內玩家UID清單
            PlayerUIDs.Clear();
            objList = _data.TryGetValue("PlayerUIDs", out value) ? value as List<object> : null;
            if (objList != null)
                PlayerUIDs = objList.ConvertAll(a => a.ToString());
        }

    }
}
