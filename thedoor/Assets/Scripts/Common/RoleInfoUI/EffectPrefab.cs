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
        Camera Cam;
        Vector2 TipOffset;

        public bool IsActive { get; set; }

        public void SetData(StatusEffect _data, Camera _cam, Vector2 _tipOffset) {
            Cam = _cam;
            MyData = _data;
            TipOffset = _tipOffset;
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
            //顯示Tip
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Cam, transform.GetComponent<RectTransform>().position);
            TipUI.Instance.Show(MyData.Name, MyData.Description, screenPosition, TipOffset);
        }
    }
}