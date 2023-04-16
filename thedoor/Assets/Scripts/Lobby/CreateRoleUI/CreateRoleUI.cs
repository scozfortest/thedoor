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

        RoleData MyRoleData;
        RolePlotData CurRolePlotData;
        int CurPlotIndex;


        public void SetRole(int _id) {
            MyRoleData = RoleData.GetData(_id);
            CurPlotIndex = 0;
            CurRolePlotData = RolePlotData.GetData(MyRoleData.ID, CurPlotIndex);
            RefreshUI();
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
                return;
            }
            RefreshUI();
        }
    }
}