using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Scoz.Func;
using System;
using TMPro;

namespace TheDoor.Main {

    public class RestUI : BaseUI {

        [SerializeField] SupplySpawner MySupplySpawner;
        [SerializeField] Image RestImg;

        public static RestUI Instance { get; private set; }

        public override void Init() {
            base.Init();
            Instance = this;
            MySupplySpawner.Init();
            MySupplySpawner.LoadItemAsset(null);
        }

        public void ShowUI() {
            AdventureManager.MyState = AdvState.Rest;
            AssetGet.GetImg("Rest", RestData.GetRndData().Ref, sprite => {
                RestImg.sprite = sprite;
                RestImg.SetNativeSize();
            });
            RefreshUI();
            SetActive(true);
        }

        public override void RefreshUI() {
            base.RefreshUI();
            MySupplySpawner.SpawnItems(GamePlayer.Instance.Data.CurRole.GetSupplyDatas(null, SupplyData.Timing.Rest), ActionSupplyPrefab.ActionSupplyType.Usable);
        }
        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }
        public void Rest() {
            float restoreHPRatio = GameSettingData.GetFloat(GameSetting.Rest_RestoreHP);
            float restoreSanPRatio = GameSettingData.GetFloat(GameSetting.Rest_RestoreSanP);
            int restoreHP = Mathf.RoundToInt(restoreHPRatio * AdventureManager.PRole.MaxHP);
            int restoreSanP = Mathf.RoundToInt(restoreSanPRatio * AdventureManager.PRole.MaxSanP);
            restoreHP += AdventureManager.PRole.GetRestExtraHPRestoreByTalent();
            restoreSanP += AdventureManager.PRole.GetRestExtraSanPRestoreByTalent();
            AdventureManager.PRole.AddHP(restoreHP);
            AdventureManager.PRole.AddSanP(restoreSanP);
            AdventureManager.GoNextDoor();
        }
        public void Search() {
            int rest_FindSupplyCount = GameSettingData.GetInt(GameSetting.Rest_FindSupplyCount);
            rest_FindSupplyCount += AdventureManager.PRole.GetSearchExtraSupplyByTalent();//腳色天賦加成
            int rest_ChoiceCount = GameSettingData.GetInt(GameSetting.Rest_ChoiceCount);
            var rest_FindSupplyRankWeight = GameSettingData.GetJsNode(GameSetting.Rest_FindSupplyRankWeight);
            int rndRank = int.Parse(Prob.GetRandomKeyFromJsNodeKeyWeight(rest_FindSupplyRankWeight));
            var supplyDatas = SupplyData.GetRndDatas(rest_FindSupplyCount, rndRank, null);
            string title = string.Format(StringData.GetUIString("ChoiceUI_Title"), rest_ChoiceCount);
            SupplyChoiceUI.Instance.ShowUI(title, rest_ChoiceCount, supplyDatas, ConfirmSelecteSupply);
        }

        void ConfirmSelecteSupply(List<SupplyData> _datas) {
            GamePlayer.Instance.GainSupply(_datas);
            BattleUI.Instance.RefreshSupplyUI();
            AdventureManager.GoNextDoor();
        }

    }
}