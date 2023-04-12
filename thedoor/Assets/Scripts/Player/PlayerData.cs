using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public class PlayerData {

        public string UID { get; protected set; }//玩家UID
        public DateTime CreateTime { get; private set; }//註冊時間

        Dictionary<Currency, long> OwnedCurrency = new Dictionary<Currency, long>();//玩家擁有貨幣

        public AuthType MyAuthType { get; private set; } = AuthType.Guest;//玩家登入類型，參考AuthType列舉

        /// <summary>
        /// 取得玩家擁有貨幣
        /// </summary>
        public long GetCurrency(Currency _currency) {
            long value = 0;
            OwnedCurrency.TryGetValue(_currency, out value);
            return value;
        }

        public virtual void SetData(Dictionary<string, object> _data) {
            if (_data == null) {
                DebugLogger.LogErrorFormat("設定PlayerData時傳入資料為Null");
                return;
            }
            object value;
            UID = _data.TryGetValue("UID", out value) ? Convert.ToString(value) : default(string);
            CreateTime = _data.TryGetValue("CreateTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
            OwnedCurrency[Currency.Gold] = _data.TryGetValue("Gold", out value) ? Convert.ToInt64(value) : default(long);
            OwnedCurrency[Currency.Point] = _data.TryGetValue("Point", out value) ? Convert.ToInt64(value) : default(long);
            //註冊方式
            MyAuthType = AuthType.NotSigninYet;
            string authTypeStr = _data.TryGetValue("AuthType", out value) ? Convert.ToString(value) : AuthType.NotSigninYet.ToString();
            AuthType myAuthType;
            if (MyEnum.TryParseEnum(authTypeStr, out myAuthType))
                MyAuthType = myAuthType;
        }

    }

}