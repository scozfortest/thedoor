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
        [SerializeField] TextMeshProUGUI HPText;
        [SerializeField] TextMeshProUGUI SanPText;

        PlayerRole PRole;
        RoleData MyRoleData;
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
            MyRoleData = RoleData.GetData(OwnedRoleData.ID);
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
            if (PRole != null) {
                HPText.text = string.Format("{0} / {1}", PRole.CurHP, PRole.MaxHP);
                SanPText.text = string.Format("{0} / {1}", PRole.CurSanP, PRole.MaxSanP);
            } else {
                HPText.text = string.Format("{0} / {1}", MyRoleData.HP, MyRoleData.HP);
                SanPText.text = string.Format("{0} / {1}", MyRoleData.SanP, MyRoleData.SanP);
            }

            RoleImg.gameObject.SetActive(false);
            AssetGet.GetImg(RoleData.DataName, MyRoleData.Ref, sprite => {
                RoleImg.sprite = sprite;
                RoleImg.gameObject.SetActive(true);
            });
        }

        public void RefreshSupply() {
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(OwnedRoleData.GetSupplyDatas(null), ActionSupplyPrefab.ActionSupplyType.Info);
            });
        }
        public void RefreshTalent() {
            MyTalentSpawner.LoadItemAsset(() => {
                MyTalentSpawner.SpawnItems(PRole.Talents);
            });
        }
        public void RefreshEffect() {

            MyEffectSpawner.LoadItemAsset(() => {
                if (PRole == null)
                    MyEffectSpawner.SpawnItems(null, PopupUI.Instance.MyCam, Vector2.left * 270);
                else
                    MyEffectSpawner.SpawnItems(PRole.Effects.Values.ToList(), PopupUI.Instance.MyCam, Vector2.left * 270);
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