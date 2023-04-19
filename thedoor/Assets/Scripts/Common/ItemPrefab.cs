using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using TMPro;

namespace TheDoor.Main {
    /// <summary>
    /// 四方形的道具Prefab，獲得道具清單都是用這個顯示
    /// </summary>
    public class ItemPrefab : MonoBehaviour, IItem {

        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] TextMeshProUGUI DescriptionText;
        [SerializeField] Image ItemImg;
        [SerializeField] Image BottomImg;

        public bool IsActive { get; set; }

        public void Show(ItemData _data) {
            if (_data == null)
                return;
            switch (_data.Type) {
                case ItemType.Gold:
                case ItemType.Point:
                    NameText.text = string.Format(StringData.GetUIString("GainCurrencyName"), StringData.GetUIString(_data.Type.ToString()), _data.Value);
                    DescriptionText.text = StringData.GetUIString(_data.Type.ToString() + "Description");
                    AssetGet.GetIconFromAtlas("Common", _data.Type.ToString(), sprite => {
                        ItemImg.sprite = sprite;
                    });
                    //AddressablesLoader.GetSpriteAtlas("CommonIcon", atlas => {
                    //    BottomImg.sprite = atlas.GetSprite(string.Format("IconFrameBot{0}", data.Rank));
                    //});
                    break;
                default:
                    var data = GameDictionary.GetItemJsonData(_data.Type, (int)_data.Value);
                    NameText.text = data.Name;
                    DescriptionText.text = data.Description;
                    AssetGet.GetIconFromAtlas(_data.Type.ToString(), data.Ref.ToString(), sprite => {
                        ItemImg.sprite = sprite;
                    });
                    //AddressablesLoader.GetSpriteAtlas("CommonIcon", atlas => {
                    //    BottomImg.sprite = atlas.GetSprite(string.Format("IconFrameBot{0}", data.Rank));
                    //});
                    break;
            }
        }
    }
}