using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace Scoz.Func {
    public class BaseUI : MonoBehaviour {
        protected bool IsInit = false;
        public static Dictionary<string, BaseUI> UIDic = new Dictionary<string, BaseUI>();
        public static bool operator true(BaseUI baseUI) { return baseUI != null; }
        public static bool operator false(BaseUI baseUI) { return baseUI == null; }


        public static T GetInstance<T>() where T : BaseUI {
            string name = typeof(T).FullName;
            if (!UIDic.ContainsKey(name))
                return null;
            return (T)UIDic[name];
        }
        public virtual void Init() {
            if (IsInit)
                return;

            List<string> keys = new List<string>(UIDic.Keys);

            foreach (var key in keys) {
                if (UIDic[key] == null || UIDic[key].gameObject == null) {
                    UIDic.Remove(key);
                }
            }

            UIDic[this.GetType().FullName] = this;
            IsInit = true;
            MyText.AddRefreshFunc(RefreshUI);
            //RefreshUI();
        }
        protected virtual void OnEnable() {

        }
        protected virtual void OnDisable() {

        }
        protected virtual void OnDestroy() {
            MyText.RemoveRefreshFunc(RefreshUI);
            UIDic[this.GetType().FullName] = null;
            //Destroy(gameObject);
        }
        public virtual void RefreshUI() {
        }

        public virtual void SetActive(bool _bool) {
            gameObject.SetActive(_bool);
        }
    }
}
