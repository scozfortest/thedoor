//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');

//取得PlayerData-Player, PlayerData-History, PlayerData-Item的Size
exports.GetDataSize = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("PlayerUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    let playerDataDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, data["PlayerUID"]);
    let historyDataDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, data["PlayerUID"]);
    let itemDataDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, data["PlayerUID"]);

    let playerDataBytes = FirestoreManager.GetSizeOfData(playerDataDoc);//取得Firestore Document.data()的大小(byte)
    let historyDataBytes = FirestoreManager.GetSizeOfData(historyDataDoc);//取得Firestore Document.data()的大小(byte)
    let itemDataBytes = FirestoreManager.GetSizeOfData(itemDataDoc);//取得Firestore Document.data()的大小(byte)

    let resultData = {
        PlayerDataBytes: playerDataBytes,
        HistoryDataBytes: historyDataBytes,
        ItemDataBytes: itemDataBytes,
    }

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: resultData,
    };

});