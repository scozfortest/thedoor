using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {

    public static class ContentSizeFitterExtension {
        /// <summary>
        /// 刷新ContentSizeFitter，要跑這個圖片隨文字延展才會更新
        /// </summary>
        public static void Update(this ContentSizeFitter _contentSizeFitter, Action _ac = null) {
            if (_contentSizeFitter == null)
                return;
            if (CoroutineJob.Instance == null)
                return;
            _contentSizeFitter.enabled = false;
            CoroutineJob.Instance.StartNewAction(() => {
                if (_contentSizeFitter != null)
                    _contentSizeFitter.enabled = true;
                if (_ac != null) {
                    CoroutineJob.Instance.StartNewAction(_ac);
                }
            }, 0.01f);
        }
        /// <summary>
        /// 刷新ContentSizeFitter，要跑這個圖片隨文字延展才會更新
        /// </summary>
        public static void Update(this ContentSizeFitter[] _contentSizeFitters) {
            if (_contentSizeFitters == null)
                return;
            if (CoroutineJob.Instance == null)
                return;
            for (int i = 0; i < _contentSizeFitters.Length; i++) {
                if (_contentSizeFitters[i] != null) {
                    _contentSizeFitters[i].enabled = false;
                    int index = i;
                    CoroutineJob.Instance.StartNewAction(() => {
                        _contentSizeFitters[index].enabled = true;
                    }, 0.01f);
                }
            }
        }
    }
}
