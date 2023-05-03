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
        [SerializeField] Image HP;
        [SerializeField] Image SanP;
        [SerializeField] TalentSpawner MyTalentSpawner;
        [SerializeField] EffectSpawner MyEffectSpawner;
        [SerializeField] SupplySpawner MySupplySpawner;
        [SerializeField] GameObject GoAdventureBtn;

        PlayerRole PRole;
        OwnedRoleData OwnedRoleData;
        public static RoleInfoUI Instance { get; private set; }



        public override void Init() {
            base.Init();
            Instance = this;
            MyTalentSpawner.Init();
            MyEffectSpawner.Init();
            MySupplySpawner.Init();
        }
        public void ShowUI(OwnedRoleData _ownedData, PlayerRole _pRole, bool _showGoAdventureBtn = false) {
            OwnedRoleData = _ownedData;
            PRole = _pRole;
            GoAdventureBtn.SetActive(_showGoAdventureBtn);
            RefreshUI();
            RefreshSupply();
            RefreshTalent();
            RefreshEffect();
            SetActive(true);
        }
        public override void RefreshUI() {
            base.RefreshUI();
            if (PRole == null) HP.fillAmount = 1;
            else HP.fillAmount = PRole.HPRatio;
            if (PRole == null) SanP.fillAmount = 1;
            else SanP.fillAmount = PRole.SanPRatio;
            AssetGet.GetImg(RoleData.DataName, PRole.Ref, sprite => {
                RoleImg.sprite = sprite;
            });
        }

        public void RefreshSupply() {
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(OwnedRoleData.GetSupplyDatas(true));
            });
        }
        public void RefreshTalent() {
            List<TalentData> talentDatas = OwnedRoleData.GetTalentDatas();
            MyTalentSpawner.LoadItemAsset(() => {
                MyTalentSpawner.SpawnItems(talentDatas);
            });
        }
        public void RefreshEffect() {

            MyEffectSpawner.LoadItemAsset(() => {
                if (PRole == null)
                    MyEffectSpawner.SpawnItems(null);
                else
                    MyEffectSpawner.SpawnItems(PRole.Effects.Values.ToList());
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