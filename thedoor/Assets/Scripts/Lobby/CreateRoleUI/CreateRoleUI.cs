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

    public class CreateRoleUI : BaseUI {

        [SerializeField] Image Img;
        [SerializeField] TextMeshProUGUI Content;

        OwnedRoleData MyOwnedRoleData;
        RoleData MyRoleData;
        RolePlotData CurRolePlotData;
        int CurPlotIndex;


        public void ShowUI(OwnedRoleData _ownedData) {
            MyRoleData = RoleData.GetData(_ownedData.ID);
            CurPlotIndex = 0;
            CurRolePlotData = RolePlotData.GetData(MyRoleData.ID, CurPlotIndex);
            if (CurRolePlotData == null) OnRolePlotEnd();
            RefreshUI();
            SetActive(true);
        }
        public override void RefreshUI() {
            base.RefreshUI();
            ShowContent();
        }


        void ShowContent() {
            if (CurRolePlotData == null) {
                Content.text = "";
                return;
            }
            Content.text = CurRolePlotData.Description;
        }
        public void Next() {
            CurPlotIndex++;
            CurRolePlotData = RolePlotData.GetData(MyRoleData.ID, CurPlotIndex);
            if (CurRolePlotData == null) {
                OnRolePlotEnd();
                return;
            }
            RefreshUI();
        }
        /// <summary>
        /// 腳色劇本跑完跑這裡
        /// </summary>
        public void OnRolePlotEnd() {

        }
    }
}