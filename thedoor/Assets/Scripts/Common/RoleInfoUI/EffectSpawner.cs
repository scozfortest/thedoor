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

        public void SpawnItems(List<StatusEffect> _effects, Camera _cam, Vector2 _tipOffset) {
            if (!LoadItemFinished) {
                WriteLog.LogError("EffectPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_effects != null && _effects.Count > 0) {
                for (int i = 0; i < _effects.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_effects[i], _cam, _tipOffset);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(_effects[i], _cam, _tipOffset);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }
    }
}