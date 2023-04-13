using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public class MyCompare
    {
        /// <summary>
        /// 兩個陣列中有任一相等返回true
        /// </summary>
        public static bool OneOfArrayAEqualToArrayB<T>(T[] _array, params T[] _check) where T : class
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _check.Length; j++)
                {
                    if (_array[i] == _check[j])
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 陣列內容均相等
        /// </summary>
        public static bool AllAreEqual<T>(params T[] _args)
        {
            if (_args != null && _args.Length > 1)
            {
                for (int i = 1; i < _args.Length; i++)
                {
                    if (!_args[i].Equals(_args[i - 1])) return false;
                }
            }
            else
                WriteLog.LogWarning("傳入要比較的參數為null或數量為0");
            return true;
        }
        /// <summary>
        /// A與另一個陣列內所有內容都相等
        /// </summary>
        public static bool EqualToAll<T>(T _beComparedArg, params T[] _compareArgs)
        {
            if (_compareArgs != null && _compareArgs.Length > 0)
            {
                for (int i = 0; i < _compareArgs.Length; i++)
                {
                    if (!_beComparedArg.Equals(_compareArgs[i])) return false;
                }
            }
            else
                WriteLog.LogWarning("傳入要比較的參數為null或數量為0");
            return true;
        }
        /// <summary>
        /// A與另一個陣列其中一項內容相等
        /// </summary>
        public static bool EqualToOneOfAll<T>(T _beComparedArg, params T[] _compareArgs)
        {
            if (_compareArgs != null && _compareArgs.Length > 0)
            {
                for (int i = 0; i < _compareArgs.Length; i++)
                {
                    if (_beComparedArg.Equals(_compareArgs[i])) return true;
                }
            }
            return false;
        }
    }
}
