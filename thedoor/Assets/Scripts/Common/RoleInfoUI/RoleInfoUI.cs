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
        [SerializeField] GameObject GoAdventureBtn;


        OwnedRoleData OwnedRoleData;
        RoleData MyRoleData;


        public override void Init() {
            base.Init();
            MySupplySpawner.Init();
            MyEffectSpawner.Init();
            MySupplySpawner.Init();
        }
        public void ShowUI(OwnedRoleData _ownedData, bool _showGoAdventureBtn = false) {
            OwnedRoleData = _ownedData;
            MyRoleData = RoleData.GetData(OwnedRoleData.ID);
            GoAdventureBtn.SetActive(_showGoAdventureBtn);
            RefreshUI();
            RefreshSupply();
            RefreshTalent();
            RefreshEffect();
            SetActive(true);
        }
        public override void RefreshUI() {
            base.RefreshUI();
            HPText.text = string.Format("HP:{0}/{1}", OwnedRoleData.CurHP, MyRoleData.HP);
            SanPText.text = string.Format("SanP:{0}/{1}", OwnedRoleData.CurSanP, MyRoleData.SanP);
            AssetGet.GetImg(RoleData.DataName, MyRoleData.Ref, sprite => {
                RoleImg.sprite = sprite;
            });
        }

        public void RefreshSupply() {
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(OwnedRoleData.GetSupplyDatas());
            });
        }
        public void RefreshTalent() {
            List<TalentData> talentDatas = OwnedRoleData.GetTalentDatas();
            MyTalentSpawner.LoadItemAsset(() => {
                MyTalentSpawner.SpawnItems(talentDatas);
            });
        }
        public void RefreshEffect() {
            List<TargetEffectData> effectDatas = OwnedRoleData.GetEffectDatas();
            MyEffectSpawner.LoadItemAsset(() => {
                MyEffectSpawner.SpawnItems(effectDatas);
            });
        }
        public void GoAdventure() {
            PopupUI.InitSceneTransitionProgress(1, "AdventureUILoaded");
            PopupUI.CallSceneTransition(MyScene.AdventureScene, () => {
                SetActive(false);
            });
        }

    }
}