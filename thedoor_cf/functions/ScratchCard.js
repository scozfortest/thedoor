//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const Logger = require('./GameTools/Logger.js');
//專案ID
const GCloudProject = process.env.GCLOUD_PROJECT

//購買刮刮卡
exports.ScratchCard_Buy = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("UID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);

    let scratchCardDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.ScratchCard, data["UID"]);


    //確認是否為有效商品
    if (scratchCardDocData["SaleState"] == "OffSale") {
        console.log("非上架商品:" + data["UID"]);
        return { Result: "非上架商品:" + data["UID"] };
    } else if (scratchCardDocData["SaleState"] == "ForTest") {
        if (GCloudProject == GameSetting.GCloudProjects.Release) {
            console.log("此為測試商品 不可用於正式版:" + data["UID"]);
            return { Result: "非上架商品" };
        }
    }
    let nowTimestamp = admin.firestore.Timestamp.now();
    if ("StartTime" in scratchCardDocData && nowTimestamp < scratchCardDocData["StartTime"]) {
        console.log("尚未上架的商品:" + data["UID"]);
        return { Result: "尚未上架的商品:" + data["UID"] };
    }
    if ("EndTime" in scratchCardDocData && nowTimestamp > scratchCardDocData["EndTime"]) {
        console.log("已下架的商品:" + data["UID"]);
        return { Result: "已下架的商品:" + data["UID"] };
    }



    //檢查道具類型
    if (!(scratchCardDocData["ItemType"] in GameSetting.ItemTypes)) {
        console.log("錯誤的道具類型: " + data["UID"]);
        return { Result: "錯誤的道具類型: " + data["UID"] };
    }


    //設定價錢
    let priceType = scratchCardDocData["PriceType"];
    let priceValue = Number(scratchCardDocData["PriceValue"]);


    //設定購買紀錄並寫入玩家PlayerData-History資料
    let updateHistoryData = {};
    let firstBuy = false;//是否第一次購買
    let historyDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);
    if (historyDocData != null) {
        if ("ScratchCards" in historyDocData) {
            let uids = historyDocData["ScratchCards"];
            if (!uids.includes(data["UID"]))
                firstBuy = true;
            uids.push(data["UID"]);
            updateHistoryData["ScratchCards"] = uids;
        } else {
            firstBuy = true;
            updateHistoryData["ScratchCards"] = admin.firestore.FieldValue.arrayUnion(data["UID"]);
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
    } else {
        firstBuy = true;
        updateHistoryData["ScratchCards"] = [data["UID"]];
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
    }
    let free = false;
    if (firstBuy == true && ("FirstFree" in scratchCardDocData) && scratchCardDocData["FirstFree"] == true) {//第一次購買 且 此刮刮卡設定為第一次購買免費
        free = true;
    } else {
        free = false;
        //確認玩家資源足夠
        if (Number(playerDocData[priceType]) < priceValue) {
            console.log("貨幣不足:" + priceType);
            return { Result: "貨幣不足:" + priceType };
        }
        //設定消耗貨幣
        let updatePlayerData = {};
        if (priceValue > 0)
            updatePlayerData[priceType] = admin.firestore.FieldValue.increment(-priceValue);

        //寫入玩家PlayerData-Player資料
        if (Object.keys(updatePlayerData).length != 0)
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updatePlayerData);
    }


    //給予玩家道具
    let returnGainItems = [];
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    returnGainItems = await PlayerItemManager.GiveItem(scratchCardDocData["ItemType"], scratchCardDocData["ItemValue"], 1, context.auth.uid, replaceGainItems);//給予玩家道具


    if (returnGainItems != null && returnGainItems.length > 0 && returnGainItems[0] != null) {
        //寫入送跑馬燈公告
        let addNewsTickerData = {
            PlayerUID: context.auth.uid,
            MsgType: "ScratchCardMsg",
            MsgParams: [playerDocData["Name"], returnGainItems[0]["ItemType"], returnGainItems[0]["ItemValue"]],
        }
        await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addNewsTickerData);
    }







    //寫log
    let price = priceValue;
    if (free)
        price = 0;
    let costKey = "Cost" + priceType;
    let logRewardData = {
        ScratchCardUID: data["UID"],
        Name: scratchCardDocData["Name"],
        [costKey]: price,
        GainItems: returnGainItems,
        Free: free,//是否是免費購買
    }
    Logger.ScratchCard_GetReward(context.auth.uid, scratchCardDocData["ItemType"], scratchCardDocData["ItemValue"], logRewardData);


    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});