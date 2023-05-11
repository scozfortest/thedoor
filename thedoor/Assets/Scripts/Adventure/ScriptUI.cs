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
        [SerializeField] Button[] OptionBtns;
        [SerializeField] TextMeshProUGUI[] Options;


        ScriptData CurScriptData;
        public static ScriptUI Instance { get; private set; }


        public override void Init() {
            base.Init();
            Instance = this;
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
        /// <summary>
        /// 每段劇情要執行的東西放這裡
        /// </summary>
        void DoScriptThings() {
            RefreshUI();
            if (CurScriptData.CamShake != 0) CameraManager.ShakeCam(CameraManager.CamNames.Adventure, 0.5f, 0.2f, CurScriptData.CamShake);
            if (!string.IsNullOrEmpty(CurScriptData.RefSound)) AudioPlayer.PlayAudioByPath(MyAudioType.Sound, CurScriptData.RefSound);
            if (!string.IsNullOrEmpty(CurScriptData.RefVoice)) AudioPlayer.PlayAudioByPath(MyAudioType.Voice, CurScriptData.RefVoice);
            if (!string.IsNullOrEmpty(CurScriptData.RefBGM)) AudioPlayer.PlayAudioByPath(MyAudioType.Music, CurScriptData.RefBGM);
            if (CurScriptData.CamEffects != null) {
                foreach (var particlePath in CurScriptData.CamEffects) {
                    GameObjSpawner.SpawnParticleObjByPath(particlePath, transform);
                }
            }



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
            if (CurScriptData == null || !CurScriptData.HaveOptions) {
                for (int i = 0; i < OptionBtns.Length; i++) {
                    OptionBtns[i].gameObject.SetActive(false);
                }
                return;
            }
            for (int i = 0; i < OptionBtns.Length; i++) {
                if (i >= CurScriptData.NextIDs.Count) {
                    OptionBtns[i].gameObject.SetActive(false);
                    continue;
                }
                OptionBtns[i].gameObject.SetActive(true);
                Options[i].text = CurScriptData.NextScript(i).Content;

                //沒達成條件的選項要標示為灰色
                bool meetRequire = CurScriptData.NextScript(i).MeetAllRequirements();
                OptionBtns[i].interactable = meetRequire;
                if (meetRequire)
                    Options[i].color = Color.white;
                else
                    Options[i].color = Color.gray;
            }
        }

        public void Next() {
            if (EndTrigger()) {
                return;
            }
            if (CurScriptData.HaveOptions) return;
            CurScriptData = CurScriptData.NextScript();
            if (CurScriptData == null) {//空資料就是前往下一道門
                NextDoor();
                return;
            }
            DoScriptThings();
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
            if (CurScriptData == null) {
                NextDoor();
                return;
            }
            DoScriptThings();
        }
        bool EndTrigger() {
            if (CurScriptData == null) {//空資料就是前往下一道門
                NextDoor();
                return true;
            }
            if (string.IsNullOrEmpty(CurScriptData.EndType)) return false;
            switch (CurScriptData.EndType) {
                case "Battle":
                    AdventureManager.CallBattle(int.Parse(CurScriptData.EndValue));
                    AdventureUI.Instance?.SwitchUI(AdventureUIs.Battle);
                    return true;
                case "NextDoor":
                    AdventureUI.Instance?.SwitchUI(AdventureUIs.Battle);
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
            AdventureManager.GoNextDoor();
        }


    }
}