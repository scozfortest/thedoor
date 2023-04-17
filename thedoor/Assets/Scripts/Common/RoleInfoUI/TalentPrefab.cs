using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;

namespace TheDoor.Main {
    public class TalentPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;
        [SerializeField] TextMeshProUGUI Description;

        TalentData MyData;

        public bool IsActive { get; set; }

        public void SetData(TalentData _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            AssetGet.GetIconFromAtlas(TalentData.DataName, MyData.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }

    }
}