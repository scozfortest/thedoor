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
            Usage = _data.TryGetValue("Usage", out value) ? Convert.ToInt32(value) : default(int);
            ID = _data.TryGetValue("ID", out value) ? Convert.ToInt32(value) : default(int);
        }

        public void AddUsage(int _value) {
            if (_value == 0) return;
            Usage += _value;
            if (Usage <= 0)
                GamePlayer.Instance.RemoveOwnedData(ColEnum.Supply, UID);
        }

    }
}
