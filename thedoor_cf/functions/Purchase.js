//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
const db = admin.firestore();
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const ArrayTool = require('./Scoz/ArrayTool.js');
const Logger = require('./GameTools/Logger.js');
const Postman = require('./Scoz/Postman.js')
//專案ID
const GCloudProject = process.env.GCLOUD_PROJECT
//Google Play API
const MyGoogleAPI = require('./IAP/GoogleAPI.js');
//Apple API
const MyAppleAPI = require('./IAP/AppleAPI.js');

//儲值
exports.Purchase = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.log("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("Receipt" in data) || !("PurchaseUID" in data)) {
        console.log("格式錯誤:Key值錯誤");
        return { Result: "格式錯誤:Key值錯誤" };
    }


    console.log("MyReceipt=" + data["Receipt"]);

    let version = data["Version"];
    console.log("玩家本機版本=" + version);
    let purchaseToken = "";
    let store = "Test";
    let purchaseLogDocName = context.auth.uid + "-" + Date.now();
    let productID = "";
    //Dev版或Test版
    if (GCloudProject != GameSetting.GCloudProjects.Release) {


        //寫Purchase開始LOG
        await Logger.Purchase_Start(purchaseLogDocName, context.auth.uid, store, data["PurchaseUID"]);//寫LOG

        productID = data["PurchaseUID"];

    } else {//Release跑這裡

        let receipt = JSON.parse(data["Receipt"]);
        store = "Test";
        if (receipt != null)
            store = receipt.Store;
        //寫Purchase開始LOG
        await Logger.Purchase_Start(purchaseLogDocName, context.auth.uid, store, data["PurchaseUID"]);//寫LOG



        //fake example PayLoad 內容請查 https://docs.unity3d.com/Packages/com.unity.purchasing@3.2/manual/UnityIAPPurchaseReceipts.html
        //正式Google流程
        if (GCloudProject == GameSetting.GCloudProjects.Release && store == "GooglePlay") {
            let packageName = GameSetting.PackageNames.Release;
            let googlePayLoad = JSON.parse(JSON.parse(receipt["Payload"]).json)
            purchaseToken = googlePayLoad.purchaseToken;
            productID = googlePayLoad.productId;
            const payLog = await db.collection(GameSetting.GameLogCols.Purchase).where('Token', '==', purchaseToken).where('Store', '==', store).get()
            if (!payLog.empty) {
                console.log("已經使用過的訂單編號" + purchaseToken);
                return {
                    Result: GameSetting.ResultTypes.Success,
                    Data: "used token",
                };
            }
            //取得訂單的productID 檢查484有該訂單
            let productData = await MyGoogleAPI.GetProduct(packageName, productID, purchaseToken);
            //console.log("productData: " + JSON.stringify(data));
            if (typeof (productData) == undefined || productData == null) {
                console.log("找不到該訂單資訊" + purchaseToken);
                return {
                    Result: GameSetting.ResultTypes.Success,
                    Data: "non-existent purchase",
                };
            }
        }
        //正式Apple流程
        else if (GCloudProject == GameSetting.GCloudProjects.Release && store == "AppleAppStore") {
            const payLog = await db.collection(GameSetting.GameLogCols.Purchase).where('Token', '==', purchaseToken).where('Store', '==', store).get()
            if (!payLog.empty) {
                console.log("已經使用過的訂單編號" + purchaseToken);
                return {
                    Result: GameSetting.ResultTypes.Success,
                    Data: "used token",
                };
            }
            //IOS >= 7
            let validateResult = await MyAppleAPI.ValidateReciept(receipt["Payload"]);
            if (!validateResult || validateResult.status != 0) {
                console.log("Apple訂單認證失敗" + validateResult.status);
                return {
                    Result: GameSetting.ResultTypes.Success,
                    Data: "non-existent purchase",
                };
            }
            purchaseToken = receipt["TransactionID"];

            for (let i = 0; i < validateResult.receipt.in_app.length; i++) {
                if (validateResult.receipt.in_app[i].transaction_id == purchaseToken) {
                    productID = validateResult.receipt.in_app[i].product_id;
                    break;
                }
            }
            console.log("productID : " + productID);
        }
    }

    if (productID == "") {
        console.log("購買商品ID為空");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "non-existent purchase id",
        };
    }

    let purchaseUID = data["PurchaseUID"];//因為DB的UID跟實際送雙平台商店的product id可能會不一樣，要取到對的DB的品項資料要用PurchaseUID

    //寫PurchaseLOG
    let logData1 = {
        Token: purchaseToken,
    }
    Logger.Purchase_AddProgress(purchaseLogDocName, "訂單驗證", logData1);

    //取得儲值品項資料
    let purchaseDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Purchase, purchaseUID);
    if (purchaseDocData == null) {
        console.log("不存在的Purchase ID:" + purchaseUID);
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "non-existent purchase id",
        };
    }


    let itemType = purchaseDocData["ItemType"];//道具類型
    let itemValue = Number(purchaseDocData["ItemValue"]);//道具數值
    let price = Number(purchaseDocData["Price"]);//台幣價格

    console.log("//////////PlayerUID=" + context.auth.uid);
    console.log("purchaseUID=" + purchaseUID);
    console.log("itemType=" + itemType);
    console.log("itemValue=" + itemValue);
    console.log("price=" + price);
    //檢查道具類型
    if (!(itemType in GameSetting.ItemTypes)) {
        console.log("錯誤的道具類型" + data["PurchaseUID"]);
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "wrong item type",
        };
    }

    //當有非法儲值商品時寫入log，但不阻止玩家獲得道具，因為會跑到這裡代表玩家已經付錢給平台了
    let illegalPurchaseLogs = [];
    //確認是否為有效商品
    if (purchaseDocData["SaleState"] == "OffSale") {
        illegalPurchaseLogs.push("非上架商品");
        console.log("非上架商品:" + data["PurchaseUID"]);
    } else if (purchaseDocData["SaleState"] == "ForTest") {
        if (GCloudProject == GameSetting.GCloudProjects.Release) {
            illegalPurchaseLogs.push("此為測試商品 不可用於正式版");
            console.log("此為測試商品 不可用於正式版" + data["PurchaseUID"]);
        }
    }
    let nowTimestamp = admin.firestore.Timestamp.now();
    if ("StartTime" in purchaseDocData && nowTimestamp < purchaseDocData["StartTime"]) {
        illegalPurchaseLogs.push("尚未上架的商品");
        console.log("尚未上架的商品" + data["PurchaseUID"]);
    }
    if ("EndTime" in purchaseDocData && nowTimestamp > purchaseDocData["EndTime"]) {
        illegalPurchaseLogs.push("已下的商品");
        console.log("已下架的商品" + data["PurchaseUID"]);
    }

    //確認商品在限購次數內
    let buyLimitType = "None";
    let historyDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄
    if (("BuyLimitType" in purchaseDocData) && ("BuyLimit" in purchaseDocData)) {//判斷此商品是不是限購商品
        buyLimitType = purchaseDocData["BuyLimitType"];
        let buyLimitTimes = Number(purchaseDocData["BuyLimit"]);
        if (buyLimitTimes <= 0) {//限購次數<=0就當沒有限購
            buyLimitType = "None";
        } else {
            switch (buyLimitType) {
                case "Daily"://每日限購
                    if (historyDocData != null && ("DailyLimitPurchaseItems" in historyDocData)) {
                        let alreadyBuyTimes = ArrayTool.Count(historyDocData["DailyLimitPurchaseItems"], data["PurchaseUID"]);//取得已經購買此商品次數
                        if (alreadyBuyTimes >= buyLimitTimes) {
                            illegalPurchaseLogs.push("已達每日限購次數");
                            console.log("已達每日限購次數" + data["PurchaseUID"]);
                        }
                    }
                    break;
                case "Permanence"://終身限購
                    if (historyDocData != null && ("LimitPurchaseItems" in historyDocData)) {
                        let alreadyBuyTimes = ArrayTool.Count(historyDocData["LimitPurchaseItems"], data["PurchaseUID"]);//取得已經購買此商品次數
                        if (alreadyBuyTimes >= buyLimitTimes) {
                            illegalPurchaseLogs.push("已達限購次數");
                            console.log("已達限購次數" + data["PurchaseUID"]);
                        }
                    }
                    break;
            }
        }
    }


    //設定限購道具的已購買次數並寫入玩家PlayerData-History資料
    let updateHistoryData = {};
    if (buyLimitType != "None") {
        switch (buyLimitType) {
            case "Daily"://每日限購
                if (historyDocData != null) {
                    if ("DailyLimitPurchaseItems" in historyDocData) {
                        let uids = historyDocData["DailyLimitPurchaseItems"];
                        uids.push(data["PurchaseUID"]);
                        updateHistoryData["DailyLimitPurchaseItems"] = uids;
                    } else {
                        updateHistoryData["DailyLimitPurchaseItems"] = admin.firestore.FieldValue.arrayUnion(data["PurchaseUID"]);
                    }
                } else {
                    updateHistoryData["DailyLimitPurchaseItems"] = [data["PurchaseUID"]];
                }
                break;
            case "Permanence"://終身限購
                if (historyDocData != null) {
                    if ("LimitPurchaseItems" in historyDocData) {
                        let uids = historyDocData["LimitPurchaseItems"];
                        uids.push(data["PurchaseUID"]);
                        updateHistoryData["LimitPurchaseItems"] = uids;
                    } else {
                        updateHistoryData["LimitPurchaseItems"] = admin.firestore.FieldValue.arrayUnion(data["PurchaseUID"]);
                    }

                } else {
                    updateHistoryData["LimitPurchaseItems"] = [data["PurchaseUID"]];

                }
                break;
        }
    }

    if (historyDocData != null) {
        if (Object.keys(updateHistoryData).length != 0)
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
    } else {
        if (Object.keys(updateHistoryData).length != 0)
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
    }


    let VIP_AddEXP_URL = ""
    switch (GCloudProject) {
        case GameSetting.GCloudProjects.Test:
            VIP_AddEXP_URL = "https://asia-east1-majampachinko-test1.cloudfunctions.net/VIP_AddEXP";
            break;
        case GameSetting.GCloudProjects.Dev:
            VIP_AddEXP_URL = "https://asia-east1-majampachinko-develop.cloudfunctions.net/VIP_AddEXP";
            break;
        case GameSetting.GCloudProjects.Release:
            VIP_AddEXP_URL = "https://asia-east1-majampachinko-release.cloudfunctions.net/VIP_AddEXP";
            break;
    }
    let postResult = await Postman.SendPost(VIP_AddEXP_URL, {
        "PlayerUID": context.auth.uid,
        "VIPType": GameSetting.VIPType.Purchase,
        "Value": price,
    })
    console.log("Purchase VIP_AddEXP Result: ", JSON.stringify(postResult))

    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    let returnGainItems = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, replaceGainItems);//給予玩家道具

    //寫PurchaseLOG
    let logData2 = {
        Name: purchaseDocData["Name"],
        ItemType: itemType,
        ItemValue: itemValue,
        CostNTD: price,
        IllegalPurchaseLogs: illegalPurchaseLogs,
        GainItems: returnGainItems,
    }
    Logger.Purchase_AddProgress(purchaseLogDocName, "完成", logData2);

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };
});

//設定玩家最後購買的PurchaseUID
exports.SetBougthShopUID = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("BougthShopUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄

    if (playerHistoryDoc != null && "BougthShopUID" in playerHistoryDoc) {//玩家導引紀錄存在
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            BougthShopUID: data["BougthShopUID"],
        });
    } else {
        let updateData = {
            BougthShopUID: data["BougthShopUID"],
        }
        if (playerHistoryDoc == null) {//玩家紀錄資料不存在就新增新的一筆
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        } else {
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }
    }

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});