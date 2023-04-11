//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const Logger = require('./GameTools/Logger.js');

//領取信件
exports.Mail_Claim = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("MailUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }


    let mailDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Mail, data.MailUID);
    let returnGainItems = [];
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)

    if (mailDocData != null) {
        //設定Log
        let logData = {
            MailUID: mailDocData["UID"],
            Title: mailDocData["Title"],
        }
        if ("SenderUID" in mailDocData) {
            logData["SenderUID"] = mailDocData["SenderUID"];
        }
        //更新信件
        let updateMailData = {
            ClaimTime: admin.firestore.Timestamp.now(),
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Mail, data.MailUID, updateMailData);
        //給予玩家道具
        if ("Items" in mailDocData) {
            let items = mailDocData["Items"];
            for (let item of items) {
                let tmpReplaceGainItems = [];
                let gainItems = await PlayerItemManager.GiveItem(item["ItemType"], item["ItemValue"], 1, context.auth.uid, tmpReplaceGainItems);//給予玩家道具
                returnGainItems = returnGainItems.concat(gainItems);//設定要返回Client的道具清單資料
                replaceGainItems = replaceGainItems.concat(tmpReplaceGainItems);//設定要返回Client的被取代道具清單
                //設定log資料
                if ("GainItems" in logData)
                    logData["GainItems"] = logData["GainItems"].concat(gainItems);
                else
                    logData["GainItems"] = gainItems;

            }
        }
        //寫Log
        Logger.Mail_Claim(context.auth.uid, logData);
    }


    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});

//領取全部信件
exports.Mail_ClaimAll = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let mailDocs = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.Mail, "OwnerUID", context.auth.uid);
    let returnGainItems = [];
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    let updateMailDatas = [];
    for (let mailDoc of mailDocs) {
        let mailDocData = mailDoc.data();
        if ("ClaimTime" in mailDocData)//有ClaimTime代表已經領取了 不用處理
            continue;

        //設定要更新的信件資料
        updateMailDatas.push({
            ColName: GameSetting.PlayerDataCols.Mail,
            UID: mailDoc.id,
            ClaimTime: admin.firestore.Timestamp.now(),
        });
        //設定Log
        let logData = {
            MailUID: mailDocData["UID"],
            Title: mailDocData["Title"],
        }
        if ("SenderUID" in mailDocData) {
            logData["SenderUID"] = mailDocData["SenderUID"];
        }
        //給予道具
        if ("Items" in mailDocData) {
            let items = mailDocData["Items"];
            for (let item of items) {
                let temReplaceGainItems = [];
                let gainItems = await PlayerItemManager.GiveItem(item["ItemType"], item["ItemValue"], 1, context.auth.uid, temReplaceGainItems);//給予道具
                returnGainItems = returnGainItems.concat(gainItems);//設定要返回Client的道具清單資料
                replaceGainItems = replaceGainItems.concat(temReplaceGainItems);//設定要返回Client的被取代道具清單
                //設定Log資料
                if ("GainItems" in logData)
                    logData["GainItems"] = logData["GainItems"].concat(gainItems);
                else
                    logData["GainItems"] = gainItems;

            }
        }
        //寫Log
        Logger.Mail_Claim(context.auth.uid, logData);
    }
    if (updateMailDatas.length > 0) {
        await FirestoreManager.UpdateDocs(updateMailDatas);//更新信件
    }


    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});