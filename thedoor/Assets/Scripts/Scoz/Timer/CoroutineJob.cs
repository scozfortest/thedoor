using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scoz.Func {

    public class CoroutineJob : MonoBehaviour {
        class AfterInitDoAction {
            public Action DoAction;
            float waitTime;
            public AfterInitDoAction(Action _ac, float _waitTime) {
                DoAction = _ac;
                WaitTime = _waitTime;
            }

            public float WaitTime {
                get {
                    return waitTime;
                }

                set {
                    if (value <= 0)
                        value = 0.01f;
                    waitTime = value;
                }
            }
        }
        public static CoroutineJob Instance;
        Dictionary<int, Coroutine> MyCoroutines = new Dictionary<int, Coroutine>();
        int CoroutineID = 0;
        private void Awake() {
            Instance = this;
            DoActionAfterInited();
        }
        static List<AfterInitDoAction> AfterInitNeedDoActions = new List<AfterInitDoAction>();

        /// <summary>
        /// 在初始化前可先加入要執行的動作，等初始化後會執行，若已經完成初始化後才加入就會直接執行
        /// </summary>
        public static void AddActionBeforeInit(Action _action, float _waitTime = 0.01f) {
            if (Instance == null) {
                AfterInitDoAction afertInitDoAC = new AfterInitDoAction(_action, _waitTime);
                AfterInitNeedDoActions.Add(afertInitDoAC);
            } else
                Instance.StartNewAction(_action, _waitTime);
        }
        void DoActionAfterInited() {
            for (int i = 0; i < AfterInitNeedDoActions.Count; i++) {
                StartNewAction(AfterInitNeedDoActions[i].DoAction, AfterInitNeedDoActions[i].WaitTime);
            }
            AfterInitNeedDoActions.Clear();
        }
        /// <summary>
        /// waitTime傳入0會等待到此偵結束時執行
        /// </summary>
        public int StartNewAction(Action _action, float _waitTime = 0) {
            CoroutineID++;
            Coroutine courtine = StartCoroutine(RunAction(_action, _waitTime, CoroutineID));
            MyCoroutines.Add(CoroutineID, courtine);
            return CoroutineID;
        }
        /// <summary>
        /// 指定CoroutineID(傳入負數int)，waitTime傳入0會等待到此偵結束時執行
        /// </summary>
        public int StartNewAction_DesignatedCoroutineID(Action _action, int _coroutineID, float _waitTime = 0) {
            Coroutine courtine = StartCoroutine(RunAction(_action, _waitTime, _coroutineID));
            MyCoroutines.Add(_coroutineID, courtine);
            return _coroutineID;
        }
        public void StopCoroutine(int _coroutineID) {
            if (MyCoroutines.ContainsKey(_coroutineID)) {
                StopCoroutine(MyCoroutines[_coroutineID]);
                MyCoroutines.Remove(_coroutineID);
            }
        }
        IEnumerator RunAction(Action _action, float _waitTime, int _coroutineID) {
            if (_waitTime > 0)
                yield return new WaitForSeconds(_waitTime);
            else
                yield return new WaitForEndOfFrame();
            _action?.Invoke();
            StopCoroutine(_coroutineID);
        }
        /// <summary>
        /// waitTime傳入0會等待到此偵結束時執行
        /// </summary>
        public int StartNewAction(Action<object> _action, object _obj, float _waitTime = 0) {
            CoroutineID++;
            Coroutine courtine = StartCoroutine(RunAction(_action, _obj, CoroutineID, _waitTime));
            MyCoroutines.Add(CoroutineID, courtine);
            return CoroutineID;
        }
        IEnumerator RunAction(Action<object> _action, object _obj, int _coroutineID, float _waitTime) {
            if (_waitTime > 0)
                yield return new WaitForSeconds(_waitTime);
            else
                yield return new WaitForEndOfFrame();
            _action?.Invoke(_obj);
            StopCoroutine(_coroutineID);
        }
    }
}