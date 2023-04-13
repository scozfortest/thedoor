using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Scoz.Func {
    public static class MyEnum {
        public static T ParseEnum<T>(string _value) where T : struct {
            if (Enum.TryParse(_value, out T t)) {
                return t;
            }
            WriteLog.LogErrorFormat("傳入字串:{0} 無法轉為 {1} Enum", _value, typeof(T));
            return default(T);
        }
        public static bool TryParseEnum<T>(string _value, out T _t) where T : struct {
            if (Enum.TryParse(_value, out T t)) {
                _t = t;
                return true;
            }
            _t = default(T);
            WriteLog.LogErrorFormat("傳入字串:{0} 無法轉為 {1} Enum", _value, typeof(T));
            return false;
        }
        public static bool IsTypeOfEnum<T>(string _value) where T : struct {
            if (Enum.TryParse(_value, out T t)) {
                return true;
            }
            return false;
        }
        public static int GetTypeCount<T>() where T : Enum {
            return Enum.GetValues(typeof(T)).Length;
        }
        public static T GetRandomValue<T>() {
            System.Random rand = new System.Random();
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(rand.Next(v.Length));
        }
        public static T[] GetArray<T>() where T : Enum {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray<T>();
        }
        public static List<T> GetList<T>() where T : Enum {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
        }
        public static string[] GetStrArray<T>() where T : Enum {
            return Array.ConvertAll<T, string>(Enum.GetValues(typeof(T)).Cast<T>().ToArray<T>(), a => a.ToString());
        }
        public static List<string> GetStrLIst<T>() where T : Enum {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>().ConvertAll(a => a.ToString());
        }
        public static HashSet<T> GetHashSet<T>() where T : Enum {
            return Enum.GetValues(typeof(T)).Cast<T>().ToHashSet<T>();
        }
        public static HashSet<string> GetStrHashSet<T>() where T : Enum {
            string[] strs = GetStrArray<T>();
            return new HashSet<string>(strs);
        }
        public static T GetValueByIndex<T>(int _index) where T : Enum {
            if (_index < 0 || _index >= typeof(T).GetEnumValues().Length)
                return default(T);
            T t = GetArray<T>()[_index];
            return t;
        }
        public static bool CheckEnumExistInArray<T>(T _check, params T[] _array) where T : struct, Enum {
            for (int i = 0; i < _array.Length; i++) {
                if (_array[i].ToString() == _check.ToString()) {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckEnumsExistInArray<T>(T[] _array, params T[] _check) where T : struct, Enum {
            for (int i = 0; i < _array.Length; i++) {
                for (int j = 0; j < _check.Length; j++) {
                    if (_array[i].ToString() == _check[j].ToString()) {
                        return true;
                    }

                }
            }
            return false;
        }
        public static bool CheckEnumExistInDicKeys<T, U>(Dictionary<T, U> _dic, params T[] _check) where T : Enum {
            T[] array = _dic.Keys.ToArray();
            for (int i = 0; i < array.Length; i++) {
                for (int j = 0; j < _check.Length; j++) {
                    if (array[i].ToString() == _check[j].ToString())
                        return true;
                }
            }
            return false;
        }
        public static Dictionary<T, U> GetEnumKeyDicWithDefaultU<T, U>(U _defaulU) where T : Enum {
            Dictionary<T, U> dic = new Dictionary<T, U>();
            T[] tArray = Enum.GetValues(typeof(T)).Cast<T>().ToArray<T>();
            for (int i = 0; i < tArray.Length; i++) {
                dic.Add(tArray[i], _defaulU);
            }
            return dic;
        }
        public static Dictionary<string, U> GetStrKeyDicWithDefaultU<T, U>(U _defaulU) where T : Enum {
            Dictionary<string, U> dic = new Dictionary<string, U>();
            T[] tArray = Enum.GetValues(typeof(T)).Cast<T>().ToArray<T>();
            for (int i = 0; i < tArray.Length; i++) {
                dic.Add(tArray[i].ToString(), _defaulU);
            }
            return dic;
        }
        public static T GetNext<T>(this T _enum) where T : Enum {
            T[] tArray = (T[])Enum.GetValues(_enum.GetType());
            int nextIndex = Array.IndexOf<T>(tArray, _enum) + 1;
            return (tArray.Length == nextIndex) ? tArray[0] : tArray[nextIndex];
        }
        public static T GetPrevious<T>(this T _enum) where T : Enum {
            T[] tArray = (T[])Enum.GetValues(_enum.GetType());
            int previousIndex = Array.IndexOf<T>(tArray, _enum) - 1;
            return (previousIndex < 0) ? tArray[tArray.Length - 1] : tArray[previousIndex];
        }
        public static T ToEnum<T>(this string _str, T _defaultEnum) where T : Enum {
            if (!Enum.IsDefined(typeof(T), _str))
                return _defaultEnum;
            return (T)Enum.Parse(typeof(T), _str);
        }


    }
}
