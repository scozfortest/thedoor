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

    public class ScriptUI : BaseUI {
        [SerializeField] Image Img;
        [SerializeField] TextMeshProUGUI Content;
        [SerializeField] GameObject[] OptionGOs;
        [SerializeField] TextMeshProUGUI[] Options;


        ScriptData CurScriptData;

        public override void Init() {
            base.Init();
        }

        public void LoadScript(string _title) {
            CurScriptData = ScriptData.GetScriptByTitle(_title);
            RefreshUI();
        }
        public override void RefreshUI() {
            base.RefreshUI();
            ShowContent();
            ShowOptions();
            ShowImg();
        }

        void ShowImg() {
            if (!string.IsNullOrEmpty(CurScriptData.RefImg)) {
                AssetGet.GetImg("Plot", CurScriptData.RefImg, sprite => {
                    Img.sprite = sprite;
                });
            }
        }

        void ShowContent() {
            if (CurScriptData == null) {
                Content.text = "";
                return;
            }
            Content.text = CurScriptData.Content;
        }

        /// <summary>
        /// 顯示選項
        /// </summary>
        void ShowOptions() {
            if (CurScriptData == null || !CurScriptData.IsOption) {
                for (int i = 0; i < OptionGOs.Length; i++) {
                    OptionGOs[i].SetActive(false);
                }
                return;
            }
            for (int i = 0; i < OptionGOs.Length; i++) {
                if (i >= CurScriptData.NextIDs.Count) {
                    OptionGOs[i].SetActive(false);
                    continue;
                }
                OptionGOs[i].SetActive(true);
                Options[i].text = CurScriptData.NextScript(i).Content;
            }
        }

        public void Next() {
            Debug.Log("Next");
            if (EndTrigger()) {
                return;
            }
            if (CurScriptData.IsOption) return;
            CurScriptData = CurScriptData.NextScript();
            if (CurScriptData == null) {//空資料就是前往下一道門
                NextDoor();
                return;
            }
            RefreshUI();
        }

        public void Option(int _index) {
            if (EndTrigger()) {
                return;
            }
            CurScriptData = CurScriptData.NextScript(_index);
            if (EndTrigger()) {
                return;
            }
            CurScriptData = CurScriptData.NextScript(0);
            RefreshUI();
        }
        bool EndTrigger() {
            if (CurScriptData == null) {//空資料就是前往下一道門
                NextDoor();
                return true;
            }
            if (string.IsNullOrEmpty(CurScriptData.EndType)) return false;
            WriteLog.Log(CurScriptData.EndType);
            switch (CurScriptData.EndType) {
                case "Battle":
                    AdventureUI.GetInstance<AdventureUI>()?.SwitchUI(AdventureUIs.Battle);
                    return true;
                case "NextDoor":
                    AdventureUI.GetInstance<AdventureUI>()?.SwitchUI(AdventureUIs.Battle);
                    return true;
                case "NextScript":
                    if (!string.IsNullOrEmpty(CurScriptData.EndValue)) LoadScript(CurScriptData.EndValue);
                    else {
                        LoadScript(ScriptTitleData.GetRndDataByType(ScriptType.Side).ID);
                    }
                    return true;
                default:
                    WriteLog.LogErrorFormat("尚未定義的Script EndType {0}", CurScriptData.EndType);
                    return false;
            }
        }
        void NextDoor() {
            WriteLog.Log("NextDoor");
        }


    }
}