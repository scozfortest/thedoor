using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheDoor.Main {
    public class AssetGet {


        public static void GetSpriteFromAtlas(string _atlasName, string _spriteName, Action<Sprite> _ac) {

            if (string.IsNullOrEmpty(_atlasName) || string.IsNullOrEmpty(_spriteName))
                _ac?.Invoke(null);
            AddressablesLoader.GetSpriteAtlas(_atlasName, atlas => {
                if (atlas != null) {
                    Sprite sprite = atlas.GetSprite(_spriteName);
                    _ac?.Invoke(sprite);
                }
            });
        }
        public static void GetImg(string _path, string _spriteName, Action<Sprite> _ac) {

            if (string.IsNullOrEmpty(_path) || string.IsNullOrEmpty(_spriteName))
                _ac?.Invoke(null);
            _path += "/" + _spriteName;
            AddressablesLoader.GetSprite(_path, (sprite, handle) => {
                _ac?.Invoke(sprite);
            });
        }

    }
}