using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class SupplySpawner : ItemSpawner_Remote<ActionSupplyPrefab> {
        public override void Init() {
            base.Init();
        }

        public void SpawnItems(List<OwnedSupplyData> _ownedDatas, ActionSupplyPrefab.ActionSupplyType _type) {
            if (!LoadItemFinished) {
                WriteLog.LogError("SupplyPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_ownedDatas != null && _ownedDatas.Count > 0) {
                for (int i = 0; i < _ownedDatas.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_ownedDatas[i], _type);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(_ownedDatas[i], _type);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }
    }
}