using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class AssetGet {


        public static void GetIconFromAtlas(string _dataName, string _spriteName, Action<Sprite> _ac) {

            if (string.IsNullOrEmpty(_dataName) || string.IsNullOrEmpty(_spriteName))
                _ac?.Invoke(null);
            AddressablesLoader.GetSpriteAtlas(_dataName + "Icon", atlas => {
                if (atlas != null) {
                    Sprite sprite = atlas.GetSprite(_spriteName);
                    _ac?.Invoke(sprite);
                }
            });
        }

    }
}