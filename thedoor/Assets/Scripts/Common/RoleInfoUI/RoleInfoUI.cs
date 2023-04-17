using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using TMPro;

namespace TheDoor.Main {

    public class RoleInfoUI : BaseUI {

        [SerializeField] Image RoleImg;
        [SerializeField] TextMeshProUGUI HPText;
        [SerializeField] TextMeshProUGUI SanPText;

        OwnedRoleData MyData;


        public override void Init() {
            base.Init();
        }
        public void SetUI() {

        }
        public override void RefreshUI() {
            base.RefreshUI();
        }


    }
}