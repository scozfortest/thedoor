
using TheDoor.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Scoz.Func {
    public class DeviceManager : MonoBehaviour {
        static Action OnApplicationQuitDo;//當Unity取消播放時
        static Action OnUnfocusDo;//當手機進入背景作業時要執行的東西放這裡
        static Action OnFocusDo;//當手機從背景作業返回時要執行的東西放這裡
        public static void AddOnApplicationQuitAction(Action _ac) {
            OnApplicationQuitDo += _ac;
        }
        public static void RemoveOnApplicationQuitAction(Action _ac) {
            OnApplicationQuitDo -= _ac;
        }
        public static void AddOnUnfocusAction(Action _ac) {
            OnUnfocusDo += _ac;
        }
        public static void RemoveOnUnfocusAction(Action _ac) {
            OnUnfocusDo -= _ac;
        }
        public static void AddOnFocusAction(Action _ac) {
            OnFocusDo += _ac;
        }
        public static void RemoveOnFocusAction(Action _ac) {
            OnFocusDo -= _ac;
        }
        public void OnApplicationFocus(bool focus) {
            if (OnFocusDo == null)
                return;
            if (focus)
                OnFocusDo?.Invoke();
            else
                OnUnfocusDo?.Invoke();
        }

        public void OnApplicationQuit() {
            if (OnApplicationQuitDo == null)
                return;
            OnApplicationQuitDo?.Invoke();
        }

    }
}