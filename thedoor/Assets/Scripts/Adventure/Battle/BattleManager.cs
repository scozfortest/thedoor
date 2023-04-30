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
        public static int CurTime { get; private set; }//目前時間

        public void Init() {
            CurTime = 0;
            Instance = this;
        }
        public static void ResetBattle(PlayerRole _pRole, int _monsterID) {
            PRole = _pRole;
            SetEnemyRole(_monsterID);
            CurTime = 0;
        }
        static void SetEnemyRole(int _monsterID) {
            MonsterData mData = MonsterData.GetData(_monsterID);
            ERole = new EnemyRole.Builder()
                .SetData(mData)
                .SetMaxHP(mData.HP)
                .SetCurHP(mData.HP)
                .Build();
        }

        public static void PlayerDoAction(RoleAction _action) {
            Instance.StartCoroutine(Instance.ActionSchedule(_action));
        }
        IEnumerator ActionSchedule(RoleAction _action) {
            int remainTime = 0;
            List<RoleAction> scheduledActions = new List<RoleAction>();
            do {
                remainTime = ERole.Actions[0].Time - _action.Time;
                if (remainTime < 0) {
                    scheduledActions.Add(ERole.Actions[0]);
                    ERole.Actions.RemoveAt(0);
                } else {
                    scheduledActions.Add(_action);
                }
            } while (remainTime > 0);
            //敵方執行行動並補新的行為
            for (int i = 0; i < scheduledActions.Count; i++) {
                OnActionTrigger(scheduledActions[i]);
                yield return new WaitForSeconds(1f);
            }

        }
        void OnActionTrigger(RoleAction _action) {
            _action.DoAction();
            ERole.ScheduleActions();
        }
    }
}