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


            //設定玩家資料
            Dictionary<string, object> historyDataDic = new Dictionary<string, object>();
            historyDataDic.Add("UID", playerDataDic["UID"]);
            historyDataDic.Add("CreateTime", GameManager.Instance.NowTime);
            historyDataDic.Add("InheritSupplies", null);
            GamePlayer.Instance.SetOwnedData<OwnedHistoryData>(ColEnum.History, historyDataDic);

            //存本地資料
            GamePlayer.Instance.SaveToLoco_PlayerData();
            GamePlayer.Instance.SaveToLoco_HistoryData();
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
            Dictionary<string, object> roleDataDic = roleData.GetJsonDataDic();
            Debug.LogError(roleDataDic["Talents"]);
            GamePlayer.Instance.SetOwnedDatas<OwnedRoleData>(ColEnum.Role, new List<Dictionary<string, object>>() { roleDataDic });
            //設定玩家資料
            GamePlayer.Instance.Data.SetCurRole_Loco(roleDataDic["UID"].ToString());

            //設定道具資料
            int defaultSupplyCount = GameSettingData.GetInt(GameSetting.Role_DefaultSupplyCount);
            var defaultSuppies = SupplyData.GetRndDatas(defaultSupplyCount, 1);
            var exclusiveSupplies = new List<SupplyData>();
            var inheritSupplies = new List<SupplyData>();
            foreach (var id in roleData.Supplies) {
                var supplyData = SupplyData.GetData(id);
                exclusiveSupplies.Add(supplyData);
            }
            foreach (var id in GamePlayer.Instance.MyHistoryData.InheritSupplies) {
                var supplyData = SupplyData.GetData(id);
                inheritSupplies.Add(supplyData);
            }
            GamePlayer.Instance.MyHistoryData.ClearInheritSupplies();

            foreach (var supplyData in defaultSuppies) {
                defaultItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }
            foreach (var supplyData in exclusiveSupplies) {
                exclusiveItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }
            foreach (var supplyData in inheritSupplies) {
                inheritItems.Add(new ItemData(ItemType.Supply, supplyData.ID));
            }


            List<Dictionary<string, object>> supplyListDic = new List<Dictionary<string, object>>();
            List<SupplyData> tmpSupplyDatas = new List<SupplyData>();
            tmpSupplyDatas.AddRange(defaultSuppies);
            tmpSupplyDatas.AddRange(exclusiveSupplies);
            tmpSupplyDatas.AddRange(SupplyData.GetRoleUnarmedDatas(roleData));//獲得腳色肉搏道具

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

            //建立冒險用腳色資料
            AdventureManager.CreatePlayerRole();

            //存本地資料
            GamePlayer.Instance.SaveSettingToLoco();
            GamePlayer.Instance.SaveToLoco_HistoryData();
            GamePlayer.Instance.SaveToLoco_PlayerData();
            GamePlayer.Instance.SaveToLoco_RoleData();
            GamePlayer.Instance.SaveToLoco_SupplyData();
            GamePlayer.Instance.SaveToLoco_AdventureData();

            //設定UI
            CreateRoleUI.Instance.SetGainItemList(exclusiveItems, defaultItems, inheritItems);

        }

        /// <summary>
        /// 移除腳色
        /// </summary>
        public static void RemoveCurUseRole() {
            //移除冒險
            GamePlayer.Instance.RemoveOwnedData(ColEnum.Adventure, GamePlayer.Instance.Data.CurRole.MyAdventure.UID);
            //移除道具並取隨機道具作為繼承道具
            var ownedSupplies = GamePlayer.Instance.Data.CurRole.GetSupplyDatas(new HashSet<string> { "Unarmed" });
            //移除道具
            foreach (var data in ownedSupplies) {
                GamePlayer.Instance.RemoveOwnedData(ColEnum.Supply, data.UID);
            }
            //取隨機道具
            var getRndSupplies = Prob.GetRandNoDuplicatedTFromTList(ownedSupplies, GameSettingData.GetInt(GameSetting.Role_InheritSupplyCount));
            List<int> inheritSupplyIDs = getRndSupplies.ConvertAll(a => a.ID);
            var ownedHistoryData = GamePlayer.Instance.MyHistoryData;
            ownedHistoryData.AddInheritedSupply(inheritSupplyIDs);
            //移除腳色
            GamePlayer.Instance.RemoveOwnedData(ColEnum.Role, GamePlayer.Instance.Data.CurRoleUID);
            GamePlayer.Instance.Data.SetCurRole_Loco("");



            //存本地資料
            GamePlayer.Instance.SaveSettingToLoco();
            GamePlayer.Instance.SaveToLoco_HistoryData();
            GamePlayer.Instance.SaveToLoco_PlayerData();
            GamePlayer.Instance.SaveToLoco_RoleData();
            GamePlayer.Instance.SaveToLoco_SupplyData();
            GamePlayer.Instance.SaveToLoco_AdventureData();
        }


    }
}