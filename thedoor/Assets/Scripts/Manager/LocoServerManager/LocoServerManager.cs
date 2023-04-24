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
            List<ItemData> defaultItems = new List<ItemData>();
            List<ItemData> exclusiveItems = new List<ItemData>();
            List<ItemData> inheritItems = new List<ItemData>();

            //設定腳色資料
            var roleData = RoleData.GetRandAvailableData();
            Dictionary<string, object> roleDataDic = new Dictionary<string, object>();
            roleDataDic.Add("UID", GamePlayer.Instance.GetNextUID("Role"));
            roleDataDic.Add("OwnerUID", GamePlayer.Instance.Data.UID);
            roleDataDic.Add("CreateTime", GameManager.Instance.NowTime);

            roleDataDic.Add("ID", roleData.ID);
            roleDataDic.Add("CurHP", roleData.HP);
            roleDataDic.Add("CurSanP", roleData.SanP);
            GamePlayer.Instance.SetOwnedData<OwnedRoleData>(ColEnum.Role, roleDataDic);

            //設定道具資料
            var defaultSuppies = SupplyData.GetRndDatas(3);
            var exclusiveSupplies = new List<SupplyData>();
            foreach (var id in roleData.Supplies) {
                var supplyData = SupplyData.GetData(id);
                exclusiveSupplies.Add(supplyData);
            }
            foreach (var supplyData in defaultSuppies) {
                defaultItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }
            foreach (var supplyData in exclusiveSupplies) {
                exclusiveItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }


            List<Dictionary<string, object>> supplyListDic = new List<Dictionary<string, object>>();
            List<SupplyData> tmpSupplyDatas = new List<SupplyData>();
            tmpSupplyDatas.AddRange(defaultSuppies);
            tmpSupplyDatas.AddRange(exclusiveSupplies);
            foreach (var data in tmpSupplyDatas) {
                Dictionary<string, object> supplyDataDic = new Dictionary<string, object>();
                supplyDataDic.Add("UID", GamePlayer.Instance.GetNextUID("Supply"));
                supplyDataDic.Add("OwnerUID", GamePlayer.Instance.Data.UID);
                supplyDataDic.Add("CreateTime", GameManager.Instance.NowTime);

                supplyDataDic.Add("ID", data.ID);
                supplyDataDic.Add("Usage", data.Usage);
                supplyDataDic.Add("OwnRoleUID", GamePlayer.Instance.Data.CurRoleUID);
                supplyListDic.Add(supplyDataDic);
            }
            GamePlayer.Instance.SetOwnedDatas<OwnedSupplyData>(ColEnum.Supply, supplyListDic);


            //設定玩家資料
            GamePlayer.Instance.Data.SetCurRole_Loco(roleDataDic["UID"].ToString());

            //設定冒險資料
            string doorTypeWeightJson = GameSettingData.GetStr(GameSetting.Adventure_DoorTypeWeight);
            Debug.LogError(doorTypeWeightJson);

            //存本地資料
            GamePlayer.Instance.SaveSettingToLoco();
            GamePlayer.Instance.SaveToLoco_PlayerData();
            GamePlayer.Instance.SaveToLoco_RoleData();
            GamePlayer.Instance.SaveToLoco_SupplyData();

            //設定UI
            CreateRoleUI.GetInstance<CreateRoleUI>().SetGainItemList(exclusiveItems, defaultItems, inheritItems);

        }
    }
}