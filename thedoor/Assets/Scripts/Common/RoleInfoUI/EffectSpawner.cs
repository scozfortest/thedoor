using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class EffectSpawner : ItemSpawner_Remote<EffectPrefab> {
        public override void Init() {
            base.Init();
        }

        public void SpawnItems(List<TargetEffectData> _effectData) {
            if (!LoadItemFinished) {
                WriteLog.LogError("EffectPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_effectData != null && _effectData.Count > 0) {
                for (int i = 0; i < _effectData.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_effectData[i]);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(_effectData[i]);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }
    }
}