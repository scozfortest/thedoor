using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {
    public class EffectPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;


        StatusEffect MyData;

        public bool IsActive { get; set; }

        public void SetData(StatusEffect _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            AssetGet.GetSpriteFromAtlas("EffectIcon", MyData.MyType.ToString(), sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}