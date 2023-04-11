using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Scoz.Func {
    public class MyText : Text {
        [SerializeField]
        public string UIString;
        public bool UseLanguageFont = true;//是否跟據語系變換字體
        bool IsAddTextList;
        static List<MyText> MyTextList = new List<MyText>();
        public delegate void MyFunction();
        static List<MyFunction> MyFuncList = new List<MyFunction>();
#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            if (!Application.isPlaying) {
                text = "My Text";
                fontSize = 30;
                color = Color.black;
                rectTransform.sizeDelta = new Vector2(250, 100);
                alignment = TextAnchor.MiddleCenter;
                font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
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
            if (UseLanguageFont)
                font = GameDictionary.GetUsingLanguageFont();
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
                    DebugLogger.LogError(_e);
                }
            }
        }
        public static void RefreshFuncs() {
            MyFuncList.RemoveAll(item => item == null);
            for (int i = 0; i < MyFuncList.Count; i++) {
                try {
                    MyFuncList[i]?.Invoke();
                } catch (Exception _e) {
                    DebugLogger.LogWarning("MyText.RefreshFuncs警告: " + _e);
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