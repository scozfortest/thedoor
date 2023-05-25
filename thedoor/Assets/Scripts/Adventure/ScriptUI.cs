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
        List<ItemData> RewardItems;


        public override void Init() {
            base.Init();
            Instance = this;
        }

        public void LoadScript(string _title) {
            AdventureManager.MyState = AdvState.Script;
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
        void DoScriptThings(ScriptData _data) {
            DoEffectThings(_data);
            if (AdventureManager.PRole.IsDead) return;//DoEffectThings腳色死亡的話 就不用執行後續
            DoRequireThings(_data);
            DoGainThings(_data);
            DoTriggerThings(_data);
            DoRewardThings(_data);

            if (_data.CamShake != 0) CameraManager.ShakeCam(CameraManager.CamNames.Adventure, 0.2f, 1, _data.CamShake);
            if (!string.IsNullOrEmpty(_data.RefSound)) AudioPlayer.PlayAudioByPath(MyAudioType.Sound, _data.RefSound);
            if (!string.IsNullOrEmpty(_data.RefVoice)) AudioPlayer.PlayAudioByPath(MyAudioType.Voice, _data.RefVoice);
            if (!string.IsNullOrEmpty(_data.RefBGM)) AudioPlayer.PlayAudioByPath(MyAudioType.Music, _data.RefBGM);
            if (_data.CamEffects != null) {
                foreach (var particlePath in _data.CamEffects) {
                    GameObjSpawner.SpawnParticleObjByPath(particlePath, AdventureUI.Instance?.transform);
                }
            }

        }
        void DoEffectThings(ScriptData _data) {
            var statusEffects = _data.GetAction(AdventureManager.PRole);
            if (statusEffects != null)
                StatusEffect.DoScriptEffectsToPlayerRole(AdventureManager.PRole, statusEffects);
        }

        /// <summary>
        /// 執行獲得物品
        /// </summary>
        void DoGainThings(ScriptData _data) {
            if (_data.GainItems == null || _data.GainItems.Count == 0) return;
            GamePlayer.Instance.GainItems(_data.GainItems);
            PopupUI.ShowGainItemListUI(StringData.GetUIString("GainItem"), _data.GainItems, null);
        }

        /// <summary>
        /// 執行戰鬥勝利獲得物品
        /// </summary>
        void DoRewardThings(ScriptData _data) {
            if (_data.RewardItems == null || _data.RewardItems.Count == 0) return;
            RewardItems = _data.RewardItems;
        }

        /// <summary>
        /// 執行觸發效果
        /// </summary>
        void DoTriggerThings(ScriptData _data) {
            switch (_data.MyTriggerType) {
                case ScriptData.TriggerType.None:
                    break;
                case ScriptData.TriggerType.GainTalent:
                    TalentData talentData = null;
                    if (_data.TriggerValue != "Rnd") talentData = TalentData.GetData(_data.TriggerValue);
                    else talentData = TalentData.GetRndTalent(AdventureManager.PRole.Talents.Select(a => a.MyTalentType).ToHashSet());
                    if (talentData != null) AdventureManager.PRole.GainTalent(talentData);
                    break;
                case ScriptData.TriggerType.FirstStrike:
                    //不用處理 直接在進Battle時取先攻資料來初始化戰鬥
                    break;
                default:
                    WriteLog.LogError("尚未實作的ScriptData.TriggerType: " + _data.MyTriggerType);
                    break;
            }
        }
        /// <summary>
        /// 執行需求物品
        /// </summary>
        void DoRequireThings(ScriptData _data) {
            if (_data.Requires == null || _data.Requires.Count == 0) return;
            var ownedSupplyDatas = GamePlayer.Instance.GetOwnedDatas<OwnedSupplyData>(ColEnum.Supply);
            HashSet<int> ids = null;
            HashSet<string> tags = null;
            foreach (var require in _data.Requires) {
                switch (require.MyType) {
                    case ScriptRequireType.UseSupplies:
                    case ScriptRequireType.ConsumeSupplies:
                        ids = TextManager.GetIntHashSetFromSplitStr(require.Value, ',');
                        if (ids != null) {
                            foreach (var id in ids) {
                                var ownedData = ownedSupplyDatas.Find(a => a.ID == id);
                                if (ownedData == null) continue;
                                if (require.MyType == ScriptRequireType.UseSupplies)
                                    ownedData.AddUsage(1);
                                else if (require.MyType == ScriptRequireType.ConsumeSupplies)
                                    ownedData.AddUsage(ownedData.Usage);
                            }
                        }
                        break;
                    case ScriptRequireType.UseSupplyTags:
                    case ScriptRequireType.ConsumeSupplyTags:
                        tags = TextManager.GetHashSetFromSplitStr(require.Value, ',');
                        if (tags != null) {
                            foreach (var ownedData in ownedSupplyDatas) {
                                var supplyData = SupplyData.GetData(ownedData.ID);
                                if (supplyData == null) continue;
                                if (!supplyData.BelongToTags(tags)) continue;
                                if (require.MyType == ScriptRequireType.UseSupplies)
                                    ownedData.AddUsage(1);
                                else if (require.MyType == ScriptRequireType.ConsumeSupplies)
                                    ownedData.AddUsage(ownedData.Usage);
                            }
                        }
                        break;
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

        /// <summary>
        /// Next按鈕按下觸發
        /// </summary>
        public void Next() {
            if (EndTrigger()) {
                return;
            }
            if (CurScriptData.HaveOptions) return;//如果有選項不會觸發此function才對
            if (!CurScriptData.ConditionalNext) CurScriptData = CurScriptData.NextScript();
            else {
                for (int i = 0; i < CurScriptData.NextIDs.Count; i++) {
                    if (i == CurScriptData.NextIDs.Count - 1) {//最後一個就不考慮條件有沒有符合了 因為一定要跳下一句
                        CurScriptData = CurScriptData.NextScript(i);
                        break;
                    }
                    if (CurScriptData.NextScript(i).MeetAllRequirements()) {
                        CurScriptData = CurScriptData.NextScript(i);
                        break;
                    }
                }
            }

            if (CurScriptData == null) {//空資料就是前往下一道門
                NextDoor();
                return;
            }
            DoScriptThings(CurScriptData);
            RefreshUI();
        }

        /// <summary>
        /// 點了選項觸發
        /// </summary>
        public void Option(int _index) {
            DoScriptThings(CurScriptData.NextScript(_index));//先執行點選了該選項需要執行的事情
            if (EndTrigger()) {//執行結束觸發的事情
                return;
            }
            CurScriptData = CurScriptData.NextScript(_index);//跳至點了選項的ScriptData
            if (EndTrigger()) {
                return;
            }
            //跳至選項的下一個ScriptData
            if (!CurScriptData.ConditionalNext) CurScriptData = CurScriptData.NextScript();
            else {
                for (int i = 0; i < CurScriptData.NextIDs.Count; i++) {
                    if (i == CurScriptData.NextIDs.Count - 1) {//最後一個就不考慮條件有沒有符合了 因為一定要跳下一句
                        CurScriptData = CurScriptData.NextScript(i);
                        break;
                    }
                    if (CurScriptData.NextScript(i).MeetAllRequirements()) {
                        CurScriptData = CurScriptData.NextScript(i);
                        break;
                    }
                }
            }

            if (CurScriptData == null) {
                NextDoor();
                return;
            }
            DoScriptThings(CurScriptData);
            RefreshUI();
        }
        bool EndTrigger() {
            if (CurScriptData == null) {//空資料就是前往下一道門
                NextDoor();
                return true;
            }
            if (string.IsNullOrEmpty(CurScriptData.EndType)) return false;
            switch (CurScriptData.EndType) {
                case "Battle":
                    int firstStrike = 0;
                    try {
                        firstStrike = (CurScriptData.MyTriggerType == ScriptData.TriggerType.FirstStrike) ? int.Parse(CurScriptData.TriggerValue) : 0;
                    } catch (Exception _e) {
                        WriteLog.LogError("Script表的TriggerValue有錯 ID: " + CurScriptData.ID);
                    }
                    AdventureManager.CallBattle(int.Parse(CurScriptData.EndValue), RewardItems, firstStrike);
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