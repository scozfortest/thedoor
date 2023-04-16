using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

namespace Scoz.Func {
    public class MyTextPro : TextMeshProUGUI {
        [SerializeField]
        public string UIString;
        public bool UseLanguageFont = true;//是否跟據語系變換字體
        bool IsAddTextList;
        static List<MyTextPro> MyTextList = new List<MyTextPro>();
        public delegate void MyFunction();
        static List<MyFunction> MyFuncList = new List<MyFunction>();
#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            if (!Application.isPlaying) {
                text = "My TextPro";
                fontSize = 30;
                color = Color.black;
                rectTransform.sizeDelta = new Vector2(250, 100);
                raycastTarget = false;
            }
        }
#endif
        protected override void OnEnable() {
            base.OnEnable();
            if (!Application.isPlaying)
                return;
            if (!string.IsNullOrEmpty(UIString)) {
                if (!IsAddTextList)
                    MyTextList.Add(this);
                IsAddTextList = true;
            }
            RefreshText();
        }
        protected override void OnDestroy() {
            MyTextList.Remove(this);
        }
        public void RefreshText() {
            if (UseLanguageFont) {
                var tmpFont = GameDictionary.GetUsingLanguageFontAsset();
                if (tmpFont != null)
                    font = GameDictionary.GetUsingLanguageFontAsset();
            }
            if (string.IsNullOrEmpty(UIString) || !GameDictionary.IsInit)
                return;
            text = StringData.GetUIString(UIString);
        }
        public static void RefreshActivityTextsAndFunctions() {
            RefreshActiveTexts();
            RefreshFuncs();
        }
        public static void RefreshActiveTexts() {
            MyTextList.RemoveAll(item => item == null);
            for (int i = 0; i < MyTextList.Count; i++) {
                try {
                    if (MyTextList[i].isActiveAndEnabled)
                        MyTextList[i].RefreshText();
                } catch (Exception _e) {
                    WriteLog.LogError(_e);
                }
            }
        }
        public static void RefreshFuncs() {
            MyFuncList.RemoveAll(item => item == null);
            for (int i = 0; i < MyFuncList.Count; i++) {
                try {
                    MyFuncList[i]?.Invoke();
                } catch (Exception _e) {
                    WriteLog.LogWarning("MyText.RefreshFuncs警告: " + _e);
                }
            }
        }
        public static void AddRefreshFunc(MyFunction _func) {
            MyFuncList.Add(_func);
        }
        public static void RemoveRefreshFunc(MyFunction _func) {
            MyFuncList.Remove(_func);
        }
    }
}