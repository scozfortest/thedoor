using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Scoz.Func {
    public class TextTypeEffector : MonoBehaviour {
        class Typer {
            public int TyperID { get; private set; }
            public float Speed { get; private set; }
            public bool IsFinished { get; private set; }
            public Coroutine MyCoroutine { get; private set; }
            public int TypeCount {
                get {
                    return TypeChars.Length;
                }
            }

            Text MyText;
            int CurTypeCharIndex;
            Char[] TypeChars;
            Action OnFinishCB;
            //ContentSizeFitter ParentFitter;不使用 因為這樣會讓文字跑完時抖一下很醜

            public Typer(int _typerID, Text _text, string _str, float _speed, Action _onFinishCB) {
                if (_text == null) {
                    DebugLogger.LogError("傳入Text為null");
                    return;
                }
                if (string.IsNullOrEmpty(_str)) {
                    _text.text = "";
                    FinishTyping();
                    return;
                }
                TyperID = _typerID;
                MyText = _text;
                MyText.text = "";
                CurTypeCharIndex = 0;
                TypeChars = _str.ToCharArray();
                Speed = _speed;
                IsFinished = false;
                OnFinishCB = _onFinishCB;
                //ParentFitter = MyText.transform.parent.GetComponent<ContentSizeFitter>();
            }
            public void SetCoroutine(Coroutine _coroutine) {
                MyCoroutine = _coroutine;
            }
            /// <summary>
            /// 打下一個字
            /// </summary>
            public void TypeNextChar() {
                if (IsFinished)
                    return;
                MyText.text = MyText.text + TypeChars[CurTypeCharIndex];
                CurTypeCharIndex++;
                if (CurTypeCharIndex >= TypeChars.Length)//若已經打完所有字就callback
                {
                    FinishTyping();
                    return;
                }
            }
            public void StopTyping() {
                IsFinished = true;
            }
            void FinishTyping() {
                if (IsFinished == false) {
                    IsFinished = true;
                    /*不使用 因為這樣會讓文字跑完時抖一下很醜
                    if (ParentFitter != null)//要開關ContentFitter更新UI
                    {
                        ParentFitter.enabled = false;
                        CoroutineJob.Instance.StartNewAction(() =>
                        {
                            ParentFitter.enabled = true;
                        });
                    }
                    */
                    OnFinishCB?.Invoke();
                }

            }
        }

        static TextTypeEffector Instance;
        public const float DefaultSpeed = 0.01f;
        static int MaxTyperID = 0;
        static Dictionary<int, Typer> Typers;
        private void Start() {
            Init();
        }
        public void Init() {
            Instance = this;
            Typers = new Dictionary<int, Typer>();
            MaxTyperID = 0;
        }

        public static int StartTyping(Text _text, string _str, float _speed = DefaultSpeed, Action _cb = null) {
            if (Instance == null) {
                DebugLogger.LogError("傳入Text為null");
                return -1;
            }
            if (_text == null) {
                DebugLogger.LogError("尚未初始化TextTypeEffector");
                return -1;
            }
            int typerID = MaxTyperID;
            MaxTyperID++;
            Typer typer = new Typer(typerID, _text, _str, _speed, _cb);
            Typers.Add(typerID, typer);

            Coroutine coroutine = Instance.StartCoroutine(Instance.Typing(typer));
            Typers[typerID].SetCoroutine(coroutine);
            return typerID;

        }
        public static void StopTyper(int _typerID) {
            if (Instance == null) {
                return;
            }
            if (Typers.ContainsKey(_typerID)) {
                Instance.StopCoroutine(Typers[_typerID].MyCoroutine);
                Typers[_typerID].StopTyping();
                Typers[_typerID] = null;
                Typers.Remove(_typerID);
            }
        }

        IEnumerator Typing(Typer _typer) {
            for (int i = 0; i < _typer.TypeCount; i++) {
                if (_typer == null || _typer.IsFinished)
                    break;
                _typer.TypeNextChar();
                yield return new WaitForSeconds(_typer.Speed);
            }
        }

    }
}