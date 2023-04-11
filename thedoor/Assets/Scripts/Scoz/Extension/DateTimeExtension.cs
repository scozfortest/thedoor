using System;
namespace Scoz.Func
{
    public static class MyDateTimeExtension
    {
        /// <summary>
        /// 將DateTime轉為yyyy-MM-dd-HH:mm:ss的字串格式(・ω・)
        /// </summary>
        public static string ToScozTimeStr(this DateTime _dateTime)
        {
            return _dateTime.ToString("yyyy-MM-dd-HH:mm:ss");
        }
    }
}