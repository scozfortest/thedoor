using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public static class NumberExtension
    {
        /// <summary>
        /// ½T»{byte½d³ò return (_value >= _min && _value <= _max);
        /// </summary>
        public static bool InRange(this byte _value, int _min, int _max)
        {
            return (_value >= _min && _value <= _max);
        }
        /// <summary>
        /// ½T»{int½d³ò return (_value >= _min && _value <= _max);
        /// </summary>
        public static bool InRange(this int _value, int _min, int _max)
        {
            return (_value >= _min && _value <= _max);
        }
    }
}