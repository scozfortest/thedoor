using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class SupplySpawner : ItemSpawner_Remote<SupplyPrefab> {
        public override void Init() {
            base.Init();
        }

        public void SpawnItems(List<SupplyData> _SupplyDatas) {
            if (!LoadItemFinished) {
                WriteLog.LogError("SupplyPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_SupplyDatas != null && _SupplyDatas.Count > 0) {
                for (int i = 0; i < _SupplyDatas.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_SupplyDatas[i]);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(null);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }
    }
}