using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;

namespace TheDoor.Main {
    public class DoorNodePrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;

        DoorData MyData;

        public bool IsActive { get; set; }

        public void SetData(DoorData _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            AssetGet.GetIconFromAtlas("AdventureUI", MyData.MyType.ToString(), sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}