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
        [SerializeField] TextMeshProUGUI Usage;
        [SerializeField] TextMeshProUGUI Time;

        OwnedSupplyData OwnedData;

        public bool IsActive { get; set; }

        public void SetData(OwnedSupplyData _data) {
            OwnedData = _data;
            Refresh();
        }
        public void Refresh() {
            var data = SupplyData.GetData(OwnedData.ID);
            Description.text = data.Description;
            Usage.text = OwnedData.Usage.ToString();
            Time.text = data.Time.ToString();
            AssetGet.GetSpriteFromAtlas(SupplyData.DataName + "Icon", data.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}