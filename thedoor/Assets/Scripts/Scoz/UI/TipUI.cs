using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using UnityEngine.UI;
using TMPro;
namespace Scoz.Func {

    public class TipUI : BaseUI {

        [HeaderAttribute("==============設定==============")]
        [SerializeField] GameObject TitleGO;
        [SerializeField] GameObject ContentGO;
        [SerializeField] TextMeshProUGUI TitleText;
        [SerializeField] TextMeshProUGUI ContentText;
        [SerializeField] ContentSizeFitter[] ContentSizeFitters;

        public static TipUI Instance { get; private set; }
        static bool IsShowing = true;

        public override void Init() {
            base.Init();
            SetActive(false);
            Instance = this;
        }
        void Update() {
            if (!IsShowing) return;
            if (Input.GetMouseButtonDown(0)) {
                Hide();
            }
        }
        public void Show(string _title, string _content, Vector2 _screenPos, Vector2 _offset) {
            //標題
            TitleGO.SetActive(!string.IsNullOrEmpty(_title));
            TitleText.text = _title;
            //內文
            ContentGO.SetActive(!string.IsNullOrEmpty(_content));
            ContentText.text = _content;


            //螢幕座標轉Canvas座標
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), _screenPos, null, out Vector2 localPos);



            transform.localPosition = localPos + _offset;
            ContentSizeFitters.Update();
            SetActive(true);
            IsShowing = true;
        }
        public void Hide() {
            IsShowing = false;
            SetActive(false);
        }
    }
}
