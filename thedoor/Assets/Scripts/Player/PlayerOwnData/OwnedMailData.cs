using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedMailData : OwnedData {
        public DateTime ClaimTime { get; private set; }
        public string SenderUID { get; private set; }
        public string Title { get; private set; } = "";
        public List<ItemData> MyItems { get; private set; } = new List<ItemData>();
        public DateTime RemoveTime { get; private set; }

        public OwnedMailData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            ClaimTime = _data.TryGetValue("ClaimTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
            RemoveTime = _data.TryGetValue("RemoveTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
            SenderUID = _data.TryGetValue("SenderUID", out value) ? Convert.ToString(value) : default(string);
            Title = _data.TryGetValue("Title", out value) ? Convert.ToString(value) : "";
            MyItems.Clear();
            if (_data.TryGetValue("Items", out value)) {
                List<object> objs = value as List<object>;
                SetItems(objs);
            }
        }
        void SetItems(List<object> _objs) {
            if (_objs != null) {
                for (int i = 0; i < _objs.Count; i++) {
                    var itemDic = _objs[i] as Dictionary<string, object>;
                    ItemType itemType;
                    if (MyEnum.TryParseEnum(itemDic["ItemType"].ToString(), out itemType)) {
                        ItemData itemData = new ItemData(itemType, Convert.ToInt64(itemDic["ItemValue"]));
                        MyItems.Add(itemData);
                    }
                }
            }
        }

    }
}
