using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
namespace TheDoor.Main {

    public static class IntExtension {
        public static object[] ToObjs(this int[] _ints) {
            if (_ints == null)
                return null;
            List<object> objs = new List<object>();
            for (int i = 0; i < _ints.Length; i++) {
                objs.Add(_ints[i]);
            }
            return objs.ToArray();
        }
    }
}
