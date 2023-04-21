using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using TMPro;

namespace TheDoor.Main {

    public class DoorNodeUI : ItemSpawner_Remote<DoorNodePrefab> {

        OwnedAdventureData MyOwnedAdvData;



        public void ShowUI(OwnedAdventureData _ownedAdvData) {
            MyOwnedAdvData = _ownedAdvData;
            SpawnItems(MyOwnedAdvData.DoorDatas);
            RefreshUI();
            SetActive(true);
        }

        public override void RefreshUI() {
            base.RefreshUI();
        }

        public void SpawnItems(List<DoorData> _doorDatas) {
            if (!LoadItemFinished) {
                WriteLog.LogError("DoorNodePrefab尚未載入完成");
                return;
            }
            InActiveAllItem();
            if (_doorDatas != null && _doorDatas.Count > 0) {
                for (int i = 0; i < _doorDatas.Count; i++) {
                    bool showUnknown = i > (MyOwnedAdvData.CurDoor + FirestoreGameSetting.GetIntData(GameDataDocEnum.Adventure, "DoorVisibility "));
                    bool showIndicator = i == MyOwnedAdvData.CurDoor;
                    bool showCover = i < MyOwnedAdvData.CurDoor;
                    if (i < ItemList.Count) {
                        ItemList[i].SetData(_doorDatas[i], showUnknown, showIndicator, showCover);
                        ItemList[i].IsActive = true;
                        ItemList[i].gameObject.SetActive(true);
                    } else {
                        var item = Spawn();
                        item.SetData(_doorDatas[i], showUnknown, showIndicator, showCover);
                    }
                    ItemList[i].transform.SetParent(ParentTrans);
                }
            }
        }

    }
}
