using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class RoleData : MyJsonData, IItemJsonData {
        public static string DataName { get; private set; }
        public string Name {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Name");
            }
        }
        public string NotificationTitle {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "NotificationTitle");
            }
        }
        public string NotificationContent {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "NotificationContent");
            }
        }
        public string TypeName {
            get {
                return StringData.GetUIString("Item_" + DataName);
            }
        }
        public ItemType MyItemType { get; } = ItemType.Role;
        public string Description {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Description");
            }
        }
        public bool Enable { get; private set; } = false;

        public string RefImg { get; private set; }
        public string RefParticle { get; private set; }
        public int Rank { get; private set; }
        public static int MaxRank { get; private set; } = 0;
        HashSet<int> RolePlotGroupIDs;
        ItemData MyItemData;

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            ItemType itemType;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "RefImg":
                        RefImg = item[key].ToString();
                        break;
                    case "RefParticle":
                        RefParticle = item[key].ToString();
                        break;
                    case "Rank":
                        Rank = int.Parse(item[key].ToString());
                        MaxRank = Mathf.Max(Rank, MaxRank);
                        break;
                    case "RolePlotGroupIDs":
                        RolePlotGroupIDs = TextManager.StringSplitToIntHashSet(item[key].ToString(), ',');
                        break;
                    case "Enable":
                        Enable = bool.Parse(item[key].ToString());
                        break;
                    case "ItemValue":
                        if (MyEnum.TryParseEnum(item["ItemType"].ToString(), out itemType)) {
                            MyItemData = new ItemData(itemType, long.Parse(item[key].ToString()));
                        } else
                            DebugLogger.LogErrorFormat("{0}表的 ID:{1} 的ItemType填錯: {2}", DataName, ID, item["ItemType"]);
                        break;
                }
            }
        }
        /// <summary>
        /// 取得腳色BlurBG
        /// </summary>
        public void GetBlurBGSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite("Role/" + RefImg + "/BlurBG", (sprite, handle) => {
                _ac?.Invoke(sprite);
            });
        }
        /// <summary>
        /// 取得腳色大圖Sprite
        /// </summary>
        public static void GetRoleSprite(int _id, int _index, Action<Sprite> _ac) {
            RoleData data = GetData(_id);
            if (data == null)
                _ac?.Invoke(null);
            data.GetRoleSprite(_index, _ac);
        }
        /// <summary>
        /// 取得腳色大圖Sprite傳入1~4
        /// </summary>
        public void GetRoleSprite(int _index, Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite("Role/" + RefImg + "/Role" + _index, (sprite, handle) => {
                _ac?.Invoke(sprite);
            });
        }
        /// <summary>
        /// 取得腳色背景Sprite
        /// </summary>
        public static void GetRoleBGSprite(int _id, Action<Sprite> _ac) {
            RoleData data = GetData(_id);
            if (data == null)
                _ac?.Invoke(null);
            data.GetRoleBGSprite(_ac);
        }
        /// <summary>
        /// 取得腳色大圖Sprite
        /// </summary>
        public void GetRoleBGSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite("Role/" + RefImg + "/RoleBG", (sprite, handle) => {
                _ac?.Invoke(sprite);
            });
        }
        /// <summary>
        /// 取得腳色特效背景圖
        /// </summary>
        public void GetEffectBGSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite("Role/" + RefImg + "/RoleEffect", (sprite, handle) => {
                _ac?.Invoke(sprite);
            });
        }
        /// <summary>
        /// 取得腳色動作圖index傳入1~7
        /// </summary>
        public void GetPostureSprite(int _index, Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite("Role/" + RefImg + "/RolePosture" + _index, (sprite, handle) => {
                _ac?.Invoke(sprite);
            });
        }

        /// <summary>
        /// 取得腳色Icon
        /// </summary>
        public static void GetIconSprite(int _id, Action<Sprite> _ac) {
            RoleData data = GetData(_id);
            if (data == null)
                _ac?.Invoke(null);
            data.GetIconSprite(_ac);
        }
        /// <summary>
        /// 取得腳色Icon
        /// </summary>
        public void GetIconSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSpriteAtlas(DataName + "Icon", atlas => {
                if (atlas != null) {
                    Sprite sprite = atlas.GetSprite(RefImg);
                    _ac?.Invoke(sprite);
                }
            });
        }
        /// <summary>
        /// 取得腳色小圖Sprite
        /// </summary>
        public static void GetRoleSmallSprite(int _id, Action<Sprite> _ac) {
            RoleData data = GetData(_id);
            if (data == null)
                _ac?.Invoke(null);
            data.GetRoleSmallSprite(_ac);
        }
        /// <summary>
        /// 取得腳色小圖Sprite
        /// </summary>
        public void GetRoleSmallSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSpriteAtlas(DataName + "Small", atlas => {
                if (atlas != null) {
                    Sprite sprite = atlas.GetSprite(RefImg);
                    _ac?.Invoke(sprite);
                }
            });
        }
        /// <summary>
        /// 取得腳色碎片
        /// </summary>
        public void GetRoleFragmentSprite(int _stuffDataID, Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg)) {
                _ac?.Invoke(null);
                return;
            }
            AddressablesLoader.GetSpriteAtlas(DataName + "Fragment", atlas => {
                if (atlas != null) {
                    Sprite sprite = atlas.GetSprite(_stuffDataID.ToString());
                    _ac?.Invoke(sprite);
                }
            });
        }
        public static RoleData GetData(int _id) {
            return GameDictionary.GetJsonData<RoleData>(DataName, _id);
        }
        public int GetRandomRoleGroupID() {
            if (RolePlotGroupIDs == null || RolePlotGroupIDs.Count == 0)
                return 0;
            int randGroupID = Probability.GetRandomTFromTHashSet(RolePlotGroupIDs);
            return randGroupID;
        }
    }

}
