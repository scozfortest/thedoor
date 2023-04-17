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

        public void SpawnItems(List<SkillData> _skillDatas) {
            if (!LoadItemFinished) {
                WriteLog.LogError("TalentPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_skillDatas != null && _skillDatas.Count > 0) {
                for (int i = 0; i < _skillDatas.Count; i++) {
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_skillDatas[i]);
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