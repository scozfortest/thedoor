using Newtonsoft.Json;
using Scoz.Func;
using SimpleJSON;
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
            GamePlayer.Instance.SetOwnedDatas<OwnedRoleData>(ColEnum.Role, new List<Dictionary<string, object>>() { roleDataDic });

            //設定道具資料
            int defaultSupplyCount = GameSettingData.GetInt(GameSetting.Role_DefaultSupplyCount);
            var defaultSuppies = SupplyData.GetRndDatas(defaultSupplyCount);
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
            var doorCount = GameSettingData.GetJsNode(GameSetting.Adventure_DoorCount);
            var doorTypeWeight = GameSettingData.GetJsNode(GameSetting.Adventure_DoorTypeWeight);
            List<DoorData> doorDatas = new List<DoorData>();
            doorDatas.Add(new DoorData(DoorType.Start));
            for (int i = 0; i < doorCount; i++) {
                DoorType rndDoorType = MyEnum.ParseEnum<DoorType>(Prob.GetRandomKeyFromJsNodeKeyWeight(doorTypeWeight));
                doorDatas.Add(new DoorData(rndDoorType));
            }
            doorDatas.Add(new DoorData(DoorType.Boss));

            Dictionary<string, object> adventureDataDic = new Dictionary<string, object>();
            adventureDataDic.Add("UID", GamePlayer.Instance.GetNextUID("Adventure"));
            adventureDataDic.Add("OwnerUID", GamePlayer.Instance.Data.UID);
            adventureDataDic.Add("CreateTime", GameManager.Instance.NowTime);

            adventureDataDic.Add("CurDoorIndex", 0);
            adventureDataDic.Add("OwnRoleUID", GamePlayer.Instance.Data.CurRoleUID);
            List<object> doorListDic = new List<object>();
            foreach (var data in doorDatas) {
                Dictionary<string, object> doorDataDic = new Dictionary<string, object>();
                doorDataDic.Add("DoorType", data.DoorType);
                doorDataDic.Add("Values", data.Values);
                doorListDic.Add(doorDataDic);
            }
            adventureDataDic.Add("Doors", doorListDic);

            GamePlayer.Instance.SetOwnedDatas<OwnedAdventureData>(ColEnum.Adventure, new List<Dictionary<string, object>>() { adventureDataDic });


            //存本地資料
            GamePlayer.Instance.SaveSettingToLoco();
            GamePlayer.Instance.SaveToLoco_PlayerData();
            GamePlayer.Instance.SaveToLoco_RoleData();
            GamePlayer.Instance.SaveToLoco_SupplyData();
            GamePlayer.Instance.SaveToLoco_AdventureData();

            //設定UI
            CreateRoleUI.GetInstance<CreateRoleUI>().SetGainItemList(exclusiveItems, defaultItems, inheritItems);

        }
    }
}