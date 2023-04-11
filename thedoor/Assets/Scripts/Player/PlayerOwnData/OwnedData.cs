using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {

    public abstract class OwnedData {
        public string UID { get; private set; }
        public string OwnerUID { get; private set; }
        public DateTime CreateTime { get; private set; }

        public OwnedData(Dictionary<string, object> _data) {
            SetData(_data);
        }
        public virtual void SetData(Dictionary<string, object> _data) {
            object value;
            UID = _data.TryGetValue(OwnedEnum.UID.ToString(), out value) ? Convert.ToString(value) : default(string);
            OwnerUID = _data.TryGetValue(OwnedEnum.OwnerUID.ToString(), out value) ? Convert.ToString(value) : default(string);
            CreateTime = _data.TryGetValue(OwnedEnum.CreateTime.ToString(), out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
        }
    }
}
