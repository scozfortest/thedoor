using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

namespace TheDoor.Main {
    public class DisplaySupplyPrefab : SupplyPrefab {

        bool Selected = false;

        public void SetData(SupplyData _data) {
            Selected = false;
            MySupplyData = _data;
            CardBG.material = null;
            Refresh();
        }
        public override void Refresh() {
            base.Refresh();
            Name.text = MySupplyData.Name;
            Description.text = MySupplyData.Description;
            CardBG.material = (Selected) ? OutlineMaterial : null;
        }
        public void SetSelect(bool _select) {
            Selected = _select;
            Refresh();
        }

        public void OnClick() {
            Selected = !Selected;
            if (Selected)
                SupplyChoiceUI.Instance?.Select(this);
            else
                SupplyChoiceUI.Instance?.UnSelect(this);
            Refresh();
        }
    }


}
