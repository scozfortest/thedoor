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

    public class BattleUI : BaseUI {


        [SerializeField] EnemyUI MyEnemyUI;
        [SerializeField] SupplySpawner MySupplySpawner;
        [SerializeField] DragIndicator MyDragIndicator;
        RoleStateUI MyRoleStateUI;

        public override void Init() {
            base.Init();
            MySupplySpawner.Init();
            MyRoleStateUI = RoleStateUI.GetInstance<RoleStateUI>();
            MyDragIndicator.Init();
        }

        public void ShowUI() {
            MyRoleStateUI.ShowUI(AdventureManager.PRole);
            MyEnemyUI.RefreshUI(AdventureManager.ERole);
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(GamePlayer.Instance.Data.CurRole.GetSupplyDatas());
            });
        }

        public void StartDrag(Transform _startTarget) {
            MyDragIndicator.StartDrag(_startTarget);
        }
        public void EndDrag() {
            MyDragIndicator.EndDrag();
        }

        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }

    }
}