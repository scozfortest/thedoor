using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class TalentSpawner : ItemSpawner_Remote<TalentPrefab> {
        public override void Init() {
            base.Init();
        }

        public void SpawnItems(List<TalentData> _talentDatas) {
            if (!LoadItemFinished) {
                WriteLog.LogError("TalentPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_talentDatas != null && _talentDatas.Count > 0) {
                for (int i = 0; i < _talentDatas.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_talentDatas[i]);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(_talentDatas[i]);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }
    }
}