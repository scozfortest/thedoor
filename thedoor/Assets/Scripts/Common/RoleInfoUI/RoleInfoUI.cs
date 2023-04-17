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
        [SerializeField] TalentSpawner MyTalentSpawner;
        [SerializeField] EffectSpawner MyEffectSpawner;
        [SerializeField] SupplySpawner MySupplySpawner;


        OwnedRoleData OwnedData;
        RoleData MyRoleData;


        public override void Init() {
            base.Init();
            MySupplySpawner.Init();
            MyEffectSpawner.Init();
            MySupplySpawner.Init();
        }
        public void ShowUI(OwnedRoleData _ownedData) {
            OwnedData = _ownedData;
            MyRoleData = RoleData.GetData(OwnedData.RoleID);
            RefreshUI();
            RefreshSupply();
            SetActive(true);
        }
        public override void RefreshUI() {
            base.RefreshUI();
            HPText.text = string.Format("HP:{0}/{1}", OwnedData.CurHP, MyRoleData.HP);
            SanPText.text = string.Format("SanP:{0}/{1}", OwnedData.CurSanP, MyRoleData.SanP);
            AssetGet.GetImg(RoleData.DataName, MyRoleData.Ref, sprite => {
                RoleImg.sprite = sprite;
            });
        }

        public void RefreshSupply() {
            List<OwnedSupplyData> supplyDatas = GamePlayer.Instance.GetOwnedDatas<OwnedSupplyData>(ColEnum.Supply);
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(supplyDatas);
            });
        }
        public void RefreshTalent() {

        }
        public void RefreshEffect() {

        }

    }
}