using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public static class ListExtension
    {
        public static int GetFirstItemOrderIndex<T>(this List<T> _list, T _t)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_t.Equals(_list[i]))
                {
                    return i;
                }
            }
            DebugLogger.LogError("�ǤJ��item���Alist��");
            return 0;
        }
    }
}