using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public class BattleManager : MonoBehaviour {
        public static PlayerRole PRole { get; private set; }
        public static EnemyRole ERole { get; private set; }
        public static BattleManager Instance { get; private set; }
        static PlayerAction CurPlayerAction;

        public void Init() {
            Instance = this;
        }
        public static void ResetBattle(PlayerRole _pRole, int _monsterID) {
            PRole = _pRole;
            SetEnemyRole(_monsterID);
            TimelineBattleUI.Instance.UpdateTimeline(ERole.Actions);
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
            CurPlayerAction = _action;
            DoActionSchedule();
        }
        static void DoActionSchedule() {

            RoleAction nextERoleAction = ERole.Actions[0];
            if (nextERoleAction.Time < CurPlayerAction.Time) {//敵方攻擊
                ERole.Actions.RemoveAt(0);
                OnActionTrigger(nextERoleAction);
            } else {//玩家攻擊
                nextERoleAction.ModifyTime(nextERoleAction.Time - CurPlayerAction.Time);
                OnActionTrigger(CurPlayerAction);
            }

        }
        static void OnActionTrigger(RoleAction _action) {
            _action.DoAction();
            if (_action is PlayerAction) {
                CurPlayerAction = null;
            } else if (_action is EnemyAction) {
                ERole.ScheduleActions();
            }
            TimelineBattleUI.Instance.UpdateTimeline(ERole.Actions);
            if (CurPlayerAction != null)
                DoActionSchedule();
        }
    }
}