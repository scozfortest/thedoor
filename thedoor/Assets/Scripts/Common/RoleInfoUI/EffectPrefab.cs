using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;
using TMPro;

namespace TheDoor.Main {
    public class EffectPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;
        [SerializeField] TextMeshProUGUI StackText;


        StatusEffect MyData;

        public bool IsActive { get; set; }

        public void SetData(StatusEffect _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            StackText.text = MyData.Stack.ToString();
            gameObject.SetActive(false);
            AssetGet.GetSpriteFromAtlas("EffectIcon", MyData.MyType.ToString(), sprite => {
                gameObject.SetActive(true);
                Icon.sprite = sprite;
            });
        }
        public void OnClick() {
            //轉換為螢幕座標
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.GetComponent<RectTransform>().position);
            TipUI.Instance.Show(MyData.Name, MyData.Description, screenPosition, Vector2.up * 270);
        }
    }
}