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


        public void ShowUI() {
            RefreshUI();
            SetActive(true);
        }

        public override void RefreshUI() {
            base.RefreshUI();
        }
        public void GoNextDoor() {
            AdventureManager.GoNextDoor();
        }
        public void Rest() {
            float restoreHP = GameSettingData.GetFloat(GameSetting.Rest_RestoreHP);
            float restoreSanP = GameSettingData.GetFloat(GameSetting.Rest_RestoreSanP);
            AdventureManager.PRole.AddHPRatio(restoreHP);
            AdventureManager.PRole.AddSanPRatio(restoreSanP);
            AdventureManager.GoNextDoor();
        }
        public void Search() {
            int rest_FindSupplyCount = GameSettingData.GetInt(GameSetting.Rest_FindSupplyCount);
            int rest_ChoiceCount = GameSettingData.GetInt(GameSetting.Rest_ChoiceCount);
            var rest_FindSupplyRankWeight = GameSettingData.GetJsNode(GameSetting.Rest_FindSupplyRankWeight);
            int rndRank = int.Parse(Prob.GetRandomKeyFromJsNodeKeyWeight(rest_FindSupplyRankWeight));
            var supplyDatas = SupplyData.GetRndDatas(rest_FindSupplyCount, rndRank);
            string title = string.Format(StringData.GetUIString("ChoiceUI_Title"), rest_ChoiceCount);
            SupplyChoiceUI.Instance.ShowUI(title, rest_ChoiceCount, supplyDatas, ConfirmSelecteSupply);
        }

        void ConfirmSelecteSupply(List<SupplyData> _datas) {
            List<Dictionary<string, object>> supplyListDic = new List<Dictionary<string, object>>();
            for (int i = 0; i < _datas.Count; i++) {
                var jsonDic = SupplyData.GetJsonDataDic(_datas[i]);
                supplyListDic.Add(jsonDic);
            }
            GamePlayer.Instance.AddOwnedDatas<OwnedSupplyData>(ColEnum.Supply, supplyListDic);
            BattleUI.Instance.RefreshSupplyUI();
            AdventureManager.GoNextDoor();
        }

    }
}