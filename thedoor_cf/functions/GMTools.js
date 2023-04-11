//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');

//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const Logger = require('./GameTools/Logger.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
//新增資料測試用
exports.AddDatas = functions.region('asia-east1').https.onRequest(async (req, res) => {
    //不用檢查權限 但正服要把外部呼叫關掉
    //在GCP把該function權限的alluser移除 讓外部網路無法呼叫

    let jsonData = req.body;

    //console.log(jsonData);

    if (!("Datas" in jsonData)) {
        console.log("格式錯誤");
        res.json({ "Result": "格式錯誤" });
        return;
    }

    /*
    Datas格式格式範例
    [
      {
          ColName:"CollectionName",
          ID:1,
          OwnerUID:"PlayerUID",
      },
      {
          ColName:"CollectionName",
          ID:1,
          OwnerUID:"PlayerUID",
      },
    ]
    */

    let datas = jsonData.Datas;
    await FirestoreManager.AddDocs(datas);
    res.json({ "Result": GameSetting.ResultTypes.Success });
    return;

});


//給予玩家道具測試
exports.GiveItem = functions.region('asia-east1').https.onRequest(async (req, res) => {
    //不用檢查權限 但正服要把外部呼叫關掉
    //在GCP把該function權限的alluser移除 讓外部網路無法呼叫

    let jsonData = req.body;

    //console.log(jsonData);

    if (!("ItemType" in jsonData) || !("ItemValue" in jsonData) || !("PlayerUID" in jsonData)) {
        console.log("格式錯誤");
        res.json({ "Result": "格式錯誤" });
        return;
    }
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    let returnGainItems = await PlayerItemManager.GiveItem(jsonData["ItemType"], jsonData["ItemValue"], jsonData["PlayerUID"], replaceGainItems);//給予玩家道具
    console.log(returnGainItems);

    res.json({
        "Result": GameSetting.ResultTypes.Success,
        "Data": {
            "ReturnGainItems": returnGainItems,
            "ReplaceGainItems": replaceGainItems,
        }
    });
    return;

});

//測試
exports.Test = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    console.log("開始執行- Crontab_RemoveTimeLimitMail");

    let nowTime = admin.firestore.Timestamp.now()
    let removableMailDocs = await FirestoreManager.GetDocs_WhereOperation(GameSetting.PlayerDataCols.Mail, "RemoveTime", "<", nowTime);//取到期的信件
    let removeUIDs = [];
    console.log(removableMailDocs);
    if (removableMailDocs != null) {
        for (let mail of removableMailDocs) {
            //如果尚未領取就移除
            if (!("ClaimTime" in mail.data())) {
                removeUIDs.push(mail.id);
                console.log("移除信件: " + mail.id);
            }
            Logger.Mail_Remove(mail.data()["OwnerUID"], mail.data());
        }
    }
    if (removeUIDs.length > 0)
        await FirestoreManager.DeleteDocsByUIDs(GameSetting.PlayerDataCols.Mail, removeUIDs);



    console.log("執行完畢- Crontab_RemoveTimeLimitMail");
});

//給予資源
exports.GiveCurrencyTest = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    console.log("測試給資源");
    await PlayerItemManager.GiveCurrency(GameSetting.CurrencyTypes.Gold, -3, context.auth.uid);
    await PlayerItemManager.GiveCurrencies([GameSetting.CurrencyTypes.Ball, GameSetting.CurrencyTypes.Point], [-4, 5], context.auth.uid);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});