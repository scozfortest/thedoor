using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;

namespace Scoz.Func
{
    public class TimeConvert
    {        public static DateTime UnixTimeStampToDateTime(double _unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(_unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

}