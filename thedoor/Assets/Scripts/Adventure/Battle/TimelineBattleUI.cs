using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;

namespace TheDoor.Main {
    public class TimelineBattleUI : ItemSpawner_Remote<RoleActionPrefab> {

        [SerializeField] int BaseUnit = 100;

        public static TimelineBattleUI Instance { get; private set; }
        public override void Init() {
            base.Init();
            Instance = this;
        }
        public void UpdateTimeline(List<EnemyAction> _actions) {
            SpawnItems(_actions);
        }

        void SpawnItems(List<EnemyAction> _actions) {
            if (!LoadItemFinished) {
                WriteLog.LogError("RoleActionPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            int curTime = 0;
            if (_actions != null && _actions.Count > 0) {
                for (int i = 0; i < _actions.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_actions[i]);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(_actions[i]);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                    //設定位置
                    var rectTrans = ItemList[i].GetComponent<RectTransform>();
                    int posY = curTime - _actions[i].Time * BaseUnit;
                    rectTrans.anchoredPosition = new Vector2(0, posY);
                    curTime = posY;
                }
            }
        }

    }
}