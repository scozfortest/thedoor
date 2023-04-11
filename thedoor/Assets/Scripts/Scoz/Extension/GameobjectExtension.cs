using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func
{
    public static class GameobjectExtension
    {
        public static void SetSelfAndChildrenLayer(this GameObject _self, int _layer)
        {
            foreach (Transform trans in _self.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = _layer;
            }
        }
    }
}
