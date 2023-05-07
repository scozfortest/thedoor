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

    public class EnemyUI : BaseUI {
        [SerializeField] Image EnemyImg;
        [SerializeField] Image HP;
        [SerializeField] List<Image> HitGOs;//頭、身、四肢
        [SerializeField] Material OutlineMaterial;
        public RectTransform DNPTrans;//跳血位置

        public static EnemyUI Instance { get; private set; }
        EnemyRole ERole;

        public override void Init() {
            base.Init();
            Instance = this;
        }

        public void SetRole(EnemyRole _enemyRole) {
            ERole = _enemyRole;
        }
        public override void RefreshUI() {
            base.RefreshUI();
            SetHP(ERole.HPRatio);
            AssetGet.GetImg("Monster", ERole.MyData.Ref, sprite => {
                EnemyImg.sprite = sprite;
            });


            for (int i = 0; i < HitGOs.Count; i++) {
                HitGOs[i].gameObject.SetActive(ERole.MyData.GetAttackPartTuple((AttackPart)i) != null);
            }
        }

        public void SetHP(float _hp) {
            HP.fillAmount = _hp;
        }

        public void ShowOutlineMaterial(bool _show) {
            for (int i = 0; i < HitGOs.Count; i++) {
                HitGOs[i].material = (_show) ? OutlineMaterial : null;
            }
        }
    }
}
