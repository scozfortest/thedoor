using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

namespace TheDoor.Main {
    public class SupplyPrefab : MonoBehaviour, IItem {
        [SerializeField] protected Image Icon;
        [SerializeField] protected Image CardBG;
        [SerializeField] protected TextMeshProUGUI Description;
        [SerializeField] protected TextMeshProUGUI Usage;
        [SerializeField] protected TextMeshProUGUI Time;
        [SerializeField] protected Material OutlineMaterial;

        public SupplyData MySupplyData { get; protected set; }

        public bool IsActive { get; set; }


        public virtual void Refresh() {
            Description.text = MySupplyData.Description;
            Usage.text = MySupplyData.Usage.ToString();
            Time.text = MySupplyData.Time.ToString();
            AssetGet.GetSpriteFromAtlas(SupplyData.DataName + "Icon", MySupplyData.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }


    }


}
