using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;

namespace TheDoor.Main {
    /// <summary>
    /// 四方形的道具Prefab，獲得道具清單都是用這個顯示
    /// </summary>
    public class ItemPrefab : MonoBehaviour, IItem {

        [SerializeField] Image ItemImg;
        [SerializeField] Image FrameImg;
        [SerializeField] Image BottomImg;
        [SerializeField] Text NameText;
        [SerializeField] GameObject NameGO;
        [SerializeField] Text TypeText;
        [SerializeField] GameObject DisableConverGO;
        [SerializeField] Text DisableText;

        public bool IsActive { get; set; }
        /// <summary>
        /// 一般介面道具顯示用
        /// </summary>
        public void Init(ItemData _data) {
            if (_data == null)
                return;
            DisableConverGO.SetActive(false);
            Gray(false);
            TypeText.gameObject.SetActive(false);
            switch (_data.Type) {
                case ItemType.Gold:
                case ItemType.Point:
                case ItemType.Ball:
                    NameGO.SetActive(true);
                    AddressablesLoader.GetSpriteAtlas("CommonIcon", atlas => {
                        ItemImg.sprite = atlas.GetSprite(_data.Type.ToString());
                        ItemImg.SetNativeSize();
                    });
                    NameText.text = _data.Value.ToString();
                    AddressablesLoader.GetSpriteAtlas("CommonIcon", atlas => {
                        if (_data.Type == ItemType.Gold) {
                            FrameImg.sprite = atlas.GetSprite("IconFrame1");
                            BottomImg.sprite = atlas.GetSprite("IconFrameBot1");
                        } else if (_data.Type == ItemType.Ball) {
                            FrameImg.sprite = atlas.GetSprite("IconFrame2");
                            BottomImg.sprite = atlas.GetSprite("IconFrameBot2");
                        } else {
                            FrameImg.sprite = atlas.GetSprite("IconFrame3");
                            BottomImg.sprite = atlas.GetSprite("IconFrameBot3");
                        }
                    });
                    break;
                default:
                    var data = GameDictionary.GetItemJsonData(_data.Type, (int)_data.Value);
                    AddressablesLoader.GetSpriteAtlas("CommonIcon", atlas => {
                        FrameImg.sprite = atlas.GetSprite(string.Format("IconFrame{0}", data.Rank));
                        BottomImg.sprite = atlas.GetSprite(string.Format("IconFrameBot{0}", data.Rank));
                    });
                    NameGO.SetActive(false);
                    data.GetIconSprite(sprite => {
                        ItemImg.sprite = sprite;
                        ItemImg.SetNativeSize();
                    });

                    break;
            }
        }
        /// <summary>
        /// 獲得獎勵清單介面時使用，_replaced傳入true代表此道具因為已擁有被取代成其他道具了
        /// </summary>
        public void Init_GainItemsUI(ItemData _data, bool _replaced) {
            if (_data == null)
                return;
            Init(_data);
            Gray(false);
            if (_replaced) {
                DisableConverGO.SetActive(true);
                DisableText.text = StringData.GetUIString("Owned");
            }
            NameGO.SetActive(false);
            TypeText.gameObject.SetActive(true);
            switch (_data.Type) {
                case ItemType.Gold:
                case ItemType.Point:
                case ItemType.Ball:
                    TypeText.text = string.Format("{0}x{1}", StringData.GetUIString(_data.Type.ToString()), _data.Value.ToString());
                    break;
                default:
                    TypeText.text = StringData.GetUIString(string.Format("Item_{0}", _data.Type.ToString()));
                    break;
            }
        }
        public void Gray(bool _gray) {
            ItemImg.color = _gray ? Color.gray : Color.white;
            FrameImg.color = _gray ? Color.gray : Color.white;
            BottomImg.color = _gray ? Color.gray : Color.white;
            NameText.color = _gray ? Color.gray : Color.white;
        }
    }
}