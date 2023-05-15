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
        [SerializeField] protected TextMeshProUGUI Name;
        [SerializeField] protected TextMeshProUGUI Description;
        [SerializeField] protected TextMeshProUGUI Usage;
        [SerializeField] protected TextMeshProUGUI Time;
        [SerializeField] protected Material OutlineMaterial;
        [SerializeField] Sprite BattleBG;
        [SerializeField] Sprite OtherBG;

        public SupplyData MySupplyData { get; protected set; }

        public bool IsActive { get; set; }


        public virtual void Refresh() {
            if (MySupplyData.ContainTiming(SupplyData.Timing.Battle)) CardBG.sprite = BattleBG;
            else CardBG.sprite = OtherBG;
            Time.gameObject.SetActive(!MySupplyData.BelongTiming(SupplyData.Timing.None));
            Time.text = MySupplyData.Time.ToString();
            AssetGet.GetSpriteFromAtlas(SupplyData.DataName + "Icon", MySupplyData.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }


    }


}
