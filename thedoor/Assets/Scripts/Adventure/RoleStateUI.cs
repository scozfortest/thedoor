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
        [SerializeField] TextMeshProUGUI HPText;
        [SerializeField] TextMeshProUGUI SanPText;
        public RectTransform DNPTrans;//跳血位置

        PlayerRole MyRole;
        public static RoleStateUI Instance { get; private set; }

        public override void Init() {
            base.Init();
            Instance = this;
            BuffSpawner.Init();
            BuffSpawner.LoadItemAsset();
            DebuffSpawner.Init();
            DebuffSpawner.LoadItemAsset();
        }
        public void ShowUI(PlayerRole _role) {
            MyRole = _role;
            RefreshUI();
            SetActive(true);
        }

        public override void RefreshUI() {
            base.RefreshUI();
            AssetGet.GetImg(RoleData.DataName, MyRole.MyData.Ref, sprite => {
                RoleImg.sprite = sprite;
            });
            RefreshState();
            RefreshEffect();
        }

        public void RefreshState() {
            HPText.text = string.Format("{0} / {1}", MyRole.CurHP, MyRole.MaxHP);
            SanPText.text = string.Format("{0} / {1}", MyRole.CurSanP, MyRole.MaxSanP);
            HPImg.fillAmount = MyRole.HPRatio;
            SanPImg.fillAmount = MyRole.SanPRatio;
        }
        public void RefreshEffect() {
            List<StatusEffect> effectDatas = MyRole.Effects.Values.ToList();
            if (effectDatas == null || effectDatas.Count == 0) {
                BuffSpawner.InActiveAllItem();
                DebuffSpawner.InActiveAllItem();
                return;
            }
            var buffs = effectDatas.FindAll(a => a.IsBuff);
            BuffSpawner.LoadItemAsset(() => {
                BuffSpawner.SpawnItems(buffs, Camera.main, Vector2.up * 270);
            });
            var debuffs = effectDatas.FindAll(a => !a.IsBuff);
            DebuffSpawner.LoadItemAsset(() => {
                DebuffSpawner.SpawnItems(debuffs, Camera.main, Vector2.up * 270);
            });
        }

        public void OnRoleInfoClick() {
            PopupUI.ShowRoleInfoUI(GamePlayer.Instance.Data.CurRole, MyRole);
        }

    }
}
