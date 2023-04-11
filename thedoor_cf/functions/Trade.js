//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const ArrayTool = require('./Scoz/ArrayTool.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const Logger = require('./GameTools/Logger.js');

//贈送PT
exports.Trade_SendPT = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("PlayerUID" in data) || !("Point" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let sendPT = Number(data["Point"]);
    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let targetDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, data["PlayerUID"]);
    let historyDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);
    if (targetDocData == null || playerDocData == null) {
        console.log("玩家不存在");
        return { Result: "格式錯誤" };
    }
    let beforePT = playerDocData["Point"];
    if (beforePT < sendPT) {
        console.log("點數不足");
        return { Result: "格式錯誤" };
    }

    let vip = 0;
    if ("VIP" in playerDocData)
        vip = playerDocData["VIP"];
    let vipJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.VIP, (vip + 1));
    let maxSendPT = Number(vipJsonData["MaxSendPT"]);//玩家目前VIP等級最高能贈送的PT
    let sentPoint = 0;
    if (historyDocData != null && "SentPoint" in historyDocData) {
        sentPoint = historyDocData["SentPoint"];
    }
    //console.log("maxSendPT=" + maxSendPT);
    //console.log("sentPoint=" + sentPoint);
    //console.log("sendPT=" + sendPT);
    if ((sentPoint + sendPT) > maxSendPT) {//超過本月可以贈送的PT額度
        console.log("超過本月可以贈送的額度");
        return { Result: "格式錯誤" };
    }
    let afterPT = (beforePT + sendPT);
    let addData = {
        OwnerUID: data["PlayerUID"],
        SenderUID: context.auth.uid,
        Title: "PlayerGift",
        Items: [{
            ItemType: "Point",
            ItemValue: sendPT,
        }],
    }

    //玩家扣PT
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, {
        Point: admin.firestore.FieldValue.increment(-sendPT),
    });
    //更新已贈送PT量
    if (historyDocData != null) {
        if ("SentPoint" in historyDocData) {
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
                SentPoint: admin.firestore.FieldValue.increment(sendPT),
            });
        } else {
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
                SentPoint: sendPT,
            });
        }
    } else {
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, {
            SentPoint: sendPT,
        });
    }


    //送禮物信件給好友
    await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, addData);

    //寫Log
    let logData = {
        Point: sendPT,
        BeforeSentPT: beforePT,
        AfterPT: afterPT,
        VIP: vip,
        BeforeMonthlySentPT: sentPoint,
        AfterMonthlySentPT: (sentPoint + sendPT),
    };
    Logger.Trade_SendPT(context.auth.uid, logData);



    return {
        Result: GameSetting.ResultTypes.Success,
    };

});
