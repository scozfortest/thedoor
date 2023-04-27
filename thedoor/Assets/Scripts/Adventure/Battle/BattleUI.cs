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
        RoleStateUI MyRoleStateUI;

        public override void Init() {
            base.Init();
            MySupplySpawner.Init();
            MyRoleStateUI = RoleStateUI.GetInstance<RoleStateUI>();
        }

        public void ShowUI() {
            MyRoleStateUI.ShowUI(AdventureManager.PRole);
            MyEnemyUI.RefreshUI(AdventureManager.ERole);
            MySupplySpawner.LoadItemAsset(() => {
                MySupplySpawner.SpawnItems(GamePlayer.Instance.Data.CurRole.GetSupplyDatas());
            });
        }



        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }

    }
}