using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public enum BattleState {
        PlayerTurn,//玩家出牌階段
        ActionPerform,//出牌後行動表演階段
        End,//戰鬥結束
    }

    public class BattleManager : MonoBehaviour {
        public static PlayerRole PRole { get; private set; }
        public static EnemyRole ERole { get; private set; }
        public static BattleManager Instance { get; private set; }
        public static BattleState CurBattleState { get; private set; }

        static PlayerAction CurPlayerAction;//玩家要執行的行動




        public void Init() {
            Instance = this;
        }
        public static void ResetBattle(PlayerRole _pRole, int _monsterID) {
            PRole = _pRole;
            SetEnemyRole(_monsterID);
            TimelineBattleUI.Instance.ResetBattleUI(ERole.Actions);
            CurBattleState = BattleState.PlayerTurn;
        }
        static void SetEnemyRole(int _monsterID) {
            MonsterData mData = MonsterData.GetData(_monsterID);
            ERole = new EnemyRole.Builder()
                .SetData(mData)
                .SetMaxHP(mData.HP)
                .SetCurHP(mData.HP)
                .Build();
        }

        public static void PlayerDoAction(PlayerAction _action) {
            if (_action == null) return;
            CurBattleState = BattleState.ActionPerform;
            CurPlayerAction = _action;
            DoActionSchedule();
        }


        /// <summary>
        /// 1. 根據耗時依序將玩家與怪物行動加入排程 直到玩家行動已經被加入排程
        /// 2. 行動減去耗時 並 將耗時為0的行動加入排程
        /// 3. 從ERole.Actions中移除耗時為0的行動，並在TimelineBattleUI.Instance.PassTime結束階段會移除Time為0的ActionPrefab
        /// </summary>
        static void DoActionSchedule() {
            List<List<RoleAction>> scheduledActions = new List<List<RoleAction>>();//行動排程 List[x].Count=2代表玩家跟怪物在同個時間點行動(但玩家還是會先)
            for (int i = 0; i < ERole.Actions.Count; i++) {
                var eAction = ERole.Actions[i];
                if (eAction.RemainTime > CurPlayerAction.RemainTime) {//敵方行動耗時>玩家行動耗時: 玩家行動
                    eAction.ModifyRemainTime(eAction.RemainTime - CurPlayerAction.RemainTime);
                    CurPlayerAction.ModifyRemainTime(0);
                    scheduledActions.Add(new List<RoleAction>() { CurPlayerAction });
                } else if (eAction.RemainTime == CurPlayerAction.RemainTime) {//行動耗時相等時: 玩家先行動後再換敵方行動
                    CurPlayerAction.ModifyRemainTime(0);
                    eAction.ModifyRemainTime(0);
                    scheduledActions.Add(new List<RoleAction>() { CurPlayerAction, eAction });
                } else {//敵方行動耗時<玩家行動耗時: 敵方行動
                    CurPlayerAction.ModifyRemainTime(CurPlayerAction.RemainTime - eAction.RemainTime);
                    eAction.ModifyRemainTime(0);
                    scheduledActions.Add(new List<RoleAction>() { eAction });
                }
                if (CurPlayerAction.RemainTime <= 0)//玩家行動剩餘耗時為0代表已經加入行動排程 此時可以結束排程工作
                    break;
            }
            ERole.Actions.RemoveAll(a => a.RemainTime <= 0);//移除已經加入排程的行動

            DoActions(0, 0, scheduledActions);//執行行動

        }

        /// <summary>
        /// 執行該時間點的行動(可能是玩家行動、怪物行動 或 同時行動(玩家還是會先))
        /// </summary>
        static void DoActions(int _index, int _timePass, List<List<RoleAction>> _scheduledActions) {
            if (_scheduledActions == null || _scheduledActions.Count == 0) {
                OnActionFinish();
                return;
            }
            //排程行動都執行完就結束
            if (_index >= _scheduledActions.Count || _scheduledActions[_index] == null || _scheduledActions[_index].Count == 0) {
                OnActionFinish();
                return;
            }
            List<RoleAction> actions = _scheduledActions[_index];

            if (actions[0] is PlayerAction) {
                actions[0].DoAction();
                //玩家行動是先執行在跑時間軸動畫
                int pActionPassTime = actions[0].Time - _timePass;//玩家行動導致時間軸推進時 推進的時間=該行動耗時-玩家行動前已經因怪物行動而流逝的時間
                TimelineBattleUI.Instance.PassTime(pActionPassTime, () => {
                    if (actions.Count == 2) {//如果也有怪物行動
                        var newEAction = ERole.AddNewAction();
                        TimelineBattleUI.Instance.SpawnNewAction(newEAction);
                        actions[1].DoAction();
                        _timePass += actions[1].Time;
                    }
                    //進下一個時間軸
                    _index++;
                    DoActions(_index, _timePass, _scheduledActions);
                });
            } else if (actions[0] is EnemyAction) {
                var newEAction = ERole.AddNewAction();
                TimelineBattleUI.Instance.SpawnNewAction(newEAction);
                //怪物行動是先跑完時間軸動畫才執行
                TimelineBattleUI.Instance.PassTime(actions[0].Time, () => {
                    actions[0].DoAction();
                    _timePass += actions[0].Time;
                    //進下一個時間軸
                    _index++;
                    DoActions(_index, _timePass, _scheduledActions);
                });
            }
        }
        static void OnActionFinish() {
            CurBattleState = BattleState.PlayerTurn;
            if (PRole.IsDead || ERole.IsDead) {
                CurBattleState = BattleState.End;
                if (PRole.IsDead)
                    BattleUI.Instance.Lose();
                else if (ERole.IsDead)
                    BattleUI.Instance.Win();
                WriteLog.LogColor("戰鬥結束", WriteLog.LogType.Battle);
            }
        }
    }
}