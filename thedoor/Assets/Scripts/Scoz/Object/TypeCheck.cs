using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scoz.Func
{
    public class TypeCheck : MonoBehaviour
    {
        public static bool IsList(object _obj)
        {
            if (_obj == null) return false;
            return _obj is IList &&
                   _obj.GetType().IsGenericType &&
                   _obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsDictionary(object _obj)
        {
            if (_obj == null) return false;
            return _obj is IDictionary &&
                   _obj.GetType().IsGenericType &&
                   _obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
    }
}