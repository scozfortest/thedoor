//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
/*
//新增跑馬燈資料
exports.NewsTicker_AddMsg = functions.region('asia-east1').https.onRequest(async (req, res) => {
    //不用檢查權限 但正服要把外部呼叫關掉
    //在GCP把該function權限的alluser移除 讓外部網路無法呼叫

    let data = req.body;

    console.log(data);

    if (!("MsgType" in data) || !("MsgParams" in data)) {
        console.log("格式錯誤");
        res.json({ "Result": "格式錯誤" });
        return;
    }

    let addMsgData = {
        PlayerUID: data.PlayerUID,
        MsgParams: data.MsgParams,
        MsgType: data.MsgType,
    };

    await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addMsgData);

    res.json({ "Result": GameSetting.ResultTypes.Success });
    return;

});
*/
//新增跑馬燈資料
exports.NewsTicker_AddMsg = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("MsgType" in data) || !("MsgParams" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }
    let addData = {
        PlayerUID: context.auth.uid,
        MsgType: data["MsgType"],
        MsgParams: data["MsgParams"],
    }

    await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addData);


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//暫時用 送全服玩家信
exports.NewsTicker_TmpSendAllMail = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let playerDocs = await FirestoreManager.GetAllDocs(GameSetting.PlayerDataCols.Player);
    var addDatas = [];
    for (let i = 0; i < playerDocs.length; i++) {
        let playerData = playerDocs[i].data();
        addDatas.push({
            ColName: GameSetting.PlayerDataCols.Mail,
            OwnerUID: playerData["UID"],
            Items: [{
                ItemType: "Gold",
                ItemValue: 1000,
            }, {
                ItemType: "Ball",
                ItemValue: 10000,
            }],
            Title: "祝大家平平安安，聖誕快樂！",
        });
    }

    console.log("寄送人數:" + addDatas.length);
    await FirestoreManager.AddDocs(addDatas);


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});