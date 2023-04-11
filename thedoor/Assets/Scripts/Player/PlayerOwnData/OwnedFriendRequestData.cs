using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    public class OwnedFriendRequestData : OwnedData {
        public string SenderUID { get; private set; }
        static HashSet<DateTime> AlreadyNotifyList = new HashSet<DateTime>();//已經有跳彈窗通知的會加入這個清單

        public OwnedFriendRequestData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;
            SenderUID = _data.TryGetValue("SenderUID", out value) ? Convert.ToString(value) : default(string);
        }

        /// 是否需要通知
        /// </summary>
        public bool NeedNotify() {
            if (!AlreadyNotifyList.Contains(CreateTime)) {
                AlreadyNotifyList.Add(CreateTime);
                if ((GameManager.Instance.NowTime - CreateTime).TotalSeconds > 30)//超過30秒就不跳邀請通知了
                    return false;
                return true;
            }
            return false;
        }

    }
}
