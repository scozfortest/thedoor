using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using TheDoor.Main;
using UnityEngine;

namespace TheDoor.Main {
    /// <summary>
    /// 應該是Server處理的東西放在這裡執行
    /// </summary>
    public class LocoServerManager {

        /// <summary>
        /// /建立玩家
        /// </summary>
        public static void CreatePlayer() {
            WriteLog.Log("<color=Orange>創新本地帳號</color>");
            //設定玩家資料
            Dictionary<string, object> playerDataDic = new Dictionary<string, object>();
            playerDataDic.Add("UID", "LocalGuest");
            playerDataDic.Add("CreateTime", GameManager.Instance.NowTime);
            playerDataDic.Add("OwnedCurrency", new Dictionary<Currency, long> { { Currency.Gold, 100 }, { Currency.Point, 0 } });
            playerDataDic.Add("AuthType", AuthType.NotSigninYet);
            playerDataDic.Add("CurRoleUID", "");
            playerDataDic.Add("TotalPurchase", 0);
            playerDataDic.Add("Ban", false);
            playerDataDic.Add("DeviceUID", SystemInfo.deviceUniqueIdentifier);
            GamePlayer.Instance.SetMainPlayerData(playerDataDic);
            GamePlayer.Instance.SaveToLoco_PlayerData();
        }

        /// <summary>
        /// 建立冒險
        /// </summary>
        public static void CreateAdventure() {
            List<ItemData> gainItems = new List<ItemData>();

            //腳色
            var roleData = RoleData.GetRandAvailableData();

            //道具
            var rndSuppies = SupplyData.GetRndDatas(3);
            var exclusiveSupplies = new List<SupplyData>();
            foreach (var id in roleData.Supplies) {
                var supplyData = SupplyData.GetData(id);
                exclusiveSupplies.Add(supplyData);
            }

            foreach (var supplyData in rndSuppies) {
                gainItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }
            foreach (var supplyData in exclusiveSupplies) {
                gainItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }


            //設定腳色資料
            Dictionary<string, object> roleDataDic = new Dictionary<string, object>();
            roleDataDic.Add("UID", GamePlayer.Instance.GetNextUID("Role"));
            roleDataDic.Add("OwnerUID", GamePlayer.Instance.Data.UID);
            roleDataDic.Add("CreateTime", GameManager.Instance.NowTime);

            roleDataDic.Add("ID", roleData.ID);
            roleDataDic.Add("CurHP", roleData.HP);
            roleDataDic.Add("CurSanP", roleData.SanP);
            GamePlayer.Instance.SetOwnedData<OwnedRoleData>(ColEnum.Role, roleDataDic);
            GamePlayer.Instance.SaveToLoco_RoleData();

            GamePlayer.Instance.Data.SetCurRole_Loco(roleDataDic["UID"].ToString());
            GamePlayer.Instance.SaveToLoco_PlayerData();

        }
    }
}