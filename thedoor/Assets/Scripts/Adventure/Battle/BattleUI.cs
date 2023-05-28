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
        [SerializeField] TimelineBattleUI MyTimelineBattleUI;
        [SerializeField] Animator WinAni;
        [SerializeField] Animator LoseAni;


        public static BattleUI Instance { get; private set; }
        Action BattleEndAC;

        public override void Init() {
            base.Init();
            Instance = this;
            MySupplySpawner.Init();
            MyEnemyUI.Init();
            MySupplySpawner.LoadItemAsset(() => {
                RefreshSupplyUI();
            });
            MyTimelineBattleUI.Init();
            MyTimelineBattleUI.LoadItemAsset();
        }

        public void ShowUI() {
            LoseAni.gameObject.SetActive(false);
            WinAni.gameObject.SetActive(false);
            RoleStateUI.Instance.ShowUI(BattleManager.PRole);
            MyEnemyUI.SetRole(BattleManager.ERole);
            MyEnemyUI.RefreshUI();

            RefreshSupplyUI();
        }
        public void RefreshSupplyUI() {
            MySupplySpawner.SpawnItems(GamePlayer.Instance.Data.CurRole.GetSupplyDatas(null, SupplyData.Timing.Battle), ActionSupplyPrefab.ActionSupplyType.Usable);
        }


        public void Win(Action _ac) {
            WriteLog.LogColor("玩家勝利", WriteLog.LogType.Battle);
            WinAni.gameObject.SetActive(true);
            WinAni.SetTrigger("Play");
            BattleEndAC = _ac;
        }
        public void Lose(Action _ac) {
            //WriteLog.LogColor("玩家戰敗", WriteLog.LogType.Battle);
            //LoseAni.gameObject.SetActive(true);
            //LoseAni.SetTrigger("Play");
            BattleEndAC = _ac;
        }
        public void OnWinLoseAniEnd() {
            LoseAni.gameObject.SetActive(false);
            WinAni.gameObject.SetActive(false);
            BattleEndAC?.Invoke();
        }

        public static RectTransform GetTargetRectTrans(Role _role) {
            if (RoleStateUI.Instance == null || EnemyUI.Instance == null) return null;
            RectTransform targetRectTrans = null;
            if (_role is PlayerRole)
                targetRectTrans = RoleStateUI.Instance.DNPTrans;
            else if (_role is EnemyRole)
                targetRectTrans = EnemyUI.Instance.DNPTrans;
            return targetRectTrans;
        }

    }
}