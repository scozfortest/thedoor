const admin = require('firebase-admin');
//Json


//Tools
const Probability = require('../Scoz/Probability.js');
const TextManager = require('../Scoz/TextManager.js');
const GameDataManager = require('./GameDataManager.js');
const FirestoreManager = require('../FirebaseTools/FirestoreManager.js');
const GameSetting = require('./GameSetting');
const ArrayTool = require('../Scoz/ArrayTool.js');
const Logger = require('./Logger');

var methods = {
    //給資源
    GiveCurrency: async function (currencyType, value, playerUID) {
        if (!(currencyType in GameSetting.CurrencyTypes)) {
            console.log("貨幣類型錯誤" + currencyType);
            return;
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, playerUID, {
            [currencyType]: admin.firestore.FieldValue.increment(value),
        })
    },
    //給複數種類資源
    GiveCurrencies: async function (currencyTypes, values, playerUID) {
        let updatePlayerData = {};
        for (let i = 0; i < currencyTypes.length; i++) {
            if (!(currencyTypes[i] in GameSetting.CurrencyTypes)) {
                console.log("貨幣類型錯誤" + currencyType);
                return;
            }
            updatePlayerData[currencyTypes[i]] = admin.firestore.FieldValue.increment(values[i]);
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, playerUID, updatePlayerData)
    },

    //傳入[ItemType,ItemValue, 數量, 玩家UID來給予玩家道具,被取代的道具清單]
    GiveItem: async function (itemType, itemValue, count, playerUID, replaceGainItems) {

        let returnGainItems = [];//最終真實獲得的道具清單


        //檢查道具類型
        if (!(itemType in GameSetting.ItemTypes)) {
            console.log("錯誤的道具類型: " + itemType);
        }

        if (itemType == "ItemGroup") {//寶箱類的道具要轉給GiveItem_ItemGroup處理
            await this.GiveItem_ItemGroup(returnGainItems, itemValue, count, playerUID, replaceGainItems);
            return returnGainItems;
        }

        if (itemType in GameSetting.CurrencyTypes) {//資源類
            itemValue = itemValue * count;//資源類道具可以直接乘上數量就是獲得資源量
            let updatePlayerData = { [itemType]: admin.firestore.FieldValue.increment(itemValue) };
            //更新資料庫
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, playerUID, updatePlayerData);

            //設定回傳獲得道具資料
            returnGainItems.push({
                ItemType: itemType,
                ItemValue: itemValue,
            });

        } else {//非資源類

            //因為非資源類的道具就要考慮重複獲得的問題，因此會跑GetReplacedDuplicatedItems這個func來檢測是否有重複獲得道具
            //有重複獲得道具時GetReplacedDuplicatedItems會依照ItemGroup表來取代為替換道具，有可能是複數道具，所以還是會定義以下獲得類型資料
            let addCurrency = {};//將要增加的資源記錄在這個字典，之後會更新資料庫
            let batchAddUniqueItemDatas = [];//將要加入的獨立資料類道具加到這個陣列，之後會送批次更新資料庫
            let addNotUniqueItem = {};//將要增加的非獨立資料類道具數量記錄在這個字典，之後會更新資料庫


            let gainItems = [];
            //用迴圈來計算獲取道具清單
            for (let i = 0; i < count; i++) {
                gainItems.push({
                    ItemType: itemType,
                    ItemValue: itemValue,
                });
            }
            //將獲得的道具中有重複的道具轉為替代道具並回傳最終獲得的道具清單
            gainItems = await GetReplacedDuplicatedItems(playerUID, gainItems, replaceGainItems);


            //依據要獲得的道具清單(ItemDatas)來設定要批次寫入資料庫的資料(addCurrency,batchAddUniqueItemDatas,addNotUniqueItem)
            await SetBatchWriteDatasFromItemDatas(playerUID, gainItems, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem);

            await WriteBatchDataToFirestore(returnGainItems, playerUID, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem);//將批次資料寫入資料庫
        }

        return returnGainItems;
    },
    //傳入[最終獲得的道具清單, ItemGroupID, 數量, 玩家UID,未取代重複道具前的實際獲得道具清單] 來設定最終獲得的道具清單
    GiveItem_ItemGroup: async function (returnGainItems, itemValue, count, playerUID, replaceGainItems) {
        let addCurrency = {};//將要增加的資源記錄在這個字典，之後會更新資料庫
        let batchAddUniqueItemDatas = [];//將要加入的獨立資料類道具加到這個陣列，之後會送批次更新資料庫
        let addNotUniqueItem = {};//將要增加的非獨立資料類道具數量記錄在這個字典，之後會更新資料庫

        await HandleItemGroupData(itemValue, count, playerUID, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem, replaceGainItems);//處理要獲得的資料(獨立資料類道具,非獨立資料類道具)

        /*
        console.log("/////////addCurrency///////");
        for (let key in addCurrency) {
            console.log(key + ": " + addCurrency[key]);
        }
        console.log("/////////batchAddUniqueItemDatas///////");
        for (let data of batchAddUniqueItemDatas) {
            for (let key in data) {
                console.log(key + ": " + data[key]);
            }
        }
        console.log("/////////addNotUniqueItem///////");
        for (let key in addNotUniqueItem) {
            console.log(key + ": " + addNotUniqueItem[key]);
        }
        */

        await WriteBatchDataToFirestore(returnGainItems, playerUID, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem);//將批次資料寫入資料庫
    },
}
module.exports = methods;

//將資料寫入資料庫
async function WriteBatchDataToFirestore(returnGainItems, playerUID, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem) {
    //1. 寫入玩家資源資料
    if (Object.keys(addCurrency).length != 0) {
        let addCurrencyToPlayer = {};
        for (let key in addCurrency) {
            addCurrencyToPlayer[key] = admin.firestore.FieldValue.increment(addCurrency[key]);
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, playerUID, addCurrencyToPlayer);//寫入玩家資源到GameData-Player中
        //設定回傳獲得玩家道具資料
        for (let key in addCurrency) {
            returnGainItems.push({
                ItemType: key,
                ItemValue: addCurrency[key],
            });
        }
    }

    //2. 寫入獨立資料類道具
    if (batchAddUniqueItemDatas.length > 0) {
        let itemUID = await FirestoreManager.AddDocs(batchAddUniqueItemDatas);
        //設定回傳獲得道具資料
        for (let data of batchAddUniqueItemDatas) {
            let itemTypeStr = data["ColName"].replace('PlayerData-', '');
            returnGainItems.push({
                ItemType: itemTypeStr,
                ItemValue: batchAddUniqueItemDatas["ID"],
                ItemUID: itemUID,
            });
        }
    }

    //console.log("test=" + JSON.stringify(addNotUniqueItem));

    //3. 寫入非獨立資料類道具資料
    if (Object.keys(addNotUniqueItem).length != 0) {
        let updateData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, playerUID);
        if (updateData != null) {//玩家本來PlayerData-Item就有資料
            for (let itemType in addNotUniqueItem) {
                for (let id in addNotUniqueItem[itemType]) {
                    if (itemType in updateData && id in updateData[itemType]) {
                        updateData[itemType][id] += Number(addNotUniqueItem[itemType][id]);
                    }
                    else if (itemType in updateData && !(id in updateData[itemType])) {
                        updateData[itemType][id] = Number(addNotUniqueItem[itemType][id]);
                    } else {
                        updateData[itemType] = {};
                        updateData[itemType][id] = Number(addNotUniqueItem[itemType][id]);
                    }
                }
            }
            //console.log("test=" + JSON.stringify(updateData));
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Item, playerUID, updateData);//更新PlayerData-Item資料庫
        } else {//玩家本來PlayerData-Item沒資料

            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.Item, playerUID, addNotUniqueItem);//更新PlayerData-Item資料庫
            delete addNotUniqueItem["UID"];//把AddDoc_DesignatedDocName過程加的UID砍掉
            delete addNotUniqueItem["CreateTime"];//把AddDoc_DesignatedDocName過程加的CreateTime砍掉
        }
        //設定回傳獲得道具資料
        for (let itemType in addNotUniqueItem) {
            for (let id in addNotUniqueItem[itemType]) {
                for (let i = 0; i < addNotUniqueItem[itemType][id]; i++) {
                    returnGainItems.push({
                        ItemType: itemType,
                        ItemValue: id,
                    });
                }
            }
        }
    }
}

//設定玩家獲得獨立類道具的資料(此資料格式可以傳入FirestoreManager.AddDocs更新資料庫)
function GetUniqueItemAddData(itemType, itemValue, playerUID) {
    if (!(itemType in GameSetting.UniqueItemTypes))
        return null;

    let addData = {
        ID: itemValue,
        OwnerUID: playerUID,
    };

    switch (itemType) {
        case GameSetting.UniqueItemTypes.Role://腳色
            addData["EnableCall"] = true;
            let randFragment = Probability.GetRandInt(3);
            addData["Fragment"] = randFragment;//獲得腳色時直接給予隨機一個碎片
            break;
    }

    return addData;
}
//設定玩家獲得非獨立資料類道具的Data
function SetNotUniqueItemUpdateData(updateItemData, itemType, itemValue) {
    if (!(itemType in GameSetting.ItemTypes))
        return;
    if ((itemType in GameSetting.UniqueItemTypes))
        return;
    let itemID = itemValue.toString();
    if (itemType in updateItemData) {
        if (itemID in updateItemData[itemType]) {
            updateItemData[itemType][itemID] += 1;
        } else {
            updateItemData[itemType][itemID] = 1
        }
    } else {
        updateItemData[itemType] = {};
        updateItemData[itemType][itemID] = 1
    }
}
//傳入寶箱ID(ItemValue)、數量、獲得道具的玩家UID、batchAddUniqueItemDatas(要批次處理的獨立資料類道具)、addNotUniqueItem(要新增的非獨立資料類道具),未被取代前的道具清單
async function HandleItemGroupData(itemValue, count, playerUID, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem, replaceGainItems) {

    let itemGroupJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.ItemGroup, itemValue);//傳入ID取得ItemGroup表格Json資料
    if (itemGroupJsonData == null)
        return;

    let gainItems = [];
    for (let i = 0; i < count; i++) {
        let tmpGainItems = await GameDataManager.GetItemDatasFromItemGroup(itemGroupJsonData, playerUID);//將ItemGroup表格Json資料的道具清單轉為ItemData陣列
        gainItems = gainItems.concat(tmpGainItems);
    }
    //console.log("gainItems=" + JSON.stringify(gainItems));
    //將獲得的道具中有重複的道具轉為替代道具並回傳最終獲得的道具清單
    let newGainItems = await GetReplacedDuplicatedItems(playerUID, gainItems, replaceGainItems);
    //console.log("newGainItems=" + JSON.stringify(newGainItems));

    //依據要獲得的道具清單(ItemDatas)來設定要批次寫入資料庫的資料(addCurrency,batchAddUniqueItemDatas,addNotUniqueItem)
    await SetBatchWriteDatasFromItemDatas(playerUID, newGainItems, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem);
}
//依據要獲得的道具清單(ItemDatas)來設定要批次寫入資料庫的資料(addCurrency,batchAddUniqueItemDatas,addNotUniqueItem)
async function SetBatchWriteDatasFromItemDatas(playerUID, itemDatas, addCurrency, batchAddUniqueItemDatas, addNotUniqueItem) {
    for (let itemData of itemDatas) {
        if (itemData["ItemType"] != "ItemGroup") {//非寶箱類道具

            if (itemData["ItemType"] in GameSetting.CurrencyTypes) {//資源類
                if (itemData["ItemType"] in addCurrency) {
                    addCurrency[itemData["ItemType"]] += Number(itemData["ItemValue"]);
                } else {
                    addCurrency[itemData["ItemType"]] = Number(itemData["ItemValue"]);
                }
            } else if (itemData["ItemType"] in GameSetting.UniqueItemTypes) {//獨立資料類道具
                let uniqueItemData = GetUniqueItemAddData(itemData["ItemType"], itemData["ItemValue"], playerUID);
                if (uniqueItemData != null) {
                    uniqueItemData["ColName"] = GameSetting.PlayerDataCols[itemData["ItemType"]];
                    batchAddUniqueItemDatas.push(uniqueItemData);
                }

            } else {//非獨立資料類
                SetNotUniqueItemUpdateData(addNotUniqueItem, itemData["ItemType"], itemData["ItemValue"]);
            }
        } else {//這個function是依據要獲得的道具清單(ItemDatas)來設定要批次寫入資料庫的資料，因此不會有寶箱類的道具傳入才對   
            let logStr = "SetBatchWriteDatasFromItemDatas不可傳入寶箱類道具";
            Logger.CFLog(logStr, GameSetting.LogTypes.Error);
            console.log(logStr);
            continue;
        }
    }
}

//將獲得的道具中有重複的道具轉為替代道具並回傳最終獲得的道具清單
async function GetReplacedDuplicatedItems(playerUID, gainItems, replaceGainItems) {
    //let newGainItems = gainItems.map(a => Object.assign({}, a));//因為ArrayTool.RemoveItem時要抓參照去移除 此種深複製是連array中的元素都複製 所以刪除陣列元素時會被當作不同的元素移除不了
    let newGainItems = Array.from(gainItems);//深複製陣列

    let playerItemDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, playerUID);//取得玩家擁有的道具清單
    //console.log("ItemData Size(byte) = " + FirestoreManager.GetSizeOfData(playerItemDocData));//取得Firestore Document.data()的大小(byte)
    let ownedUniqueItemDatas = {};//玩家擁有的獨立類道具id清單
    for (let itemData of gainItems) {
        if (itemData["ItemType"] == "ItemGroup") {//這個function是將最終獲得的道具中有重複的道具轉為替代道具，因此不會有寶箱類的道具傳入才對
            let logStr = "ConvertDuplicatedItemToOtherItems不可傳入寶箱類道具";
            Logger.CFLog(logStr, GameSetting.LogTypes.Error);
            console.log(logStr);
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
                //紀錄被替換的道具回傳給client顯示用
                replaceGainItems.push({
                    ItemType: itemData["ItemType"],
                    ItemValue: itemData["ItemValue"],
                });
                let replaceItems = await GetReplaceItems(itemData["ItemType"], itemData["ItemValue"], playerUID);//設定取代後的道具
                newGainItems = newGainItems.concat(replaceItems);//把取代後的道具加進最終會獲得的道具清單
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
                    let stuffJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.Stuff, itemData["ItemValue"]);
                    let stackable = false;
                    if ("Stackable" in stuffJsonData)
                        stackable = TextManager.ToBoolean(stuffJsonData["Stackable"]);
                    if (stackable)
                        needReplaceDuplicatedItem = false;//可堆疊道具就不替換道具
                }
            }

            if (needReplaceDuplicatedItem) {
                let replaceItems = await GetReplaceItems(itemData["ItemType"], itemData["ItemValue"], playerUID);//設定取代後的道具
                //紀錄被替換的道具回傳給client顯示用
                replaceGainItems.push({
                    ItemType: itemData["ItemType"],
                    ItemValue: itemData["ItemValue"],
                });
                newGainItems = newGainItems.concat(replaceItems);//把取代後的道具加進最終會獲得的道具清單
                ArrayTool.RemoveItem(newGainItems, itemData);//移除被取代的道具

            } else {
                playerItemDocData[itemData["ItemType"]][itemData["ItemValue"]] = 1;//玩家擁有的此道具數量設為1
            }
        }
    }

    return newGainItems;
}

//重複道具轉換成其他道具(根據ItemGroup表轉換對應的道具)
async function GetReplaceItems(itemType, itemValue, playerUID) {
    let gainItems = [];
    let itemGroupID = 0;
    let rank = 1;
    switch (itemType) {
        case GameSetting.ItemTypes.Role://腳色
            let roleJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.Role, itemValue);
            if ("Rank" in roleJsonData)
                rank = Number(roleJsonData["Rank"]);
            itemGroupID = 110 + rank;//腳色類編號從110開始(參考ItemGroup表)
            break;
        case GameSetting.ItemTypes.Stuff://道具
            let stuffJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.Stuff, itemValue);
            switch (stuffJsonData["StuffType"]) {
                case "RoleFragment"://腳色碎片類
                    if ("Rank" in stuffJsonData)
                        rank = Number(stuffJsonData["Rank"]);
                    itemGroupID = 120 + rank;//腳色碎片類編號從120開始(參考ItemGroup表)
                    break;
                default://其他
                    //沒特別定義的道具都跑這裡
                    if (itemType in GameSetting.GameJsonNames) {//抓被替換的道具Rank
                        let otherStuffJsonData = GameDataManager.GetData(GameSetting.GameJsonNames[itemType], itemValue);
                        if ("Rank" in otherStuffJsonData)
                            rank = Number(otherStuffJsonData["Rank"]);
                    } else {
                        let logStr = "要被取代的道具類型尚未定義在GameSetting.GameJsonNames中: " + itemType;
                        Logger.CFLog(logStr, GameSetting.LogTypes.Error);
                        console.log(logStr);
                    }
                    itemGroupID = 100 + rank;//其他類編號從100開始(參考ItemGroup表)
                    break;
            }
            break;
        default:
            //沒特別定義的道具都跑這裡
            if (itemType in GameSetting.GameJsonNames) {//抓被替換的道具Rank
                let otherJsonData = GameDataManager.GetData(GameSetting.GameJsonNames[itemType], itemValue);
                if ("Rank" in otherJsonData)
                    rank = Number(otherJsonData["Rank"]);
            } else {
                let logStr = "要被取代的道具類型尚未定義在GameSetting.GameJsonNames中: " + itemType;
                Logger.CFLog(logStr, GameSetting.LogTypes.Error);
                console.log(logStr);
            }
            itemGroupID = 100 + rank;//其他類編號從100開始(參考ItemGroup表)
            break;
    }
    if (itemGroupID == 0)
        return null;
    let itemGroupJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.ItemGroup, itemGroupID);
    if (itemGroupJsonData == null)
        return null
    gainItems = await GameDataManager.GetItemDatasFromItemGroup(itemGroupJsonData, playerUID);
    return gainItems;
}