using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Scoz.Func {
    public class PopupTextSpawner : ItemSpawner<PopupTextItem> { }
    public class PopupText : MonoBehaviour {
        public enum AniName {
            gainCurrecny,
            criticalHit,
            fadeOutTip,
            gainHappyness,
        }
        public static bool IsInit { get; private set; }
        static PopupText Myself;
        [SerializeField]
        PopupTextItem PopupItemPrefab = null;


        PopupTextSpawner MyPopupTextSpawner;

        private void Start() {
            if (!IsInit)
                Init();
        }
        public void Init() {
            Myself = this;
            IsInit = true;
            MyPopupTextSpawner = gameObject.AddComponent<PopupTextSpawner>();
            MyPopupTextSpawner.ParentTrans = transform;
            MyPopupTextSpawner.ItemPrefab = PopupItemPrefab;
            DontDestroyOnLoad(gameObject);
        }
        public Camera GetCamera() {
            return GetComponent<Camera>();
        }
        public static void Show(string _text, Vector2 _pos, AniName _aniName) {
            if (!Myself)
                return;
            PopupTextItem item = Myself.GetAvailableItem();
            if (item == null)
                item = Myself.SpawnNewItem();
            item.transform.position = _pos;
            item.Init(_text, _aniName);
        }
        public static void Show(string _text, Sprite _sprite, Vector2 _pos, AniName _aniName) {
            if (!Myself)
                return;
            PopupTextItem item = Myself.GetAvailableItem();
            if (item == null)
                item = Myself.SpawnNewItem();
            item.transform.position = _pos;
            item.Init(_text, _sprite, _aniName);
        }
        public static void Show_CamToCamera(string _text, Vector2 _pos, AniName _aniName) {
            if (!Myself)
                return;
            PopupTextItem item = Myself.GetAvailableItem();
            if (item == null)
                item = Myself.SpawnNewItem();
            _pos = RectTransformUtility.WorldToScreenPoint(Camera.main, _pos);
            _pos -= new Vector2(Screen.width / 2, Screen.height / 2);
            item.transform.position = _pos;
            item.Init(_text, _aniName);
        }
        public static void ShowOnly(string _text, Vector2 _pos, AniName _aniName) {
            if (!Myself)
                return;
            ClearAllItem();
            PopupTextItem item = Myself.GetAvailableItem();
            if (item == null)
                item = Myself.SpawnNewItem();
            item.transform.position = _pos;
            item.Init(_text, _aniName);
        }
        public PopupTextItem SpawnNewItem() {
            return MyPopupTextSpawner.Spawn<PopupTextItem>();
        }
        public PopupTextItem GetAvailableItem() {
            for (int i = 0; i < MyPopupTextSpawner.ItemList.Count; i++) {
                if (!MyPopupTextSpawner.ItemList[i].IsActive)
                    return MyPopupTextSpawner.ItemList[i];
            }
            return null;
        }
        public static void ClearAllItem() {
            for (int i = 0; i < Myself.MyPopupTextSpawner.ItemList.Count; i++) {
                Myself.MyPopupTextSpawner.ItemList[i].IsEndPlay();
            }
        }
    }
}
