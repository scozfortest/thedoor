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


        //進遊戲不先初始化，等到要用時才初始化的UI放這裡
        //[SerializeField] AssetReference SocialUIAsset;
        //後產生的UI父層
        //[SerializeField] Transform SocialUIParent;

        public override void Init() {
            base.Init();
            MyScriptUI.Init();
            SwitchUI(AdventureUIs.Default);
            MyScriptUI.LoadScript("witch");
        }

        public void SwitchUI(AdventureUIs _ui, Action _cb = null) {

            if (LastPopupUI != null)
                LastPopupUI.SetActive(false);//關閉彈出介面
            //PlayerInfoUI.GetInstance<PlayerInfoUI>()?.SetActive(false);//所有介面預設都不會開啟資訊界面

            switch (_ui) {
                case AdventureUIs.Default://本來在其他介面時，可以傳入Default來關閉彈出介面並顯示回預設介面
                    MyScriptUI.SetActive(true);
                    _cb?.Invoke();
                    break;
                case AdventureUIs.Test:
                    ////判斷是否已經載入過此UI，若還沒載過就跳讀取中並開始載入
                    //if (MyScratchCardUI != null) {
                    //    MyScratchCardUI.ShowUI();
                    //    _cb?.Invoke();
                    //} else {
                    //    PopupUI.ShowLoading(StringData.GetUIString("WaitForLoadingUI"));
                    //    //初始化UI
                    //    AddressablesLoader.GetPrefabByRef(ScratchCardUIAsset, (prefab, handle) => {
                    //        PopupUI.HideLoading();
                    //        GameObject go = Instantiate(prefab);
                    //        go.transform.SetParent(ScratchCardUIParent);
                    //        go.transform.localPosition = prefab.transform.localPosition;
                    //        go.transform.localScale = prefab.transform.localScale;
                    //        /*
                    //      RectTransform rect = go.GetComponent<RectTransform>();
                    //      rect.offsetMin = Vector2.zero;//Left、Bottom
                    //      rect.offsetMax = Vector2.zero;//Right、Top
                    //      */
                    //        MyScratchCardUI = go.GetComponent<ScratchCardUI>();
                    //        MyScratchCardUI.Init();
                    //        MyScratchCardUI.ShowUI();
                    //    }, () => { DebugLogger.LogError("載入ScratchCardUIAsset失敗"); });
                    //}
                    //LastPopupUI = MyScratchCardUI;
                    break;
            }
        }

    }
}