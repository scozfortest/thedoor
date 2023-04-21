using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {
    public class EffectPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;


        TargetEffectData MyData;

        public bool IsActive { get; set; }

        public void SetData(TargetEffectData _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            AssetGet.GetSpriteFromAtlas("EffectIcon", MyData.EffectType.ToString(), sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}