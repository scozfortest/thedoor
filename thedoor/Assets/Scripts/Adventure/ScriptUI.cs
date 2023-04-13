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
            Content.text = CurScriptData.Content;
            ShowOptions();
        }

        /// <summary>
        /// 顯示選項
        /// </summary>
        void ShowOptions() {
            if (CurScriptData == null || CurScriptData.NextIDs.Count <= 1) {
                for (int i = 0; i < 5; i++) {
                    OptionGOs[i].SetActive(false);
                }
                return;
            }
            for (int i = 0; i < 5; i++) {
                if (i >= CurScriptData.NextIDs.Count) {
                    OptionGOs[i].SetActive(false);
                    continue;
                }
                OptionGOs[i].SetActive(true);
                Options[i].text = CurScriptData.GetNextScript(i).Content;
            }
        }

        public void Next() {
            if (EndTrigger())
                End();
            if (CurScriptData.NextIDs.Count > 1) return;
            CurScriptData = CurScriptData.GetNextScript();
            RefreshUI();
        }

        public void Option(int _index) {
            if (EndTrigger())
                End();
            CurScriptData = CurScriptData.GetNextScript(_index);
            RefreshUI();
        }
        bool EndTrigger() {
            if (string.IsNullOrEmpty(CurScriptData.EndType)) return false;
            switch (CurScriptData.EndType) {
                case "Battle":
                    return true;
                case "NextDoor":
                    return true;
                default:
                    WriteLog.LogErrorFormat("尚未定義的Script EndType {0}", CurScriptData.EndType);
                    return false;
            }
        }

        void End() {

        }


    }
}