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

    public class RoleStateUI : BaseUI {
        [SerializeField] Image RoleImg;
        [SerializeField] Image HPImg;
        [SerializeField] Image SanPImg;
        [SerializeField] EffectSpawner BuffSpawner;
        [SerializeField] EffectSpawner DebuffSpawner;

        OwnedRoleData OwnedRoleData;
        RoleData MyRoleData;

        public override void Init() {
            base.Init();
            BuffSpawner.Init();
            DebuffSpawner.Init();
        }
        public void ShowUI(OwnedRoleData _ownedData) {
            OwnedRoleData = _ownedData;
            MyRoleData = RoleData.GetData(OwnedRoleData.ID);
            RefreshUI();
            SetActive(true);
        }

        public override void RefreshUI() {
            base.RefreshUI();
            AssetGet.GetImg(RoleData.DataName, MyRoleData.Ref, sprite => {
                RoleImg.sprite = sprite;
            });
            RefreshState();
            RefreshEffect();
        }

        public void RefreshState() {
            HPImg.fillAmount = (float)OwnedRoleData.CurHP / (float)MyRoleData.HP;
            SanPImg.fillAmount = (float)OwnedRoleData.CurSanP / (float)MyRoleData.SanP;
        }
        public void RefreshEffect() {
            List<TargetEffectData> effectDatas = OwnedRoleData.GetEffectDatas();
            if (effectDatas == null || effectDatas.Count == 0) {
                BuffSpawner.InActiveAllItem();
                DebuffSpawner.InActiveAllItem();
                return;
            }
            var buffs = effectDatas.FindAll(a => a.IsBuff);
            BuffSpawner.LoadItemAsset(() => {
                BuffSpawner.SpawnItems(buffs);
            });
            var debuffs = effectDatas.FindAll(a => !a.IsBuff);
            DebuffSpawner.LoadItemAsset(() => {
                DebuffSpawner.SpawnItems(debuffs);
            });
        }

        public void OnRoleInfoClick() {
            PopupUI.ShowRoleInfoUI(OwnedRoleData);
        }

    }
}
