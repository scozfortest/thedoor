using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public class TimelineBattleUI : ItemSpawner_Remote<RoleActionPrefab> {

        private PlayerRole PRole;
        private EnemyRole ERole;
        private int CurTime;

        public TimelineBattleUI(PlayerRole _pRole, EnemyRole _eRole) {
            PRole = _pRole;
            ERole = _eRole;
            CurTime = 0;
        }
        public override void Init() {
            base.Init();
        }

        public void SpawnItems() {
            if (!LoadItemFinished) {
                WriteLog.LogError("RoleActionPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            var actions = ERole.Actions;
            if (actions != null && actions.Count > 0) {
                for (int i = 0; i < actions.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(actions[i]);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(actions[i]);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }
        public void PlayerDoAction(RoleAction _action) {
            int remainTime = ERole.Actions[0].Time - CurTime;
            if (_action.Time < remainTime) {
                _action.DoAction();
            } else {
                ERole.Actions[0].DoAction();
            }
        }
    }
}