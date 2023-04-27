using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {
    public class RoleActionPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;

        RoleAction MyData;

        public bool IsActive { get; set; }

        public void SetData(RoleAction _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            AssetGet.GetSpriteFromAtlas("RoleIcon", MyData.Doer.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}