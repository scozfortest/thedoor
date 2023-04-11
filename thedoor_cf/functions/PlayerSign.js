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

//玩家更改自介
exports.ChangeIntro = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("IntroBefore" in data) || !("IntroAfter" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    if (data["IntroAfter"] == null || data["IntroAfter"] == "") {
        console.log("傳入自介錯誤");
        return { Result: "傳入自介錯誤" };
    }


    //設為上線中並更新上線時間戳
    let updateData = {
        Intro: data["IntroAfter"],
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updateData);


    Logger.PlayerChangeIntro(context.auth.uid, data["IntroBefore"], data["IntroAfter"]);//寫更改自介LOG

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




//更新玩家導引階段
exports.SetGuide = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("GuideID" in data) || !("Step" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄

    if (playerHistoryDoc != null && "GuideSteps" in playerHistoryDoc) {//玩家導引紀錄存在
        let guideSteps = playerHistoryDoc["GuideSteps"];
        guideSteps[data["GuideID"]] = data["Step"];
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            GuideSteps: guideSteps
        });
    } else {
        let updateData = {
            GuideSteps: {
                [data["GuideID"]]: data["Step"],
            },
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

// 根據GiftCodeSetting建立『所有』邀請序號碼
// 建立邀請序號碼(根據目前活動的GameData-GiftCodeSetting，玩家會創建一組自己的序號(每個活動各會有一組)
// 此序號碼可以給其他玩家輸入，輸入後雙方都會領取到獎勵(寄送到mail)
exports.CreateInvitationCodes = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let nowTimestamp = admin.firestore.Timestamp.now();
    let giftCodeSettingDocs = await FirestoreManager.GetAllDocs(GameSetting.GameDataCols.GiftCodeSetting);
    let giftCodeSettingDocDats = [];
    //設定可以執行的禮物碼
    for (let i = 0; i < giftCodeSettingDocs.length; i++) {
        let data = giftCodeSettingDocs[i].data();
        if ("StartTime" in data && nowTimestamp < data["StartTime"])
            continue;
        if ("EndTime" in data && nowTimestamp > data["EndTime"])
            continue;
        giftCodeSettingDocDats.push(data);
    }
    let myInvitationCodeDocs = await FirestoreManager.GetDocs_Where(GameSetting.GameDataCols.GiftCode, "OwnerUID", context.auth.uid);
    for (let i = 0; i < giftCodeSettingDocDats.length; i++) {
        let needCreateInvitationCode = true;//是否需要產生邀請碼
        if (myInvitationCodeDocs != null) {
            for (let j = 0; j < myInvitationCodeDocs.length; j++) {
                if (myInvitationCodeDocs[j].data()["GiftCodeSettingUID"] == giftCodeSettingDocDats[i]["UID"]) {
                    needCreateInvitationCode = false;
                    break;
                }
            }
        }
        //產生玩家個人邀請碼
        if (needCreateInvitationCode) {
            let invitationData = {
                Name: giftCodeSettingDocDats[i]["Name"],
                CurClaimTimes: 0,
                MaxClaimTimes: Number(giftCodeSettingDocDats[i]["MaxClaimTimes"]),
                Type: "Invitation",
                OwnerUID: context.auth.uid,
                GiftCodeSettingUID: giftCodeSettingDocDats[i]["UID"],
                Invitees: [],
            }
            if ("StartTime" in giftCodeSettingDocDats[i])
                invitationData["StartTime"] = giftCodeSettingDocDats[i]["StartTime"];
            if ("EndTime" in giftCodeSettingDocDats[i])
                invitationData["EndTime"] = giftCodeSettingDocDats[i]["EndTime"];

            await FirestoreManager.AddDoc(GameSetting.GameDataCols.GiftCode, invitationData);
        }
    }

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

/*
//傳入GiftCodeSettingUID建立邀請序號碼
// 建立邀請序號碼(根據目前活動的GameData-GiftCodeSetting，玩家會創建一組自己的序號(每個活動各會有一組)
// 此序號碼可以給其他玩家輸入，輸入後雙方都會領取到獎勵(寄送到mail)
exports.CreateInvitationCodeBySettingUID = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("GiftCodeSettingUID" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let nowTimestamp = admin.firestore.Timestamp.now();
    let giftCodeSettingDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.GiftCodeSetting, data["GiftCodeSettingUID"]);
    if (giftCodeSettingDocData == null) {
        console.log("無此序號碼設定");
        return { Result: "無此序號碼設定" };
    }
    if ("StartTime" in giftCodeSettingDocData && nowTimestamp < giftCodeSettingDocData["StartTime"]) {
        console.log("要創建的序號碼尚未開始");
        return { Result: "序號碼錯誤" };
    }
    if ("EndTime" in giftCodeSettingDocData && nowTimestamp > giftCodeSettingDocData["EndTime"]) {
        console.log("要創建的序號碼已過期");
        return { Result: "序號碼錯誤" };
    }
    let myInvitationCodeDocs = await db.collection(GameSetting.GameDataCols.GiftCode).where("OwnerUID", '==', context.auth.uid).where("GiftCodeSettingUID", '==', giftCodeSettingDocData["UID"]).get();
    if (myInvitationCodeDocs.empty != true) {//序號碼已經創建過了
        return {
            Result: GameSetting.ResultTypes.Success,
        };
    }

    //創建序號碼
    let invitationData = {
        Name: giftCodeSettingDocData["Name"],
        CurClaimTimes: 0,
        MaxClaimTimes: Number(giftCodeSettingDocData["MaxClaimTimes"]),
        Type: "Invitation",
        OwnerUID: context.auth.uid,
        GiftCodeSettingUID: giftCodeSettingDocData["UID"],
         Invitees: [],
    }

    await FirestoreManager.AddDoc(GameSetting.GameDataCols.GiftCode, invitationData);


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});
*/

