using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;

namespace TheDoor.Main {
    public class SupplyPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;
        [SerializeField] TextMeshProUGUI Description;

        SupplyData MyData;

        public bool IsActive { get; set; }

        public void SetData(SupplyData _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            Description.text = MyData.Description;
            AssetGet.GetIconFromAtlas(SupplyData.DataName, MyData.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}