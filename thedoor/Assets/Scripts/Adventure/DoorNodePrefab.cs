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
        [SerializeField] GameObject Indicator;
        [SerializeField] GameObject Cover;

        DoorData MyData;

        public bool IsActive { get; set; }

        public void SetData(DoorData _data, bool _showKnown, bool _showIndicator, bool _showCover) {
            MyData = _data;

            string spriteName = MyData.MyType.ToString();
            Indicator.SetActive(_showIndicator);
            Cover.SetActive(_showCover);
            if (_showKnown)
                spriteName = "Unknown";
            AssetGet.GetSpriteFromAtlas("AdventureUI", spriteName, sprite => {
                Icon.sprite = sprite;
                Icon.SetNativeSize();
            });
        }

    }
}