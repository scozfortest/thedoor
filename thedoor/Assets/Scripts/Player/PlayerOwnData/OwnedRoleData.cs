using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedRoleData : OwnedData {
        public int RoleID { get; private set; }
        public bool EnableCall { get; private set; }


        public OwnedRoleData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            RoleID = _data.TryGetValue("RoleID", out value) ? Convert.ToInt32(value) : default(int);
            EnableCall = _data.TryGetValue("RoleID", out value) ? Convert.ToBoolean(value) : true;
        }

    }
}
