using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Scoz.Func
{
    public static class StringExtention
    {
        public static string ReplaceFirstOccuranceOfSubStrInStr(this string _str,string _subStr,string _replaceStr)
        {
            Regex regReplace = new Regex(_subStr);
            string res = regReplace.Replace(_str, _replaceStr, 1);
            return res;
        }

        /// <summary>
        /// 從yyyy-MM-dd-HH:mm:ss的字串格式轉為TimeDate(づ￣ 3￣)づ
        /// </summary>
        public static DateTime GetTimeDateFromScozTimeStr(this string _timeStr)
        {
            return DateTime.ParseExact(_timeStr, "yyyy-MM-dd-HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string ToMD5(this string _str)
        {
            return MD5.GetMD5(_str);
        }
        /// <summary>
        /// 等於其中之一就返回true
        /// </summary>
        public static bool EqualToAny(this string _str,params string[] _strs)
        {
            for(int i=0;i<_strs.Length;i++)
            {
                if (_str == _strs[i])
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 不等於傳入的任何字串就返回true
        /// </summary>
        public static bool NotEqualToAll(this string _str, params string[] _strs)
        {
            for (int i = 0; i < _strs.Length; i++)
            {
                if (_str == _strs[i])
                    return false;
            }
            return true;
        }

    }
}
