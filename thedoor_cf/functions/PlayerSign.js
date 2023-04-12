//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const Logger = require('./GameTools/Logger.js');
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const MyTime = require('./Scoz/MyTime.js');



//註冊完Firebase執行
//※初始化程式碼有調整的話網頁端註冊也要改
exports.SignUp = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("AuthType" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    let nowTime = admin.firestore.Timestamp.now();




    //設定要寫的玩家道具
    let defaultItemDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "DefaultItem");
    let writeItemData = {};
    writeItemData = Object.assign({}, writeItemData, defaultItemDocData);

    //寫PlayerData-Item資料
    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.Item, context.auth.uid, writeItemData);

    //設定要寫的玩家
    let writePlayerData = {
        AuthType: data.AuthType,
        SignupType: GameSetting.SignupType.Game,
        State: GameSetting.PlayerStateTypes.Online,
        LastSigninTime: nowTime,
    }

    let defaultPlayerDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "DefaultPlayer");
    if (defaultPlayerDocData != null) {
        writePlayerData = Object.assign({}, writePlayerData, defaultPlayerDocData);
        let curSerialNO = writePlayerData["CurSerialNO"];
        writePlayerData["Name"] = writePlayerData["Name"] + curSerialNO;
        delete writePlayerData["CurSerialNO"];
    }
    //寫PlayerData-Player資料
    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.Player, context.auth.uid, writePlayerData);
    //更新DefaultPlayer表的CurSerialNO
    FirestoreManager.UpdateDoc(GameSetting.GameDataCols.Setting, "DefaultPlayer", {
        CurSerialNO: admin.firestore.FieldValue.increment(1),
    });

    //寫OnlineTimeStamp資料
    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.OnlineTimestamp, context.auth.uid, {
        UpdateTime: admin.firestore.Timestamp.now(),
    });

    //寫註冊LOG
    Logger.Signup(context.auth.uid);

    //重新設定CreateTime格式為C#可讀取的str回傳給Client使用
    let scozTimeStr = MyTime.ConvertToScozTimeStr(MyTime.AddHours(nowTime.toDate(), 8));
    writePlayerData["CreateTime"] = scozTimeStr;

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: writePlayerData,
    };

});


//剛進遊戲時執行，玩家遊玩中跨日(凌晨0點)也會執行
exports.SignIn = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    //設為上線中並更新上線時間戳
    let updateData = {
        LastSigninTime: admin.firestore.Timestamp.now(),
        State: "Online",
    }
    FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updateData);


    Logger.Signin(context.auth.uid);//寫登入LOG

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//設定玩家裝置UID，剛進遊戲時執行
exports.SetDevice = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("DeviceUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    //設為上線中並更新上線時間戳
    let updateData = {
        DeviceUID: data["DeviceUID"],
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updateData);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//玩家更名
exports.ChangeName = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("NameBefore" in data) || !("NameAfter" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    if (data["NameAfter"] == null || data["NameAfter"] == "") {
        console.log("傳入名稱錯誤");
        return { Result: "傳入名稱錯誤" };
    }

    let docData = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.Player, "Name", data["NameAfter"]);
    if (docData != null) {
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "DuplicatedName"
        };
    }

    //設為上線中並更新上線時間戳
    let updateData = {
        Name: data["NameAfter"],
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updateData);


    Logger.PlayerChangeName(context.auth.uid, data["NameBefore"], data["NameAfter"]);//寫更名LOG

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//玩家上線時，每間隔一段時間會送更新時間戳
//送的頻率參考GameData - Setting -> Timer -> OnlineTimeStampCD
exports.UpdateOnlineTimestamp = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    if (playerDocData == null) return;


    try {

        //如果玩家目前是下線的，改回上線中並更新上線時間戳
        if (playerDocData["State"] == "Offline") {
            let updateData = {
                LastSigninTime: admin.firestore.Timestamp.now(),
                State: "Online"
            }
            FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updateData);
        }
        //更新時間戳
        FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.OnlineTimestamp, context.auth.uid, {
            UpdateTime: admin.firestore.Timestamp.now(),
        });
    }
    catch (e) {
        Logger.CFLog("UpdateOnlineTimestamp Error: " + e, GameSetting.LogTypes.Error);
    }

});


//觸發事件紀錄
exports.TriggerEvent = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("Type" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let triggerEventSettingDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "TriggerEvent");
    let recordableEvents = triggerEventSettingDocData["RecordableEvents"];
    //不需要紀錄的事件就直接返回
    if (!recordableEvents.includes(data["Type"])) {
        return {
            Result: GameSetting.ResultTypes.Success,
        };
    }
    let updateData = {
        TriggerEvents: admin.firestore.FieldValue.arrayUnion({
            Type: data["Type"],
            TriggerTime: admin.firestore.Timestamp.now(),
        })
    }
    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄
    if (playerHistoryDoc != null && "TriggerEvents" in playerHistoryDoc) {//玩家導引紀錄存在

        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
    } else {
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