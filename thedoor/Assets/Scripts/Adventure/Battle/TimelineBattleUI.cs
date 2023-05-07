using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using DG.Tweening;

namespace TheDoor.Main {
    public class TimelineBattleUI : ItemSpawner_Remote<RoleActionPrefab> {

        [SerializeField] int BaseUnit = 100;

        int CurTime = 0;

        public static TimelineBattleUI Instance { get; private set; }
        public override void Init() {
            base.Init();
            Instance = this;
        }

        public void ResetBattleUI(List<EnemyAction> _actions) {
            CurTime = 0;
            if (!LoadItemFinished) {
                WriteLog.LogError("RoleActionPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
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
                    int posY = CurTime - _actions[i].Time * BaseUnit;
                    rectTrans.anchoredPosition = new Vector2(0, posY);
                    CurTime = posY;
                }
            }
        }

        public void SpawnNewAction(EnemyAction _action) {
            var item = Spawn();
            item.SetData(_action);
            item.transform.SetParent(ParentTrans);
            //設定位置
            var rectTrans = item.GetComponent<RectTransform>();
            int posY = CurTime - _action.Time * BaseUnit;
            rectTrans.anchoredPosition = new Vector2(0, posY);
            CurTime = posY;
        }

        /// <summary>
        /// 將已經執行的ActionPrefab從清單中移除並刪除物件
        /// </summary>
        public void RemoveOldActions() {
            if (ItemList == null || ItemList.Count == 0) return;
            List<int> removeIndex = new List<int>();
            for (int i = 0; i < ItemList.Count; i++) {
                if (ItemList[i].MyData.Done)
                    removeIndex.Add(i);
            }
            for (int i = removeIndex.Count - 1; i >= 0; i--) {
                Destroy(ItemList[i].gameObject);
                ItemList.RemoveAt(removeIndex[i]);
            }
        }
        public void PassTime(int _passTime, Action _ac) {
            CurTime += _passTime;
            for (int i = 0; i < ItemList.Count; i++) {
                var rectTrans = ItemList[i].GetComponent<RectTransform>();
                float targetPosY = rectTrans.anchoredPosition.y + _passTime * BaseUnit;
                var doTween = rectTrans.DOAnchorPosY(targetPosY, 0.5f, true);
                //最後一個Item移動到位後callback
                if (i == ItemList.Count - 1) {
                    doTween.OnComplete(() => {//完成動畫時執行
                        RemoveOldActions();
                        _ac?.Invoke();
                    });
                }
            }

        }

    }
}