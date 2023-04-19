//基本設定
const admin = require('firebase-admin');

//自訂方法
const GameSetting = require('./GameSetting.js');
const FirestoreManager = require('../FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameDataManager');


module.exports = {
    //寫Log
    Write: function (playerUID, col, data) {
        //設定獲得道具的名稱
        SetItemNames(data)
        //設定玩家基本資料
        SetDefaultPlayerLogData(data, playerUID);
        console.log("playerUID=" + playerUID)
        console.log("col=" + col)
        console.log("data=" + JSON.stringify(data))
        FirestoreManager.AddDoc_DontWait(col, data);
    },
    //寫Cound Function Log
    CFLog: function (log, logType) {
        let data = {
            Log: log,
            LogType: logType,
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.CFLog, data);
    },
    //帳號註冊
    Signup: function (playerUID) {
        let data = {
            SignupType: GameSetting.SignupType.Game,
        }
        //設定玩家基本資料
        SetDefaultPlayerLogData(data, playerUID);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Signup, data);
    },
    //登入
    Signin: function (playerUID) {
        let data = {}
        //設定玩家基本資料
        SetDefaultPlayerLogData(data, playerUID);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Signin, data);
    },
    //登出
    Signout: function (playerUID) {
        let data = {}
        //設定玩家基本資料
        SetDefaultPlayerLogData(data, playerUID);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Signout, data);
    },

    //商城購買開始
    ShopBuy_Start: async function (docName, playerUID, shopUID, buyCount, name, itemType, itemValue, beforeCurrencies) {
        let data = SetDefaultPlayerLogData(playerUID);
        data["Progress"] = ["開始"];
        data["ShopUID"] = shopUID;
        data["BuyCount"] = buyCount;
        data["Name"] = name;
        data["ItemType"] = itemType;
        data["ItemValue"] = itemValue;
        data["ItemName"] = GetItemName(itemType, itemValue);
        data["BeforeCurrencies"] = beforeCurrencies;

        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.GameLogCols.ShopBuy, docName, data);
    },
    //商城購買追加進度
    ShopBuy_AddProgress: function (docName, addProgressStr, logData) {
        let data = {};
        data["Progress"] = admin.firestore.FieldValue.arrayUnion(addProgressStr);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
            }
            //合併資料
            data = Object.assign({}, data, logData);
        }
        //更新資料庫
        FirestoreManager.UpdateDoc(GameSetting.GameLogCols.ShopBuy, docName, data);
    },
    //儲值完後，驗證給商品處理LOG-開始
    Purchase_Start: async function (docName, playerUID, store, purchaseUID) {
        let data = SetDefaultPlayerLogData(playerUID);
        data["Progress"] = ["開始"];
        data["PurchaseUID"] = purchaseUID;
        data["Store"] = store;
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.GameLogCols.Purchase, docName, data);
    },
    //儲值完後，驗證給商品處理LOG-追加進度
    Purchase_AddProgress: function (docName, addProgressStr, logData) {
        let data = {};
        data["Progress"] = admin.firestore.FieldValue.arrayUnion(addProgressStr);
        if (logData != null) {
            //設定獲得道具的名稱
            if ("ItemType" in logData && "ItemValue" in logData) {
                logData["ItemName"] = GetItemName(logData["ItemType"], logData["ItemValue"]);
            }
            //設定獲得道具的名稱
            if ("GainItems" in logData) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
            }
            //合併資料
            data = Object.assign({}, data, logData);
        }
        //更新資料庫
        FirestoreManager.UpdateDoc(GameSetting.GameLogCols.Purchase, docName, data);
    },
}
//設定獲得道具的名稱
function SetItemNames(data) {
    if (data != null) {
        if ("GainItems" in data && data["GainItems"] != null && data["GainItems"] != undefined) {
            for (let gainItem of data["GainItems"]) {
                gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
            }
        }
    }
}

function SetDefaultPlayerLogData(data, playerUID) {
    data["PlayerUID"] = playerUID
}
//取得道具中文名稱
function GetItemName(itemType, itemValue) {
    //檢查道具類型
    if (!(itemType in GameSetting.ItemTypes)) {
        return "錯誤的道具類型: " + itemType;
    }
    //資源類道具
    if (itemType in GameSetting.CurrencyTypes) {
        return GameDataManager.GetStr("UI_Item_" + itemType, "TW_TW") + "x" + itemValue;
    }
    let strID = itemType + "_" + itemValue;
    return GameDataManager.GetStr(strID, "Name_TW");
}
