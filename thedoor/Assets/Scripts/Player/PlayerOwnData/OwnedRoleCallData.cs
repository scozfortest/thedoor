using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedRoleCallData : OwnedData {
        public int RoleID { get; private set; }
        public bool Called { get; private set; }
        public DateTime CallTime { get; private set; }


        public OwnedRoleCallData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            RoleID = _data.TryGetValue("RoleID", out value) ? Convert.ToInt32(value) : default(int);
            Called = _data.TryGetValue("Called", out value) ? Convert.ToBoolean(value) : default(bool);
            CallTime = _data.TryGetValue("CallTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
        }

    }
}
