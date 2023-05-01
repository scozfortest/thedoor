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
        [SerializeField] TimelineBattleUI MyTimelineBattleUI;
        RoleStateUI MyRoleStateUI;

        public static BattleUI Instance { get; private set; }

        public override void Init() {
            base.Init();
            Instance = this;
            MySupplySpawner.Init();
            MyRoleStateUI = RoleStateUI.Instance;
            MyDragIndicator.Init();
            MyEnemyUI.Init();
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(GamePlayer.Instance.Data.CurRole.GetSupplyDatas());
            });
            MyTimelineBattleUI.Init();
            MyTimelineBattleUI.LoadItemAsset();
        }

        public void ShowUI() {
            MyRoleStateUI.ShowUI(BattleManager.PRole);
            MyEnemyUI.SetRole(BattleManager.ERole);
            MyEnemyUI.RefreshUI();
            RefreshSupplyUI();
        }
        public void RefreshSupplyUI() {
            MySupplySpawner.SpawnItems(GamePlayer.Instance.Data.CurRole.GetSupplyDatas());
        }
        public void StartDrag(Transform _startTarget, Action<string> _cb) {
            MyDragIndicator.StartDrag(_startTarget, _cb);
            MyEnemyUI.ShowOutlineMaterial(true);
        }
        public void EndDrag() {
            MyDragIndicator.EndDrag();
            MyEnemyUI.ShowOutlineMaterial(false);
        }

        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }

    }
}