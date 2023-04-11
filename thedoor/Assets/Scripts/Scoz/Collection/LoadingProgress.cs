using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace Scoz.Func {

    public class LoadingProgress {
        Dictionary<string, bool> Progress;//讀取進度清單
        Action FinishCB;//進度完成時CallBack
        public bool IsFinished { get; private set; } = false;//進度完成時會設為true，且不能再新增Loading項目
        float CBTime;//callBack前的等待時間
        /// <summary>
        /// 傳入完成進度時的callBack與完成進度時callBack前的等待時間
        /// </summary>
        public LoadingProgress(Action _cb, float _cbTime = 0) {
            Progress = new Dictionary<string, bool>();
            FinishCB = _cb;
            CBTime = _cbTime;
            IsFinished = false;
        }

        public void ResetProgress() {
            IsFinished = false;
            Progress.Clear();
        }
        /// <summary>
        /// 新增要讀取的Key
        /// </summary>
        public void AddLoadingProgress(params string[] _loadingKeys) {
            if (IsFinished) {
                DebugLogger.LogError("LoadingProgress 已經完成 無法再新增Loading項目");
                return;
            }
            for (int i = 0; i < _loadingKeys.Length; i++) {
                if (string.IsNullOrEmpty(_loadingKeys[i])) {
                    DebugLogger.LogErrorFormat("要加入的Key為空: {0}", _loadingKeys[i]);
                    continue;
                }
                if (!Progress.ContainsKey(_loadingKeys[i]))
                    Progress.Add(_loadingKeys[i], false);
                else
                    DebugLogger.LogError("嘗試新增重複的LoadingKey:" + _loadingKeys[i]);
            }
        }
        /// <summary>
        /// 完成讀取進度
        /// </summary>
        public void FinishProgress(string _loadingKey) {
            if (Progress.ContainsKey(_loadingKey))
                Progress[_loadingKey] = true;
            else {
                //DebugLogger.LogWarning("嘗試完成不存在的LoadingKey:" + _loadingKey);
            }
            if (CheckIfProgressIsFinished()) {
                IsFinished = true;
                CoroutineJob.Instance.StartNewAction(() => { FinishCB?.Invoke(); }, CBTime);
            }
        }
        bool CheckIfProgressIsFinished() {
            foreach (var key in Progress.Keys) {
                if (Progress[key] == false) {
                    return false;
                }
            }
            return true;
        }
        public List<string> GetNotFinishedKeys() {
            List<string> keys = new List<string>();
            foreach (var key in Progress.Keys) {
                if (Progress[key] == false) {
                    keys.Add(key);
                }
            }
            return keys;
        }
    }
}
