using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using System.Text;

namespace Scoz.Func {
    public class TextManager {

        /// <summary>
        /// 字串以字元分割轉int[]
        /// </summary>
        public static int[] StringSplitToIntArray(string _str, char _char) {
            int[] result;
            string[] resultStr = _str.Split(_char);
            result = System.Array.ConvertAll(resultStr, a => int.Parse(a));
            return result;
        }
        public static List<int> StringSplitToIntList(string _str, char _char) {
            return StringSplitToIntArray(_str, _char).ToList();
        }
        public static HashSet<int> StringSplitToIntHashSet(string _str, char _char) {
            return StringSplitToIntArray(_str, _char).ToHashSet();
        }

        /// <summary>
        /// int陣列轉字串並以字元分割
        /// </summary>
        /// <returns></returns>
        public static string IntArrayToStringSplitByChar(int[] _ints, char _char) {
            string result = "";
            for (int i = 0; i < _ints.Length; i++) {
                if (i != 0)
                    result += _char;
                result += _ints[i].ToString();
            }
            return result;
        }
        public static Int2D ParseTextToInt2D(string _text, char _char) {
            int[] values = new int[2] { 0, 0 };
            try {
                values = Array.ConvertAll(_text.Split(_char), a => int.Parse(a));
                if (values.Length != 2)
                    WriteLog.LogErrorFormat("Parse {0} to Vector2 失敗", _text);
            } catch {
                WriteLog.LogErrorFormat("Parse {0} to Vector2 失敗", _text);
            }
            Int2D int2D = new Int2D(values[0], values[1]);
            return int2D;
        }
        public static Vector3 ParseTextToVect3(string _text, char _char) {
            float[] values = new float[3] { 0, 0, 0 };
            try {
                values = Array.ConvertAll(_text.Split(_char), a => float.Parse(a));
                if (values.Length != 3)
                    WriteLog.LogErrorFormat("Parse {0} to Vector3 失敗", _text);
            } catch {
                WriteLog.LogErrorFormat("Parse {0} to Vector3 失敗", _text);
                return Vector3.zero;
            }
            Vector3 vector3 = new Vector3(values[0], values[1], values[2]);
            return vector3;
        }
        /// <summary>
        /// int List轉字串並以字元分割
        /// </summary>
        /// <returns></returns>
        public static string IntListToStringSplitByChar(List<int> _ints, char _char) {
            string result = "";
            for (int i = 0; i < _ints.Count; i++) {
                if (i != 0)
                    result += _char;
                result += _ints[i].ToString();
            }
            return result;
        }
        public static HashSet<T> GetEnumHashSetFromSplitStr<T>(string _s, char _char) where T : struct, Enum {
            HashSet<T> hashSet = new HashSet<T>();
            string[] strs = _s.Split(_char);
            for (int i = 0; i < strs.Length; i++) {
                T t = MyEnum.ParseEnum<T>(strs[i]);
                if (!hashSet.Contains(t))
                    hashSet.Add(t);
            }
            return hashSet;
        }
        public static HashSet<string> GetHashSetFromSplitStr(string _s, char _char) {
            HashSet<string> hashSet = new HashSet<string>();
            string[] strs = _s.Split(_char);
            for (int i = 0; i < strs.Length; i++) {
                string t = strs[i];
                if (!hashSet.Contains(t))
                    hashSet.Add(t);
            }
            return hashSet;
        }
        public static HashSet<int> GetIntHashSetFromSplitStr(string _s, char _char) {
            HashSet<int> hashSet = new HashSet<int>();
            string[] strs = _s.Split(_char);
            for (int i = 0; i < strs.Length; i++) {
                string t = strs[i];
                if (int.TryParse(t, out int value)) {
                    if (!hashSet.Contains(value))
                        hashSet.Add(value);
                }
            }
            return hashSet;
        }
        /// <summary>
        /// 字串以字元分割轉字串List
        /// </summary>
        public static string StringListToString(List<string> _list, char _char) {
            string result = "";
            for (int i = 0; i < _list.Count; i++) {
                if (result != "")
                    result += _char;
                result += _list[i];
            }
            return result;
        }
        /// <summary>
        /// 小數轉為百分比
        /// </summary>
        public static float FloatToPercent(float _value) {
            return _value * 100;
        }
        /// <summary>
        /// 傳入小數獲得機率文字 Ex. 0.1=>10%
        /// </summary>
        public static string FloatToPercentStr(float _value, int _decimal = 2) {
            return string.Format("{0}%", Math.Round(FloatToPercent(_value), _decimal));
        }
        public static int GetNthIndex(string s, char t, int n) {
            int count = 0;
            for (int i = 0; i < s.Length; i++) {
                if (s[i] == t) {
                    count++;
                    if (count == n) {
                        return i;
                    }
                }
            }
            return -1;
        }
        public static string AappendColorStr(string _str, Color _color) {
            string colorCode = ColorUtility.ToHtmlStringRGBA(_color);
            _str = string.Format("<color=#{0}>{1}</color>", colorCode, _str);
            return _str;
        }
        public static string AappendColorStr(string _str, string _colorCode) {
            _str = string.Format("<color=#{0}>{1}</color>", _colorCode, _str);
            return _str;
        }
        public static MinMax GetMinMax(string _str, char _splitChar) {
            int[] ints = Array.ConvertAll(_str.Split(_splitChar), a => int.Parse(a));
            return new MinMax(ints[0], ints[1]);
        }
        public static string GetScozTimeStr(DateTime _time) {
            return _time.ToString("dd/MM/yyyy HH:mm:ss");
        }
        public static DateTime GetDateTimeFormScozTimeStr(string _timeStr) {
            return DateTime.ParseExact(_timeStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }
        public static string GetTMPSpriteAssetText_Number(int _value) {
            List<char> chars = new List<char>(_value.ToString().ToCharArray());
            string resultStr = "";
            for (int i = 0; i < chars.Count; i++) {
                resultStr += "<sprite name=\"" + chars[i] + "\" tint=1>";
            }
            return resultStr;
        }
        public static string GetTMPSpriteAssetText_Str(string _value) {
            string resultStr = "<sprite name=\"" + _value + "\" tint=1>";
            return resultStr;
        }
        public static string GetColorText(string _value, string _colorColde) {
            string resultStr = "<color=#" + _colorColde + ">" + _value + "</color>";
            return resultStr;
        }

        public static string GetTransAddressStr(Transform _trans) {
            if (_trans == null)
                return "";
            string address = _trans.name;
            Transform parentTrans = _trans;
            parentTrans = parentTrans.parent;
            while (parentTrans != null) {
                address = string.Format("{0}/{1}", parentTrans.name, address);
                parentTrans = parentTrans.parent;
            }
            return address;
        }
        public static string GetHexStrFromByte(byte _byte, bool _withPrefix) {
            if (_withPrefix)
                return "0x" + Convert.ToByte(_byte).ToString("x2");
            else
                return Convert.ToByte(_byte).ToString("x2");
        }
        public static string ASCIIOctetsToString(byte[] bytes) {
            StringBuilder sb = new StringBuilder(bytes.Length);
            foreach (char c in bytes.Select(b => (char)b)) {
                switch (c) {
                    case '\u0000': sb.Append("<NUL>"); break;
                    case '\u0001': sb.Append("<SOH>"); break;
                    case '\u0002': sb.Append("<STX>"); break;
                    case '\u0003': sb.Append("<ETX>"); break;
                    case '\u0004': sb.Append("<EOT>"); break;
                    case '\u0005': sb.Append("<ENQ>"); break;
                    case '\u0006': sb.Append("<ACK>"); break;
                    case '\u0007': sb.Append("<BEL>"); break;
                    case '\u0008': sb.Append("<BS>"); break;
                    case '\u0009': sb.Append("<HT>"); break;
                    case '\u000A': sb.Append("<LF>"); break;
                    case '\u000B': sb.Append("<VT>"); break;
                    case '\u000C': sb.Append("<FF>"); break;
                    case '\u000D': sb.Append("<CR>"); break;
                    case '\u000E': sb.Append("<SO>"); break;
                    case '\u000F': sb.Append("<SI>"); break;
                    case '\u0010': sb.Append("<DLE>"); break;
                    case '\u0011': sb.Append("<DC1>"); break;
                    case '\u0012': sb.Append("<DC2>"); break;
                    case '\u0013': sb.Append("<DC3>"); break;
                    case '\u0014': sb.Append("<DC4>"); break;
                    case '\u0015': sb.Append("<NAK>"); break;
                    case '\u0016': sb.Append("<SYN>"); break;
                    case '\u0017': sb.Append("<ETB>"); break;
                    case '\u0018': sb.Append("<CAN>"); break;
                    case '\u0019': sb.Append("<EM>"); break;
                    case '\u001A': sb.Append("<SUB>"); break;
                    case '\u001B': sb.Append("<ESC>"); break;
                    case '\u001C': sb.Append("<FS>"); break;
                    case '\u001D': sb.Append("<GS>"); break;
                    case '\u001E': sb.Append("<RS>"); break;
                    case '\u001F': sb.Append("<US>"); break;
                    case '\u007F': sb.Append("<DEL>"); break;
                    default:
                        if (c > '\u007F') {
                            WriteLog.LogErrorFormat("{0} 為非可轉成ASCII的Char (in ASCII, any octet in the range 0x80-0xFF doesn't have a character glyph associated with it)", c);
                            sb.AppendFormat(@"\u{0:X4}", (ushort)c); // in ASCII, any octet in the range 0x80-0xFF doesn't have a character glyph associated with it
                        } else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
        public static string ByesToHexStr(byte[] bytes) {
            StringBuilder sb = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) {
                if (i != 0)
                    sb.Append(",");
                sb.Append(GetHexStrFromByte(bytes[i], false));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 比較兩個版本文字_aVersion與_bVersion，若_aVersion>=_bVersion就返回true
        /// a= 1.1 b=1.2 返回false
        /// a= 1.2 b=1.2 返回true
        /// a= 1.1.1 b=1.2 返回false
        /// a= 1.1 b=1.1.1 返回true
        /// a= 1.2 b=1.1 返回true
        /// a= 1.1.1 b=1.1 返回true
        /// a= 1.1.1 b=1.1.2 返回false
        /// a= 1.1.3 b=1.1.2 返回true
        /// </summary>
        public static bool AVersionGreaterEqualToBVersion(string _aVersion, string _bVersion) {
            int[] _aVersionNums = StringSplitToIntArray(_aVersion, '.');
            int[] _bVersionNums = StringSplitToIntArray(_bVersion, '.');
            for (int i = 0; i < _aVersionNums.Length; i++) {
                if (i < _bVersionNums.Length && _aVersionNums[i] < _bVersionNums[i])//前面數字小就是小
                    return false;
                else if (i < _bVersionNums.Length && _aVersionNums[i] > _bVersionNums[i])//前面數字大就是大
                    return true;
            }
            return true;
        }

        public static bool AVersionGreaterToBVersion(string _aVersion, string _bVersion) {
            int[] _aVersionNums = StringSplitToIntArray(_aVersion, '.');
            int[] _bVersionNums = StringSplitToIntArray(_bVersion, '.');
            for (int i = 0; i < _aVersionNums.Length; i++) {
                if (i < _bVersionNums.Length && _aVersionNums[i] < _bVersionNums[i])
                    return false;
                else if (i < _bVersionNums.Length && _aVersionNums[i] > _bVersionNums[i])
                    return true;
            }
            return false;
        }
    }
}
