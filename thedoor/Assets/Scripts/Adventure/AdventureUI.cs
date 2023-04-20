using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;

namespace TheDoor.Main {

    public class AdventureUI : BaseUI {

        public BaseUI LastPopupUI { get; private set; }//紀錄上次的彈出介面(切介面時要關閉上次的彈出介面)

        static bool FirstEnterAdventure = true;//第一次進入冒險後會設定回false 用來判斷是否第一次進入冒險

        //進遊戲就要初始化的UI放這裡(會增加場景切換時的讀取時間)
        [SerializeField] ScriptUI MyScriptUI;
        [SerializeField] RoleStateUI MyRoleStateUI;

        //進遊戲不先初始化，等到要用時才初始化的UI放這裡
        [SerializeField] AssetReference BattleUIAsset;
        //後產生的UI父層
        [SerializeField] Transform BattleUIParent;
        //後產生的UI實體
        BattleUI MyBattleUI;

        public override void Init() {
            base.Init();
            MyScriptUI.Init();
            MyRoleStateUI.Init();
            SwitchUI(AdventureUIs.Default);
            MyScriptUI.LoadScript("witch");
            MyRoleStateUI.ShowUI(GamePlayer.Instance.Data.CurRole);
        }

        public void SwitchUI(AdventureUIs _ui, Action _cb = null) {

            if (LastPopupUI != null)
                LastPopupUI.SetActive(false);//關閉彈出介面
            //PlayerInfoUI.GetInstance<PlayerInfoUI>()?.SetActive(false);//所有介面預設都不會開啟資訊界面

            switch (_ui) {
                case AdventureUIs.Default://本來在其他介面時，可以傳入Default來關閉彈出介面並顯示回預設介面
                    MyScriptUI.SetActive(true);
                    MyRoleStateUI.ShowUI(GamePlayer.Instance.Data.CurRole);
                    MyBattleUI?.SetActive(false);
                    _cb?.Invoke();
                    LastPopupUI = MyScriptUI;
                    break;
                case AdventureUIs.Battle:
                    MyScriptUI.SetActive(false);
                    MyRoleStateUI.SetActive(false);
                    MyBattleUI?.SetActive(true);
                    //判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
                    if (MyBattleUI != null) {
                        MyBattleUI.SetBattle();
                        _cb?.Invoke();
                    } else {
                        LoadAssets(_ui, _cb);//讀取介面
                    }
                    LastPopupUI = MyBattleUI;

                    break;
            }
        }

        void LoadAssets(AdventureUIs _ui, Action _cb) {
            switch (_ui) {
                case AdventureUIs.Battle:
                    PopupUI.ShowLoading(StringData.GetUIString("WaitForLoadingUI"));
                    //初始化UI
                    AddressablesLoader.GetPrefabByRef(BattleUIAsset, (prefab, handle) => {
                        PopupUI.HideLoading();
                        GameObject go = Instantiate(prefab);
                        go.transform.SetParent(BattleUIParent);

                        RectTransform rect = go.GetComponent<RectTransform>();
                        go.transform.localPosition = prefab.transform.localPosition;
                        go.transform.localScale = prefab.transform.localScale;
                        rect.offsetMin = Vector2.zero;//Left、Bottom
                        rect.offsetMax = Vector2.zero;//Right、Top

                        MyBattleUI = go.GetComponent<BattleUI>();
                        MyBattleUI.Init();
                        MyBattleUI.SetBattle();
                        _cb?.Invoke();
                    }, () => { WriteLog.LogError("載入BattleUIAsset失敗"); });
                    break;
            }
        }
    }
}