using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedSupplyData : OwnedData {
        [ScozSerializable] public string OwnRoleUID { get; private set; }
        [ScozSerializable] public int Usage { get; private set; }
        [ScozSerializable] public int ID { get; private set; }

        public OwnedSupplyData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            OwnRoleUID = _data.TryGetValue("OwnRoleUID", out value) ? Convert.ToString(value) : default(string);
            Usage = _data.TryGetValue("Usage", out value) ? Convert.ToInt32(value) : 0;
            ID = _data.TryGetValue("ID", out value) ? Convert.ToInt32(value) : default(int);
        }

        public void AddUsage(int _value) {
            if (Usage == -1) return;//無限次數的道具無法增減使用次數
            if (_value == 0) return;
            WriteLog.LogColorFormat("{0} 道具剩餘次數改變 {1}->{2}", WriteLog.LogType.Player, SupplyData.GetData(ID).Name, Usage, Usage + _value);
            Usage += _value;
            if (Usage <= 0)
                GamePlayer.Instance.RemoveOwnedData(ColEnum.Supply, UID);
        }
        public void Consume() {
            WriteLog.LogColorFormat("失去道具 {0}", WriteLog.LogType.Player, SupplyData.GetData(ID).Name);
            Usage = 0;
            if (Usage <= 0)
                GamePlayer.Instance.RemoveOwnedData(ColEnum.Supply, UID);
        }


    }
}
