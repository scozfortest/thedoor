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
        [SerializeField] Animator WinAni;
        [SerializeField] Animator LoseAni;


        public static BattleUI Instance { get; private set; }

        public override void Init() {
            base.Init();
            Instance = this;
            MySupplySpawner.Init();
            MyDragIndicator.Init();
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
        public void StartDrag(PlayerAction _action, Transform _startTarget, Action<string> _cb) {
            MyDragIndicator.StartDrag(_startTarget, _cb, () => { TimelineBattleUI.Instance.RemovePlayerToken(); });
            MyEnemyUI.ShowOutlineMaterial(true);
            TimelineBattleUI.Instance.ShowPlayerToken(_action);
        }
        public void EndDrag() {
            MyDragIndicator.EndDrag();
            MyEnemyUI.ShowOutlineMaterial(false);
        }

        public void Win() {
            WriteLog.LogColor("玩家勝利", WriteLog.LogType.Battle);
            WinAni.gameObject.SetActive(true);
            WinAni.SetTrigger("Play");
        }
        public void Lose() {
            WriteLog.LogColor("玩家戰敗", WriteLog.LogType.Battle);
            LoseAni.gameObject.SetActive(true);
            LoseAni.SetTrigger("Play");
        }
        public void OnWinLoseAniEnd() {
            LoseAni.gameObject.SetActive(false);
            WinAni.gameObject.SetActive(false);
            if (!AdventureManager.PRole.IsDead)
                AdventureManager.GoNextDoor();
            else
                AdventureManager.GameOver();
        }

        public static RectTransform GetTargetRectTrans(Role _role) {
            RectTransform targetRectTrans = null;
            if (_role is PlayerRole)
                targetRectTrans = RoleStateUI.Instance.DNPTrans;
            else if (_role is EnemyRole)
                targetRectTrans = EnemyUI.Instance.DNPTrans;
            return targetRectTrans;
        }

    }
}