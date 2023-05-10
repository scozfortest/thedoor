using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {
    public class RoleActionPrefab : MonoBehaviour, IItem {
        [SerializeField] Image Icon;

        public RoleAction MyData { get; private set; }

        public bool IsActive { get; set; }

        public void SetData(RoleAction _data) {
            MyData = _data;
            Refresh();
        }
        public void Refresh() {
            AssetGet.GetSpriteFromAtlas("RoleIcon", MyData.Doer.Ref, sprite => {
                Icon.sprite = sprite;
            });
        }
        public void OnClick() {
            //轉換為螢幕座標
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.GetComponent<RectTransform>().position);
            TipUI.Instance.Show(MyData.Name, "", screenPosition, new Vector2(80, 100));
        }
    }
}