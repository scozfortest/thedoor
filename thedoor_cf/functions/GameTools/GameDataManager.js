//Json
const ItemGroupJson = require('../GameData/ItemGroup.json');
const HandJson = require('../GameData/Hand.json');
const IconJson = require('../GameData/Icon.json');
const PlayerLVJson = require('../GameData/PlayerLV.json');
const RoleJson = require('../GameData/Role.json');
const StuffJson = require('../GameData/Stuff.json');
const EmojiJson = require('../GameData/Emoji.json');
const TitleJson = require('../GameData/Title.json');
const VoiceJson = require('../GameData/Voice.json');
const AIPlayerJson = require('../GameData/AIPlayer.json');
const VIPJson = require('../GameData/VIP.json');
const DailyRewardJson = require('../GameData/DailyReward.json');
const StringJson = require('../GameData/String.json');
const QuestJson = require('../GameData/Quest.json');

//自訂方法
const GameSetting = require('./GameSetting.js');
const ArrayTool = require('../Scoz/ArrayTool.js');
const Probability = require('../Scoz/Probability.js');
const FirestoreManager = require('../FirebaseTools/FirestoreManager.js');
const TextManager = require('../Scoz/TextManager.js');

module.exports = {

    //取得String
    GetStr: function (stringID, keyName) {
        let stringDatas = StringJson.String;
        for (let i = 0; i < stringDatas.length; i++) {
            if (stringDatas[i].ID == stringID) {
                if (keyName in stringDatas[i]) {
                    return stringDatas[i][keyName];
                }
                return keyName + " not found";
            }
        }
        let Logger = require('./Logger.js');
        let logStr = "String ID不存在表格中 ID: " + stringID;
        console.log(logStr);
        Logger.CFLog(logStr, GameSetting.LogTypes.Warn);
        return "";
    },
    //傳入Json檔名(GameSetting.GameJsonName)與ID取得該ID對應的資料
    GetData: function (jsonName, id) {
        let jsonData = GetJson(jsonName);

        if (jsonData == null)
            return null;

        for (let data of jsonData) {
            if (data["ID"] == id) {
                return data;
            }
        }

        let Logger = require('./Logger.js');
        let logStr = jsonName + "表不存在 ID: " + id;
        Logger.CFLog(logStr, GameSetting.LogTypes.Error);
        console.log(logStr);
        return null;
    },
    //傳入Json檔名(GameSetting.GameJsonName)取得該Json表所有ID清單
    GetJsonDataIDs: function (jsonName) {
        let aiIDs = [];
        let jsonData = GetJson(jsonName);
        if (jsonData == null)
            return aiIDs;

        for (let data of jsonData) {
            if ("ID" in data)
                aiIDs.push(data["ID"]);
        }
        return aiIDs;
    },
    //傳入Json表名稱取得JsonData
    GetJson: function (jsonName) {
        return GetJson(jsonName);
    },
    //傳入Json表名稱與Rank取得符合Rank的所有資料
    GetRankDatas: function (jsonName, rank) {
        let jsonData = GetJson(jsonName);

        if (jsonData == null)
            return null;

        let datas = [];

        for (let data of jsonData) {
            if (data["Rank"] == rank) {
                datas.push(data);
            }
        }
        return datas;
    },
    //傳入ItemGroupJsonData取得ItemDatas
    GetItemDatasFromItemGroup: async function (itemGroupJsonData, playerUID) {
        if (itemGroupJsonData == null)
            return [];
        let itemDatas = [];
        let curItemData = {};
        let itemGroupType = itemGroupJsonData["Type"];
        if (itemGroupType == "Random") {//如果是Random類型的寶箱，必須有填TypeValue
            if (!("TypeValue" in itemGroupJsonData) || Number(itemGroupJsonData["TypeValue"]) < 0) {
                let Logger = require('./Logger.js');
                let logStr = "Random類型寶箱沒填TypeValue 寶箱ID: " + itemGroupJsonData["TypeValue"];
                console.log(logStr);
                Logger.CFLog(logStr, GameSetting.LogTypes.Error);
                return itemDatas;
            }
        }
        for (let key in itemGroupJsonData) {
            if (key.includes("ItemType")) {
                curItemData = {};
                curItemData["ItemType"] = itemGroupJsonData[key];
            } else if (key.includes("ItemValue")) {
                curItemData["ItemValue"] = Number(itemGroupJsonData[key]);
                itemDatas.push(curItemData);
            }
            else if (key.includes("ItemWeight")) {
                curItemData["ItemWeight"] = Number(itemGroupJsonData[key]);
            }
        }

        switch (itemGroupType) {
            case "All"://全部獲得
                break;
            case "Random"://隨機獲得X個
                itemDatas = ArrayTool.GetArrayDictWhichDontContainKey(itemDatas, "ItemWeight");//如果是Random類型的寶箱，要把品項中沒填權重的移除
                let pickCount = Number(itemGroupJsonData["TypeValue"]);//要隨機出來的道具數量
                if (pickCount <= 0 || isNaN(pickCount)) //如果要取的道具數量資料填錯就返回空陣列
                    return [];
                if (itemDatas.length == 0)
                    return [];
                itemDatas = Probability.GetItemsByWeights(itemDatas, "ItemWeight", pickCount);//取得隨機數量商品
                break;
            case "RandomGold"://獲得範圍內隨機金幣數
            case "RandomPoint"://獲得範圍內隨機點數
            case "RandomBall"://獲得範圍內隨機小鋼珠數
                let rangeStrs = GetTypeValueRangeStrs(itemGroupJsonData["TypeValue"]);
                let randomValue = Probability.GetRandIntBetween(Number(rangeStrs[0]), Number(rangeStrs[1]));
                let currency = itemGroupType.replace('Random', '');
                itemDatas = [{
                    ItemType: GameSetting.CurrencyTypes[currency],
                    ItemValue: randomValue,
                }];
                break;
            case "RandomEmoji"://隨機獲得任一ID的表情貼圖，TypeValue填入亂數的品質、填入0代表不分品質
            case "RandomHand"://隨機獲得任一ID的瞇牌手，TypeValue填入亂數的品質、填入0代表不分品質
            case "RandomVoice"://隨機獲得任一ID的通用語音，TypeValue填入亂數的品質、填入0代表不分品質
            case "RandomIcon"://隨機獲得任一ID的頭像，TypeValue填入亂數的品質、填入0代表不分品質
                let rank = Number(itemGroupJsonData["TypeValue"]);
                let randomItemType = itemGroupType.replace('Random', '');
                let randomItemID = 0;
                if (rank == 0) {
                    let ids = this.GetJsonDataIDs(randomItemType);
                    if (ids.length > 0)
                        randomItemID = ids[Probability.GetRandInt(ids.length)];
                } else {
                    let rankDatas = this.GetRankDatas(randomItemType, rank);
                    if (rankDatas.length > 0)
                        randomItemID = rankDatas[Probability.GetRandInt(rankDatas.length)]["ID"];
                }
                //console.log("rank=" + rank);
                //console.log("randomItemType=" + randomItemType);
                //console.log("randomItemID=" + randomItemID);
                if (typeof (randomItemID) == "undefined" && randomItemID == null) {
                    itemDatas = [];
                    let Logger = require('./Logger.js');
                    let logStr = "ItemGroup的RandomItem系列有填錯資料 或 對應獲得道具表資料不存在 ItemGroupID=" + itemGroupJsonData["ID"];
                    Logger.CFLog(logStr, GameSetting.LogTypes.Error);
                    console.log(logStr);
                    return;
                } else {
                    itemDatas = [{
                        ItemType: randomItemType,
                        ItemValue: Number(randomItemID),
                    }];
                }
                break;
            case "TenDrawSet"://十抽固定品項獎勵，只能填10個品項，十抽抽出來固定獲得這10個道具
                if (itemDatas.length != 10) {
                    itemDatas = [];
                    let Logger = require('./Logger.js');
                    let logStr = "ItemGroup表的TenDrawSet數量不為10個";
                    Logger.CFLog(logStr, GameSetting.LogTypes.Error);
                    console.log(logStr);
                    return;
                }
                break;
            case "ScratchCard"://隨機獲得1個且排除玩家已經擁有的不能重複的道具
                itemDatas = ArrayTool.GetArrayDictWhichDontContainKey(itemDatas, "ItemWeight");//如果是Random類型的寶箱，要把品項中沒填權重的移除
                let newGainItems = Array.from(itemDatas);//深複製陣列

                let playerItemDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, playerUID);//取得玩家擁有的道具清單
                let ownedUniqueItemDatas = {};//玩家擁有的獨立類道具id清單
                for (let itemData of itemDatas) {
                    if (itemData["ItemType"] == "ItemGroup") {
                        continue;
                    }

                    if (itemData["ItemType"] in GameSetting.CurrencyTypes) {//資源類道具重複沒關係，所以不處理
                    } else if (itemData["ItemType"] in GameSetting.UniqueItemTypes) {//獨立資料類道具


                        //取得玩家擁有的獨立類道具ID清單
                        if (!(itemData["ItemType"] in ownedUniqueItemDatas)) {
                            ownedUniqueItemDatas[itemData["ItemType"]] = [];
                            let ownedUniqueItemDocs = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols[itemData["ItemType"]], "OwnerUID", playerUID);
                            if (ownedUniqueItemDocs != null) {
                                for (let doc of ownedUniqueItemDocs) {
                                    ownedUniqueItemDatas[itemData["ItemType"]].push(doc.data()["ID"]);
                                }
                            }
                        }

                        if (ownedUniqueItemDatas[itemData["ItemType"]].includes(itemData["ItemValue"])) {//如果已經有此道具就替換道具
                            ArrayTool.RemoveItem(newGainItems, itemData);//移除被取代的道具
                        } else {
                            ownedUniqueItemDatas[itemData["ItemType"]].push(itemData["ItemValue"]);//如果本來沒此道具就設定玩家已經擁有此道具了
                        }

                    } else {//非獨立資料類

                        let needReplaceDuplicatedItem = true;
                        if (playerItemDocData == null) {
                            playerItemDocData = {};
                            needReplaceDuplicatedItem = false;//玩家本來就沒有道具資料就不替換道具
                        }
                        if (!(itemData["ItemType"] in playerItemDocData)) {
                            playerItemDocData[itemData["ItemType"]] = {};
                            needReplaceDuplicatedItem = false;//玩家本來就沒有道具資料就不替換道具
                        }


                        if (!(itemData["ItemValue"] in playerItemDocData[itemData["ItemType"]])) {
                            needReplaceDuplicatedItem = false;//玩家本來就沒有道具資料就不替換道具
                        }
                        //判斷是否為Stuff道具且是否可堆疊，是可堆疊道具就代表能重複獲得，因此不替換道具
                        if (needReplaceDuplicatedItem) {
                            if (itemData["ItemType"] == GameSetting.ItemTypes.Stuff) {
                                let stuffJsonData = this.GetData(GameSetting.GameJsonNames.Stuff, itemData["ItemValue"]);
                                let stackable = false;
                                if ("Stackable" in stuffJsonData)
                                    stackable = TextManager.ToBoolean(stuffJsonData["Stackable"]);
                                if (stackable)
                                    needReplaceDuplicatedItem = false;//可堆疊道具就不替換道具
                            }
                        }

                        if (needReplaceDuplicatedItem) {
                            ArrayTool.RemoveItem(newGainItems, itemData);//移除被取代的道具

                        } else {
                            playerItemDocData[itemData["ItemType"]][itemData["ItemValue"]] = 1;//玩家擁有的此道具數量設為1
                        }
                    }
                }

                if (newGainItems.length == 0)
                    return [];
                itemDatas = Probability.GetItemsByWeights(newGainItems, "ItemWeight", 1);//取得隨機數量商品
                break;
            default:
                itemDatas = [];
                let Logger = require('./Logger.js');
                let logStr = "ItemGroupID=" + itemGroupJsonData["ID"] + "有未定義的ItemGroupType: " + itemGroupType;
                Logger.CFLog(logStr, GameSetting.LogTypes.Error);
                console.log(logStr);
                return;
        }

        //箱中箱處理
        //1. 透過遞迴把每個寶箱取出的itemData陣列存起來
        let itemDatasArray = [];
        for (let itemData of itemDatas) {
            if (itemData["ItemType"] == "ItemGroup") {
                let itemGroupJsonData = this.GetData(GameSetting.GameJsonNames.ItemGroup, itemData["ItemValue"]);
                if (itemGroupJsonData != null) {
                    let addItemDatas = await this.GetItemDatasFromItemGroup(itemGroupJsonData, playerUID);//遞迴
                    if (addItemDatas != null && addItemDatas.length > 0)
                        itemDatasArray.push(addItemDatas);
                }
            }
        }

        //2. 把所有寶箱打開的ItemData陣列合併
        for (let addItemDatas of itemDatasArray) {
            itemDatas = itemDatas.concat(addItemDatas);
        }
        //3. 把所有ItemGroup類的Item移除
        itemDatas = itemDatas.filter(a => a.ItemType != "ItemGroup");

        /*
        for (let itemData of itemDatas) {
            console.log(itemData["ItemType"] + ": " + itemData["ItemValue"]);
        }
        */

        return itemDatas;
    },

}
//傳入ItemGroup中TypeValue，取得陣列(若資料填錯就回傳null) Ex.5~8就會回傳 rangeStrs[0]="5" rangeStrs[1]="8"
function GetTypeValueRangeStrs(typeValueStr) {
    let rangeStrs = typeValueStr.split('~');
    if (rangeStrs.length == 2) {
        if (isNaN(rangeStrs[0]) || isNaN(rangeStrs[1])) {//如果要取的資源數量範圍資料填錯就返回空陣列
            return null;
        }
    } else if (rangeStrs.length == 1) {
        if (isNaN(rangeStrs[0])) {//如果要取的資源數量範圍資料填錯就返回空陣列
            return null;
        }
    } else {//如果要取的資源數量範圍資料填錯就返回空陣列
        return null;
    }
    if (Number(rangeStrs[0]) >= Number(rangeStrs[1]))//如果要取的資源數量範圍資料填錯就返回空陣列
        return null;

    return rangeStrs;
}
//傳入Json表名稱取得Json
function GetJson(jsonName) {
    let jsonData = null;
    switch (jsonName) {
        case GameSetting.GameJsonNames.ItemGroup://寶箱表
            jsonData = ItemGroupJson.ItemGroup;
            break;
        case GameSetting.GameJsonNames.Role://腳色表
            jsonData = RoleJson.Role;
            break;
        case GameSetting.GameJsonNames.Hand://瞇牌手表
            jsonData = HandJson.Hand;
            break;
        case GameSetting.GameJsonNames.Icon://頭貼表
            jsonData = IconJson.Icon;
            break;
        case GameSetting.GameJsonNames.Emoji://頭貼表
            jsonData = EmojiJson.Emoji;
            break;
        case GameSetting.GameJsonNames.Stuff://物品表
            jsonData = StuffJson.Stuff;
            break;
        case GameSetting.GameJsonNames.Title://稱號表
            jsonData = TitleJson.Title;
            break;
        case GameSetting.GameJsonNames.VIP://VIP表
            jsonData = VIPJson.VIP;
            break;
        case GameSetting.GameJsonNames.Voice://通用語音表
            jsonData = VoiceJson.Voice;
            break;
        case GameSetting.GameJsonNames.AIPlayer://AI玩家表
            jsonData = AIPlayerJson.AIPlayer;
            break;
        case GameSetting.GameJsonNames.DailyReward://簽到簿
            jsonData = DailyRewardJson.DailyReward;
            break;
        case GameSetting.GameJsonNames.PlayerLv://生涯獎勵表
            jsonData = PlayerLVJson.PlayerLV;
            break;
        case GameSetting.GameJsonNames.Quest://任務表
            jsonData = QuestJson.Quest;
            break;
        default:
            console.log("尚未定義Json表: " + jsonName);
            break;
    }
    if (jsonData == null || jsonData === undefined) {
        let Logger = require('./Logger.js');
        let logStr = "找不到或未定義json檔: " + jsonName;
        Logger.CFLog(logStr, GameSetting.LogTypes.Error);
        console.log(logStr);
    }
    return jsonData;
}