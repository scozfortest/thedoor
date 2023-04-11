//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const Probability = require('./Scoz/Probability.js');
const Crypto = require('./Scoz/Crypto.js');
const GameSetting = require('./GameTools/GameSetting.js');

const GameDataManager = require('./GameTools/GameDataManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const VIPManager = require('./VIP.js');
const Postman = require('./Scoz/Postman.js')
const GCloudProject = process.env.GCLOUD_PROJECT

//建立配對房間資料
exports.CreateMaJamMatchingRoom = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    if (!("RoomID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }
    let nowTime = admin.firestore.Timestamp.now();
    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let roomName = playerDocData["Name"] + "#" + Probability.GetRandInt(100);//房間名稱，玩家手動輸入加房間用的名稱，由玩家名稱+0~99組成
    let roomUID = Crypto.GetMD5(context.auth.uid + nowTime);//不會重複的UID若配桌後進入遊戲，也是該局麻將的配對UID之後查Log用

    //好友房間次數檢測
    let roomSettingDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "Room");//取得Room後台設定
    let friendRoomDailyLimit = Number(roomSettingDocData["FriendRoomDailyLimit"]);
    let vip = Number(playerDocData["VIP"]) + 1;//取VIP等級編號(等級+1就是表格編號)
    let vipJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.VIP, vip.toString());
    let extraFriendRoom = Number(vipJsonData["ExtraFriendRoom"]);
    friendRoomDailyLimit += extraFriendRoom;
    //console.log("friendRoomDailyLimit=" + friendRoomDailyLimit);
    let playerHistoryDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家歷史紀錄
    if (playerHistoryDocData != null && "CreateFriendRoomTimes" in playerHistoryDocData) {
        if (playerHistoryDocData["CreateFriendRoomTimes"] >= friendRoomDailyLimit) {
            console.log("今日已到最大次數");
            return { Result: "格式錯誤" };
        }
    }



    //let roomUID = crypto.createHash('md5').update(context.auth.uid + nowTime).digest('hex').toString();//不會重複的UID若配桌後進入遊戲，也是該局麻將的配對UID之後查Log用
    let maJamMatchingRoomData = {
        RoomName: roomName,
        RoomID: data["RoomID"],//房間類型名稱 對應GameData-MaJamRoom的DocID(Ex. Gold1)
        PlayerUIDs: [context.auth.uid],//玩家UID陣列
        OwnerUID: context.auth.uid,//房主UID
        LastUpdateTime: admin.firestore.Timestamp.now(),//上次更新時間
        Start: false,//是否開始遊戲了
    }
    //配對房間建立時，一併移除開房者之前創的房間(如果有)
    await FirestoreManager.DeleteDocs_WhereOperation(GameSetting.PlayerDataCols.MaJamMatchingRoom, "OwnerUID", "==", context.auth.uid);
    //建立配對房資料
    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.MaJamMatchingRoom, roomUID, maJamMatchingRoomData);
    //移除時間欄位，因為時間欄位的資料傳回Client(C#)沒辦法直接讀取也用不到
    delete maJamMatchingRoomData["CreateTime"];
    delete maJamMatchingRoomData["LastUpdateTime"];

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: maJamMatchingRoomData,
    };

});


//移除配對房間資料
exports.RemoveMaJamMatchingRoom = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    await FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"]);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//配對房間移除時，一併移除來自此房間的遊戲邀請(如果有)
exports.OnRoomRemove = functions.region('asia-east1').firestore.document('PlayerData-MaJamMatchingRoom/{UID}').onDelete(async (snap, context) => {
    //刪除來自此房間的遊戲邀請(如果有)
    await FirestoreManager.DeleteDocs_WhereOperation(GameSetting.PlayerDataCols.MaJamMatchingInvite, "RoomUID", "==", snap.data()["UID"]);
});

//配對房間中，房客點擊離開房間 或是 房主踢人
exports.MaJamMatchingRemovePlayer = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomUID" in data) || !("PlayerUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }


    let maJamMatchingRoomData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"]);
    if (maJamMatchingRoomData == null) {
        console.log("找不到房間");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }
    if (maJamMatchingRoomData["OwnerUID"] != context.auth.uid) {//如果不是房主呼叫此Func
        if (data["PlayerUID"] != context.auth.uid) {//要踢除的對象不是自己
            console.log("只有房主能踢人");
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "只有房主能踢人",
            };
        }
    }


    let updateData = {
        PlayerUIDs: admin.firestore.FieldValue.arrayRemove(data["PlayerUID"]),
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"], updateData);//更新房間資料


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//更新配對房間時間戳，房主送時間戳計時器(每分鐘會更新房間時間戳，如果玩家直接把App滑掉，太久沒更新房間會自動移除太久沒更新的房間)
exports.UpdateMaJamMatchingRoomTimestamp = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }


    let maJamMatchingRoomData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"]);
    if (maJamMatchingRoomData == null) {
        console.log("找不到房間");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }


    let updateData = {
        LastUpdateTime: admin.firestore.Timestamp.now(),//上次更新時間,
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"], updateData);//更新房間資料


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});
//配對房間中點擊遊戲開始
exports.MaJamMatchingStartGame = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    let updateData = {
        Start: true,
    }
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"], updateData);//更新房間資料


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});
//加入配對房間(可傳入RoomName或RoomUID)
exports.JoinMaJamMatchingRoom = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomName" in data) && !("RoomUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    let maJamMatchingRoomData = null;
    if ("RoomName" in data) {
        let matchingRoomDocs = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.MaJamMatchingRoom, "RoomName", data["RoomName"]);
        if (matchingRoomDocs != null)
            maJamMatchingRoomData = matchingRoomDocs[0].data();
    } else if ("RoomUID" in data) {
        maJamMatchingRoomData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"]);
    }
    if (maJamMatchingRoomData == null) {
        console.log("找不到房間名");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "RoomNotExist",
        };
    }

    let playerUIDs = maJamMatchingRoomData["PlayerUIDs"];
    if (playerUIDs.length >= 4) {
        console.log("房間人數已滿");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "FullRoom",
        };
    }
    if (maJamMatchingRoomData["PlayerUIDs"].includes(context.auth.uid)) {//當玩家重複加入時(一般不會發生才對)還是回傳加入成功
        //console.log("玩家重複加入");
        //移除時間欄位，因為時間欄位的資料傳回Client(C#)沒辦法讀取也用不到
        delete maJamMatchingRoomData["CreateTime"];
        delete maJamMatchingRoomData["LastUpdateTime"];

        return {
            Result: GameSetting.ResultTypes.Success,
            Data: maJamMatchingRoomData,
        };
    }

    maJamMatchingRoomData["PlayerUIDs"].push(context.auth.uid);
    let updateData = {
        PlayerUIDs: admin.firestore.FieldValue.arrayUnion(context.auth.uid),
    }

    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.MaJamMatchingRoom, maJamMatchingRoomData["UID"], updateData);//更新房間資料

    //移除時間欄位，因為時間欄位的資料傳回Client(C#)沒辦法讀取也用不到
    delete maJamMatchingRoomData["CreateTime"];
    delete maJamMatchingRoomData["LastUpdateTime"];
    console.log("length=" + maJamMatchingRoomData["PlayerUIDs"].length);
    return {
        Result: GameSetting.ResultTypes.Success,
        Data: maJamMatchingRoomData,
    };

});

//寄送好友房邀請
exports.MaJamMatchingInvite = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomUID" in data) || !("InviteeUIDs" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    let matchingRoomDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.MaJamMatchingRoom, data["RoomUID"]);
    if (matchingRoomDocData == null) {
        console.log("找不到好友配對房");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "MatchingNotExist",
        };
    }

    let setMergeDatas = [];
    for (let i = 0; i < data["InviteeUIDs"].length; i++) {
        let docUID = data["InviteeUIDs"][i] + "=" + context.auth.uid;
        setMergeDatas.push({
            UID: docUID,
            CreateTime: admin.firestore.Timestamp.now(),
            OwnerUID: data["InviteeUIDs"][i],
            InviterUID: context.auth.uid,
            RoomID: matchingRoomDocData["RoomID"],
            RoomUID: data["RoomUID"],
        });
    }

    //寫入好友房邀請資料
    if (setMergeDatas.length > 0) {
        for (let setMergeData of setMergeDatas) {
            FirestoreManager.SetDoc_DesignatedDocName(GameSetting.PlayerDataCols.MaJamMatchingInvite, setMergeData["UID"], setMergeData);
        }
    }


    return {
        Result: GameSetting.ResultTypes.Success,
    };

});

//拒絕(刪除)好友房邀請
exports.RemoveMaJamMatchingInvite = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("RoomUID" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.MaJamMatchingInvite, data["RoomUID"]);

    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//給Server取隨機AI用
exports.GetRandomAIPlayerIDs = functions.region('asia-east1').https.onRequest(async (req, res) => {

    let jsonData = req.body;

    //console.log(jsonData);

    if (!("Count" in jsonData)) {
        console.log("格式錯誤");
        res.json({ "Result": "格式錯誤" });
        return;
    }
    let pickCount = Number(jsonData["Count"]);
    let aiIDs = GameDataManager.GetJsonDataIDs(GameSetting.GameJsonNames.AIPlayer);
    let pickedIDs = Probability.GetRandNoDuplicatedItems(aiIDs, pickCount);

    res.json({
        "Result": GameSetting.ResultTypes.Success,
        "Data": pickedIDs,
    });
    return;

});

//傳入房間UID取得麻將房間設定(測試用)
exports.GetMaJamRoomData = functions.region('asia-east1').https.onRequest(async (req, res) => {
    let jsonData = req.body;

    //console.log(jsonData);

    if (!("RoomUID" in jsonData)) {
        console.log("格式錯誤");
        res.json({ "Result": "格式錯誤" });
        return;
    }
    let roomUID = jsonData["RoomUID"];
    let roomDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.MaJamRoom, roomUID);
    console.log("roomDocData=" + JSON.stringify(roomDocData));


    res.json({
        "Result": GameSetting.ResultTypes.Success,
        "Data": roomDocData,
    });
    return;
});

// MaJamEndRound結算
exports.MaJamEndRound = functions.region('asia-east1').https.onRequest(async (req, res) => {
    let jsonData = req.body;
    console.log("[normal] callEndRoundStart jsonData" + JSON.stringify(jsonData));

    // ----------------------撈資料並檢查 START↓
    // jsonData資料檢查
    const checkJsonDataKey = ["Players", "IsAI", "CurrentRound", "WinnerNum", "IsLife", "RoundChanges", "GameDataRoomUID", "RoomName", "DocUID", "WinBalls"]
    let allReturn = false
    checkJsonDataKey.forEach(key => {
        if (!(key in jsonData)) {
            allReturn = true
            let errMessage = "PlayingData with wrong " + key + ": " + JSON.stringify(jsonData)
            console.warn("[ERROR] " + errMessage);
            res.json({
                "Result": errMessage,
                "Data": jsonData
            });
            return;
        }
    })
    if (allReturn) {
        return
    }
    if (jsonData.Players.length != 4) {
        let errMessage = "playingData with wrong Players size:" + JSON.stringify(jsonData)
        res.json({
            "Result": errMessage,
            "Data": jsonData.Players
        });
        return;
    }
    if (jsonData.CurrentRound <= 0) {
        console.warn("[ERROR] playingData with CurrentRound <= 0" + JSON.stringify(jsonData));
        res.json({
            "Result": "playingData with CurrentRound <= 0",
            "Data": jsonData
        });
        return;
    }

    // GameData資料檢查
    let roomGameData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.MaJamRoom, jsonData.GameDataRoomUID);
    // console.log("[normal] roomGameData: " + JSON.stringify(roomGameData));
    const checkRoomGameDataKey = ["BetType"]
    checkRoomGameDataKey.forEach(key => {
        if ((roomGameData) && (!(key in roomGameData))) {
            let errMessage = "RoomUID: " + JSON.stringify(jsonData.GameDataRoomUID) + ", GameData with wrong " + key + ": " + JSON.stringify(roomGameData)
            console.warn("[ERROR] " + errMessage);
            res.json({
                "Result": errMessage,
                "Data": roomGameData
            });
            return;
        }
    })

    const roomBetType = roomGameData.BetType // 房型使用的BetType
    let getOutThreshold = 0
    if ("GetOutThreshold" in roomGameData) {
        getOutThreshold = roomGameData.GetOutThreshold
    }
    let ballRate = 1
    if ("BallRate" in roomGameData) {
        ballRate = roomGameData.BallRate
    }
    let commission = 0
    if ("Commission" in roomGameData) {
        commission = roomGameData.Commission
    }
    let ante = 0
    let tai = 0
    if ("Bet" in roomGameData) {
        ante = Number(roomGameData["Bet"].split("/")[0])
        tai = Number(roomGameData["Bet"].split("/")[1])
    }

    // 真實玩家資料
    const playerSnapshot = await FirestoreManager.GetDocs_WhereIn(GameSetting.PlayerDataCols.Player, "UID", jsonData.Players)
    //console.log("[normal] " + JSON.stringify(playerSnapshot));
    if (playerSnapshot.empty) {
        console.warn("[ERROR] 找不到任何玩家, RoomName:" + jsonData["RoomName"] + " ,players: " + jsonData.Players);
        res.json({
            "Result": "找不到任何玩家",
            "Data": playerSnapshot
        });
        return;
    }

    // ----------------------整理&計算 START↓
    // 整理接下來需要的玩家資料:
    // 玩家資料樣板
    let uidToIndex = {}
    for (let i = 0; i < jsonData.Players.length; i++) {
        uidToIndex[jsonData.Players[i]] = i
    }

    const tGetOut = "GetOut"
    const tBroken = "Broken"

    let playerDatas = {
        UIDToIndex: uidToIndex,
        index: [0, 1, 2, 3],
        uid: jsonData.Players,
        isAI: jsonData.IsAI,
        isLife: jsonData.IsLife,
        rawChange: jsonData.RoundChanges,                   // 該局主貨幣差(server來的計分結果，未修訂)
        rawBall: jsonData.WinBalls,                         // 該局原始鋼珠數量
        playerTag: [[], [], [], []],
        // 以下隨後賦值
        isGetOut: [false, false, false, false],             // 是否踢出
        startBet: [0, 0, 0, 0],                             // 進房主貨幣量 (機器人預設0)
        currentBet: [0, 0, 0, 0],                           // 當前主貨幣量 (機器人預設0)
        revisedChange: [0, 0, 0, 0],                        // 該局主貨幣差 (修訂後)
        revisedBall: [0, 0, 0, 0],                          // 該局小鋼珠差 (修訂後)
        // 臨時錢包專區
        tmpAllBag: [null, null, null, null],                // 紀錄個人臨時錢包
        tmpGameBag: [null, null, null, null],               // 個人該場臨時錢包
        tmpBagStartBet: [0, 0, 0, 0],                       // 臨時錢包初始金額
        // VIP
        vipData: [null, null, null, null]
    };

    let reportNeedData = {
        commission: [0, 0, 0, 0],
        ballChangeWithoutVIPRate: [0, 0, 0, 0],
    }

    // vipData-realPlayer
    for (let doc of playerSnapshot) {
        let playerIndex = uidToIndex[doc.data()["UID"]]
        let vipLv = 0
        if ("VIP" in doc.data()) {
            vipLv = doc.data()["VIP"]
        }
        playerDatas.vipData[playerIndex] = VIPManager.GetVIPData(vipLv)
    }
    // vipData-aiPlayer
    for (let i = 0; i < playerDatas.index.length; i++) {
        if (!playerDatas.isAI[i]) {
            continue
        }
        const localAIPrefix = "COMPUTER_LOCAL_"
        const internationalAIPrefix = "COMPUTER_INTERNATIONAL_"
        let vipLv = 0
        let uid = playerDatas.uid[i]
        if (uid.substring(0, localAIPrefix.length) == localAIPrefix) {
            let fakeUID = uid.replace(localAIPrefix, "")
            console.log("[normal] try get local ai: ", fakeUID)
            const fakePlayer = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, fakeUID)
            if ((fakePlayer) && ("VIP" in fakePlayer)) {
                vipLv = fakePlayer["VIP"]
                console.log("[normal] get local ai vip ok: ", vipLv)
            }
        } else if (uid.substring(0, internationalAIPrefix.length) == internationalAIPrefix) {
            console.log("try get international ai: ", uid)
            if ("AIUID" in jsonData) {
                let aiUID = jsonData["AIUID"][i]
                let aiData = GameDataManager.GetData(GameSetting.GameJsonNames.AIPlayer, aiUID);
                if (aiData) {
                    vipLv = Number(aiData["VipLevel"])
                    console.log("[normal] internationalAI get vip by AIUID: " + aiUID + ", lv: " + vipLv)
                }
            } else {
                let aiName = uid.replace(internationalAIPrefix, "")
                let aiJson = GameDataManager.GetJson(GameSetting.GameJsonNames.AIPlayer);
                console.log("try get international ai: " + uid + " -> " + aiName)
                for (let aiData of aiJson) {
                    let aiID = aiData["ID"]
                    let strID = "AIPlayer_" + aiID
                    if (GameDataManager.GetStr(strID, "Name_TW") == aiName) {
                        vipLv = Number(aiData["VipLevel"])
                        console.log("[normal] internationalAI get vip by AIName: " + strID + ",Name: " + aiName + " ,lv: " + vipLv)
                        break
                    }
                }
            }
        }
        playerDatas.vipData[i] = VIPManager.GetVIPData(vipLv)
    }


    // playerDatas.startBet (機器人預設0)
    let needAddStartData = false
    const settlementSnapshot = await FirestoreManager.GetDocData(GameSetting.GameLogCols.MaJamGameSettlement, jsonData.DocUID)
    if (!settlementSnapshot) {
        if (jsonData.CurrentRound != 1) { //非Round1撈不到資料一樣要新建
            console.warn("[ERROR] PlayerUID、StartData loss with jsonData: " + JSON.stringify(jsonData));
        }
        needAddStartData = true
        playerSnapshot.forEach(doc => {
            let playerIndex = uidToIndex[doc.data().UID]
            playerDatas.startBet[playerIndex] = doc.data()[roomBetType]
        })
    } else {
        if (jsonData.CurrentRound == 1) {
            // Round1卻已經有資料? 留LOG，但還是用新資料覆蓋
            console.warn("[ERROR] SettlementData already exist before 1st round?: " + JSON.stringify(jsonData))
            needAddStartData = true
            playerSnapshot.forEach(doc => {
                let playerIndex = uidToIndex[doc.data().UID]
                playerDatas.startBet[playerIndex] = doc.data()[roomBetType]
            })
        } else { // 其他Round抓第1Round產的資料 
            // console.log("settleSnap" + JSON.stringify(settlementSnapshot))
            if (!("PlayerUID" in settlementSnapshot) || !("StartData" in settlementSnapshot)) {
                console.warn("[ERROR] PlayerUID、StartData loss with jsonData: " + JSON.stringify(jsonData));
                needAddStartData = true
                playerSnapshot.forEach(doc => {
                    let playerIndex = uidToIndex[doc.data().UID]
                    playerDatas.startBet[playerIndex] = doc.data()[roomBetType]
                })
            } else { // 缺少PlayerUID、StartData，用當前資料創新的
                if (!("StartBet" in settlementSnapshot.StartData)) {
                    // 有StartData卻沒有StartBet，用當前資料創新的
                    console.warn("[ERROR] startBet disappear in StartData: " + JSON.stringify(jsonData));
                    needAddStartData = true
                    playerSnapshot.forEach(doc => {
                        let playerIndex = uidToIndex[doc.data().UID]
                        playerDatas.startBet[playerIndex] = doc.data()[roomBetType]
                    })
                } else {
                    let playerUID = settlementSnapshot.PlayerUID
                    // console.log("StartBet " + JSON.stringify(settlementSnapshot.StartData.StartBet))
                    for (let uid of playerUID) {
                        let playerIndex = uidToIndex[uid]
                        playerDatas.startBet[playerIndex] = settlementSnapshot.StartData.StartBet[playerIndex]
                        // console.log("setStartBet: " + uid + " Index: " + playerIndex + " => " + playerDatas.startBet[playerIndex]);
                    }
                    console.log("[normal] startBet from 1st round =>" + roomBetType + ": " + JSON.stringify(playerDatas.startBet));
                }
            }
        }
    }

    // playerDatas.currentBet (機器人預設0)
    // currentBet = playerBet + tmpBagBet
    for (let doc of playerSnapshot) {
        let playerUID = doc.data().UID
        let playerIndex = uidToIndex[playerUID]
        let newTmpGameBag = true
        if (playerDatas.isLife[playerIndex]) { // 正常活人
            newTmpGameBag = false
            const tAllBag = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.PlayerTmpBag, playerUID)
            // 臨時錢包回補
            let tmpGameBagMoney = 0
            if (tAllBag) {
                playerDatas.tmpAllBag[playerIndex] = tAllBag
                if (jsonData.DocUID in tAllBag) {
                    // 臨時錢包存在
                    playerDatas.tmpGameBag[playerIndex] = tAllBag[jsonData.DocUID]

                    if (roomBetType in tAllBag[jsonData.DocUID]) {
                        tmpGameBagMoney = playerDatas.tmpGameBag[playerIndex][roomBetType]
                        playerDatas.tmpBagStartBet[playerIndex] = tmpGameBagMoney
                        console.log("[tmpGameBag] tmpGameBagMoneyBack: ", playerUID, " tmpGameBagStartBet: ", playerDatas.tmpBagStartBet[playerIndex])
                    }
                }
            }
            //console.log("[normal] DocData: " + JSON.stringify(doc.data()))
            const playerBet = doc.data()[roomBetType]
            playerDatas.currentBet[playerIndex] = tmpGameBagMoney + playerBet
            console.log("[normal] OnlinePlayer: " + playerUID + " ,tmpGameBagMoney:" + tmpGameBagMoney, " + playerBet: ", playerBet)
        } else { // 離線
            const tAllBag = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.PlayerTmpBag, playerUID)
            // 臨時錢包回補
            let tmpGameBagMoney = 0
            if (tAllBag) {
                playerDatas.tmpAllBag[playerIndex] = tAllBag
                if ((jsonData.DocUID in tAllBag) &&
                    (roomBetType in tAllBag[jsonData.DocUID])) {
                    // 臨時錢包存在
                    playerDatas.tmpGameBag[playerIndex] = tAllBag[jsonData.DocUID]
                    tmpGameBagMoney = playerDatas.tmpGameBag[playerIndex][roomBetType]
                    playerDatas.tmpBagStartBet[playerIndex] = tmpGameBagMoney
                    newTmpGameBag = false
                }
            }
            if (newTmpGameBag) {
                // 臨時錢包不存在
                const playerMoney = doc.data()[roomBetType]
                tmpGameBagMoney = (10 * ante + 0 * tai)
                if (tmpGameBagMoney > playerMoney) {
                    tmpGameBagMoney = playerMoney
                }
                playerDatas.tmpBagStartBet[playerIndex] = tmpGameBagMoney
                console.log("[tmpGameBag] OfflinePlayerNewTmpBag")
            }
            playerDatas.currentBet[playerIndex] = tmpGameBagMoney
            console.log("[tmpGameBag] OfflinePlayerCurrentBet: ", tmpGameBagMoney)
        }
        console.log("[tmpGameBag] PlayerBet: ", playerUID,
            " ,currentPlayerBet: ", playerDatas.currentBet[playerIndex],
            " ,tmpBagStartBet: ", playerDatas.tmpBagStartBet[playerIndex],
            " ,IsLife: ", (playerDatas.isLife[playerIndex]),
            " ,IsNewTmpGameBag", newTmpGameBag)
    }


    // playerDatas.revisedChange (先比照playerDatas.rawChange)
    playerDatas.revisedChange = Object.assign([], playerDatas.rawChange)

    let anyGetOut = false
    if (jsonData.WinnerNum <= 0) {
        // 和局不用修訂
    } else {
        // 修訂規則
        // 輸家:
        // 1. 無法一炮多響。
        // 2. 最多只能輸到沒錢
        // 3. 輸到沒錢||低於門檻值會被請出房
        // 贏家:
        // 1. 進房帶多少錢，對單一玩家最多就只能贏多少錢

        // 修訂開始:
        // 先找贏家 & 對單一玩家最高贏分
        // 無法一炮多響。
        let winnerIndex = -1
        let winMax = 0
        for (let i = 0; i < jsonData.Players.length; i++) {
            const rawChange = playerDatas.rawChange[i]
            if (rawChange > 0) {
                if (winnerIndex < 0) {
                    winnerIndex = i
                    winMax = playerDatas.startBet[i]
                } else {
                    console.warn("[ERROR]" + i + " " + winnerIndex + "一炮多響, 目前不適用。 jsonData: " + JSON.stringify(jsonData) + " ,playerDatas: " + JSON.stringify(playerDatas));
                    res.json({
                        "Result": "一炮多響, 目前不適用。",
                        "Data": jsonData
                    });
                    return;
                }
            }
        }

        // 輸家處理&贏分總計
        let winnerRevisedChange = 0
        for (let i = 0; i < jsonData.Players.length; i++) {
            const rawChange = playerDatas.rawChange[i]
            if (rawChange >= 0) {
                continue
            }
            const currentBet = playerDatas.currentBet[i]

            // 最多只能輸到沒錢
            if (!playerDatas.isAI[i]) {
                if (currentBet + rawChange >= 0) {
                    playerDatas.revisedChange[i] = rawChange
                } else {
                    playerDatas.revisedChange[i] = -currentBet
                }
            } else {
                playerDatas.revisedChange[i] = rawChange
            }

            // 輸到沒錢||低於門檻值會被請出房
            const revisedChange = playerDatas.revisedChange[i]
            if (!playerDatas.isAI[i]) {
                if (currentBet + revisedChange == 0) {
                    playerDatas.isGetOut[i] = true
                    playerDatas.playerTag[i].push(tBroken)
                    anyGetOut = true
                }
                if ((playerDatas.isLife[i]) && (currentBet + revisedChange < getOutThreshold)) {
                    playerDatas.isGetOut[i] = true
                    playerDatas.playerTag[i].push(tGetOut)
                    anyGetOut = true
                }
            }

            // 贏家進房帶多少錢，對單一玩家最多就只能贏多少錢
            if (playerDatas.isAI[winnerIndex]) {
                winnerRevisedChange = winnerRevisedChange + (-revisedChange)
            } else {
                if (winMax >= (-revisedChange)) {
                    winnerRevisedChange = winnerRevisedChange + (-revisedChange)
                } else {
                    winnerRevisedChange = winnerRevisedChange + winMax
                }
            }
        }

        // 贏家獎勵
        let winnerCommission = commission
        if ((roomBetType == "Point") &&
            (playerDatas.vipData[winnerIndex]) &&
            ("PointCommission" in playerDatas.vipData[winnerIndex])) {
            let winnerVipCommission = (Number(playerDatas.vipData[winnerIndex]["PointCommission"]) / 100)
            if (winnerVipCommission < commission) {
                winnerCommission = winnerVipCommission
            }
        }
        let winnerRevisedRate = (1 - winnerCommission)
        if (winnerRevisedRate < 0) {
            console.warn("[WARN] winner: " + playerDatas.uid[winnerIndex] + "commission error: " + winnerRevisedRate + " = " + "(1-" + winnerCommission + ")")
            winnerRevisedRate = (1 - commission)
        }
        if (playerDatas.vipData[winnerIndex]) {
            console.log("[normal] winnerVIPData: " + JSON.stringify(playerDatas.vipData[winnerIndex]))
        }
        let winnerFinalRevisedChange = winnerRevisedChange * winnerRevisedRate
        if (winnerFinalRevisedChange < 0) {
            console.warn("[WARN] winner: " + playerDatas.uid[winnerIndex] + "winnerFinalRevisedChange < 0, " + winnerFinalRevisedChange + " => 0 ")
            winnerFinalRevisedChange = 0
        }

        console.log("[normal] winnerRevisedChange: " + playerDatas.uid[winnerIndex] + " => " + winnerFinalRevisedChange + " = " + winnerRevisedChange + "*" + winnerRevisedRate)
        let winnerVipBallRate = 0
        if ((playerDatas.vipData[winnerIndex]) &&
            ("BallMultiplier" in playerDatas.vipData[winnerIndex])) {
            winnerVipBallRate = Number(playerDatas.vipData[winnerIndex]["BallMultiplier"])
        }
        const winnerRevisedBall = jsonData.WinBalls[winnerIndex] * ballRate * ((100 + winnerVipBallRate) / 100)

        // console.log("[normal] types: ", typeof playerDatas.uid[winnerIndex], typeof winnerRevisedBall, typeof jsonData.WinBalls[winnerIndex], typeof ballRate, typeof winnerVipBallRate)
        console.log("[normal] winnerRevisedBall: " + playerDatas.uid[winnerIndex] + " => " + winnerRevisedBall + " = " + jsonData.WinBalls[winnerIndex] + "*" + ballRate + "*" + "((100 +" + winnerVipBallRate + ") / 100)" + " , vRate: ", ((100 + winnerVipBallRate) / 100))
        playerDatas.revisedChange[winnerIndex] = Math.ceil(winnerFinalRevisedChange)
        playerDatas.revisedBall[winnerIndex] = Math.ceil(winnerRevisedBall)
        reportNeedData.commission[winnerIndex] = winnerRevisedChange - Math.ceil(winnerFinalRevisedChange)
        reportNeedData.ballChangeWithoutVIPRate[winnerIndex] = Math.ceil(jsonData.WinBalls[winnerIndex] * ballRate)
    }

    // ----------------------獎勵/貨幣處理/發獎/臨時錢包處理 START↓
    let playerRewards = {}
    let logRewards = {}
    // 獎勵/貨幣處理
    for (let i = 0; i < playerDatas.index.length; i++) {
        let uid = playerDatas.uid[i]
        // 貨幣類結算
        // 貨幣
        let cReward = {}
        cReward[roomBetType] = playerDatas.revisedChange[i]

        // 新手導引房輸錢不扣款並通知
        if ((jsonData.GameDataRoomUID == "Guide1") &&
            (!playerDatas.isAI[i]) &&
            (cReward[roomBetType] < 0)) {
            cReward[roomBetType] = 0
            let mailContent = {}
            let mailTitle = "導引期間不輸錢，十張輕鬆做大牌！"
            mailContent["Title"] = mailTitle
            mailContent["OwnerUID"] = uid
            let mailResult = await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, mailContent);
            console.log("[GuideFree] player: " + uid + " ,AMail: " + JSON.stringify(mailContent) + "=> Result: " + mailResult)
        }

        if (playerDatas.revisedBall[i] > 0) {
            cReward["Ball"] = playerDatas.revisedBall[i]
        }
        playerRewards[uid] = {}
        playerRewards[uid].currencyReward = cReward
        logRewards[uid] = cReward
        // 物品類發獎 (目前沒有，之後再說)
        // let iReward = {}
        // playerRewards[uid].itemReward = iReward
    }

    // 貨幣類發獎/臨時錢包
    console.log("[normal] playerDatas: " + JSON.stringify(playerDatas))
    for (let uid in playerRewards) {
        let playerIndex = uidToIndex[uid]
        if (playerDatas.isAI[playerIndex]) {
            continue
        }
        if (playerDatas.isLife[playerIndex]) {
            if (!playerDatas.tmpGameBag[playerIndex]) {
                // 線上玩家無臨時錢包, 直接發獎回玩家身上
                console.log("[tmpGameBag] onlinePlayer: ", uid, "reward: ", JSON.stringify(playerRewards[uid].currencyReward))
                for (cType in playerRewards[uid].currencyReward) {
                    await PlayerItemManager.GiveCurrency(cType, playerRewards[uid].currencyReward[cType], uid);
                }
            } else {
                // 線上玩家有臨時錢包, 整合臨時錢包後全部發獎回玩家身上
                console.log("[tmpGameBag] onlineTmpBagPlayer: ", uid, "reward: ", JSON.stringify(playerRewards[uid].currencyReward), " bag: ", JSON.stringify(playerDatas.tmpGameBag[playerIndex]))
                let mergeReward = { ...playerDatas.tmpGameBag[playerIndex], ...playerRewards[uid].currencyReward }
                for (let cType in mergeReward) {
                    if (cType == "CreateTime") {
                        continue
                    }
                    let oriValue = mergeReward[cType]
                    let addValue = 0
                    if (cType in playerDatas.tmpGameBag[playerIndex]) {
                        addValue = playerDatas.tmpGameBag[playerIndex][cType]
                    }
                    let allReturnToPlayer = oriValue + addValue
                    console.log("[normal] onlinePlayerReturnTmpGameBag: " + uid + ": " + cType + ": (ori)", oriValue, " + (add)", addValue)
                    await PlayerItemManager.GiveCurrency(cType, allReturnToPlayer, uid);
                }
                // 移除臨時錢包
                if (playerDatas.tmpAllBag[playerIndex]) {
                    let pAllBag = playerDatas.tmpAllBag[playerIndex]
                    let docNum = 0
                    for (let key in pAllBag) {
                        if (key == "CreateTime") {
                            continue
                        }
                        if (key == "UID") {
                            continue
                        }
                        docNum = docNum + 1
                    }
                    if (docNum <= 1) {
                        await FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.PlayerTmpBag, uid)
                    } else {
                        let deleteData = {}
                        deleteData[jsonData.DocUID] = admin.firestore.FieldValue.delete()
                        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.PlayerTmpBag, uid, deleteData)
                    }
                } else {
                    console.warn("[ERROR] player:" + uid + "has tmpGameBag but no tmpAllBag!!?")
                }
            }
        } else {
            if (!playerDatas.tmpGameBag[playerIndex]) {
                // 預扣款
                await PlayerItemManager.GiveCurrency(roomBetType, -playerDatas.tmpBagStartBet[playerIndex], uid);
                // 離線玩家新增臨時錢包
                let newGameBag = {}
                for (let cType in playerRewards[uid].currencyReward) {
                    let PlayerResult = playerRewards[uid].currencyReward[cType]
                    if (cType == roomBetType) {
                        PlayerResult = PlayerResult + playerDatas.tmpBagStartBet[playerIndex]
                    }
                    newGameBag[cType] = PlayerResult
                }
                let updateData = {}
                updateData[jsonData.DocUID] = newGameBag
                updateData[jsonData.DocUID]["CreateTime"] = admin.firestore.Timestamp.now();
                if (playerDatas.tmpAllBag[playerIndex]) {
                    console.log("[tmpGameBag] offlineNewRoundTmpBagPlayer: ", uid, "reward: ", JSON.stringify(playerRewards[uid].currencyReward), " bag: ", JSON.stringify(updateData))
                    await FirestoreManager.SetDoc_DesignatedDocName_Merge(GameSetting.PlayerDataCols.PlayerTmpBag, uid, updateData)
                } else {
                    console.log("[tmpGameBag] offlineNewEmptyTmpBagPlayer: ", uid, "reward: ", JSON.stringify(playerRewards[uid].currencyReward), " bag: ", JSON.stringify(updateData))
                    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.PlayerTmpBag, uid, updateData)
                }
            } else {
                // 已斷線離線玩家更新臨時錢包
                let updateData = {}
                for (let cType in playerRewards[uid].currencyReward) {
                    if (playerRewards[uid].currencyReward[cType] == 0) {
                        continue
                    }
                    updateData[jsonData.DocUID + "." + cType] = admin.firestore.FieldValue.increment(playerRewards[uid].currencyReward[cType])
                }
                if (Object.keys(updateData).length > 0) {
                    console.log("[tmpGameBag] offlineUpdateTmpBagPlayer: ", uid, "reward: ", JSON.stringify(playerRewards[uid].currencyReward), " bag: ", JSON.stringify(updateData))
                    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.PlayerTmpBag, uid, updateData)
                }
            }
        }
    }

    // ----------------------LOG紀錄 START↓
    if (!settlementSnapshot) { // 需要新建
        // template 
        let SetSettlementData = {
            PlayerUID: playerDatas.uid,
            IsAI: playerDatas.isAI,
            StartData: {},
            RoundData: {},
            //EndData: {}, // EndData在EndGame階段更新就好，EndRound階段不用管
        }

        // StartData
        SetSettlementData.StartData = {
            GameDataRoomUID: jsonData.GameDataRoomUID,
            BetType: roomBetType,
            BallRate: ballRate,
            Commission: commission,
            StartBet: playerDatas.startBet,
        }
        if (!needAddStartData) {
            console.warn("[ERROR] Something wrong when EndRound settlementSnapshot...Need Add Doc but !needAddStartData." + JSON.stringify(jsonData))
            SetSettlementData.StartData["Warning"] = "Add Doc but !needAddStartData"
        }

        // RoundData
        let roundData = {
            IsInRoom: playerDatas.isLife,
            CurrentBet: playerDatas.currentBet,
            RawChange: playerDatas.rawChange,
            RevisedChange: playerDatas.revisedChange,
            RoundReward: logRewards,
        }
        SetSettlementData.RoundData[jsonData.CurrentRound] = roundData
        // console.log("[normal] RoundSettlementData: " + JSON.stringify(SetSettlementData));
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.GameLogCols.MaJamGameSettlement, jsonData.DocUID, SetSettlementData);
    } else { // 已有資料需更新
        let UpdateSettlementData = {}
        if (needAddStartData) {
            let startData = {
                GameDataRoomUID: jsonData.GameDataRoomUID,
                BetType: roomBetType,
                BallRate: ballRate,
                StartBet: playerDatas.startBet,
                Warning: "AddFromRound" + length(jsonData.CurrentRound)
            }
            UpdateSettlementData.StartData = startData
            UpdateSettlementData.PlayerUID = playerDatas.uid
        }
        let roundData = {
            IsInRoom: playerDatas.isLife,
            CurrentBet: playerDatas.currentBet,
            RawChange: playerDatas.rawChange,
            RevisedChange: playerDatas.revisedChange,
            RoundReward: logRewards,
        }
        UpdateSettlementData[`RoundData.${jsonData.CurrentRound}`] = roundData
        await FirestoreManager.UpdateDoc(GameSetting.GameLogCols.MaJamGameSettlement, jsonData.DocUID, UpdateSettlementData);
    }

    let returnServerData = {
        IsStop: anyGetOut,
        ResultTag: playerDatas.playerTag,
        ShowRevised: false,
        RevisedScore: playerDatas.revisedChange,
        RevisedBall: playerDatas.revisedBall,
        Commission: reportNeedData.commission,
        RevisedBallWithoutVIPRate: reportNeedData.ballChangeWithoutVIPRate
    }

    res.json({
        "Result": GameSetting.ResultTypes.Success,
        "Data": returnServerData,
    });
    return;
});

function DealTmpGameBagBackItemMailTitle(titleTemplate, gamedataRoomUID, mailItem, lang) {
    let dealStr = (titleTemplate.match(/\{.*?\}/g))
    let dealNum = dealStr.length
    console.log("[normal] DealTempGameBagBackItemMailTitle: ", titleTemplate, " ,roomUID: ", gamedataRoomUID, " ,Items: ", mailItem, " ,titleDealNum: ", dealNum)

    let roomName = GameDataManager.GetStr("UI_MaJamRoomName_" + gamedataRoomUID, lang);
    if (!roomName) {
        roomName = gamedataRoomUID
    }
    let returnTitle = titleTemplate.replace("{0}", roomName)
    let itemTitleNum = dealNum - 1
    for (let i = 0; i < itemTitleNum; i++) {
        let replaceWord = "{" + (i + 1).toString() + "}"
        if (i >= mailItem.length) {
            returnTitle = returnTitle.replace(replaceWord, "")
            continue
        }
        let itemName = getItemTitle(mailItem[i].ItemType, "", lang) //不顯示數量所以填“”
        if (i > 0) {
            itemName = "、" + itemName
        }
        returnTitle = returnTitle.replace(replaceWord, itemName)
    }
    return returnTitle
}

function DealTmpGameBagBackEmptyMailTitle(titleTemplate, gamedataRoomUID, lang) {
    // let dealStr = (titleTemplate.match(/\{.*?\}/g))
    // let dealNum = dealStr.length
    // console.log("[normal] DealTmpGameBagBackEmptyMailTitle: ", titleTemplate, " ,roomUID: ", gamedataRoomUID, " ,titleDealNum: ", dealNum)

    let roomName = GameDataManager.GetStr("UI_MaJamRoomName_" + gamedataRoomUID, lang);
    if (!roomName) {
        roomName = gamedataRoomUID
    }
    let returnTitle = titleTemplate.replace("{0}", roomName)
    return returnTitle
}

//取得道具中文名稱
function getItemTitle(itemType, itemValue, lang) {
    //檢查道具類型
    if (!(itemType in GameSetting.ItemTypes)) {
        return "錯誤的道具類型: " + itemType;
    }
    //資源類道具
    if (itemType in GameSetting.CurrencyTypes) {
        let currencyTitle = GameDataManager.GetStr("UI_Item_" + itemType, lang)
        if ((itemValue) && (itemValue == "")) {
            currencyTitle = currencyTitle + "x" + itemValue;
        }
        return currencyTitle
    }
    let strID = itemType + "_" + itemValue;
    return GameDataManager.GetStr(strID, "Name_TW");
}

// MaJamEndGame結算
exports.MaJamEndGame = functions.region('asia-east1').https.onRequest(async (req, res) => {
    let jsonData = req.body;
    console.log("[normal] jsonData: " + JSON.stringify(jsonData));

    // ----------------------撈資料並檢查↓
    const checkJsonDataKey = ["Players", "IsAI", "GameDataRoomUID", "DocUID", "RoomName", "BaseRound", "TotalRound", "TotalScores", "TotalBalls"]
    checkJsonDataKey.forEach(key => {
        if (!(key in jsonData)) {
            let errMessage = "PlayingData with wrong " + key + ": " + JSON.stringify(jsonData)
            console.warn("[ERROR] " + errMessage);
            res.json({
                "Result": errMessage,
                "Data": jsonData
            });
            return;
        }
    })

    if (jsonData.Players.length != 4) {
        let errMessage = "playingData with wrong Players size:" + JSON.stringify(jsonData)
        res.json({
            "Result": errMessage,
            "Data": jsonData.Players
        });
        return;
    }

    // GameData資料檢查
    let roomGameData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.MaJamRoom, jsonData.GameDataRoomUID);
    // console.log("[normal] roomGameData: " + JSON.stringify(roomGameData));
    const checkRoomGameDataKey = ["BetType"]
    checkRoomGameDataKey.forEach(key => {
        if ((roomGameData) && (!(key in roomGameData))) {
            let errMessage = "RoomUID: " + JSON.stringify(jsonData.GameDataRoomUID) + ", GameData with wrong " + key + ": " + JSON.stringify(roomGameData)
            console.log("[ERROR] " + errMessage);
            res.json({
                "Result": errMessage,
                "Data": roomGameData
            });
            return;
        }
    })
    let exp = [0, 0, 0, 0] // 預設EXP
    if ("Exp" in roomGameData) {
        exp = roomGameData.Exp
    }

    let roomBetType = "Bet"
    if ("BetType" in roomGameData) {
        roomBetType = roomGameData.BetType
    }

    // 之前的記錄撈取
    const settlementSnapshot = await FirestoreManager.GetDocData(GameSetting.GameLogCols.MaJamGameSettlement, jsonData.DocUID)
    if (!settlementSnapshot) {
        let errMessage = "Something wrong when EndGame settlementSnapshot." + jsonData.DocUID
        console.log("[ERROR] " + errMessage)
        res.json({
            "Result": errMessage,
            "Data": {}
        });
        return;
    }

    // ----------------------整理&計算↓
    let uidToIndex = {}
    for (let i = 0; i < jsonData.Players.length; i++) {
        uidToIndex[jsonData.Players[i]] = i
    }
    let playerDatas = {
        UIDToIndex: uidToIndex,
        uid: jsonData.Players,
        isAI: jsonData.IsAI,
        totalBet: jsonData.TotalScores,
        totalBall: jsonData.TotalBalls,
        addExp: [0, 0, 0, 0],
        // VIP
        vipData: [null, null, null, null]
    };

    // 傳進來已經有照分數排序了
    let getExpIdx = 0
    for (let i = 0; i < jsonData.Players.length; i++) {
        if ((i > 0) && (playerDatas.totalBet[i - 1] > playerDatas.totalBet[i])) {
            getExpIdx = getExpIdx + 1
        }
        let oriAddExp = exp[getExpIdx]
        playerDatas.addExp[i] = oriAddExp
    }
    console.log("[normal] oriExp: " + JSON.stringify(playerDatas.addExp))

    // ----------------------發獎
    // addExp
    let playerNewExp = []
    for (let i = 0; i < jsonData.Players.length; i++) {
        if (playerDatas.isAI[i]) {
            playerNewExp.push(0)
            continue
        }
        let uid = playerDatas.uid[i]
        const player = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, uid)
        if (!player) {
            playerNewExp.push(0)
            continue
        }
        let oldExp = 0
        if ("EXP" in player) {
            oldExp = player["EXP"]
        }
        let revisedExp = playerDatas.addExp[i]
        let vipLv = 0
        if ("VIP" in player) {
            vipLv = player["VIP"]
        }
        playerDatas.vipData[i] = VIPManager.GetVIPData(vipLv)
        if ((playerDatas.vipData[i]) &&
            ("LVPointBonus" in playerDatas.vipData[i])) {
            let bonusRate = Number(playerDatas.vipData[i]["LVPointBonus"])
            revisedExp = Math.floor(((100 + bonusRate) * playerDatas.addExp[i]) / 100)
            playerDatas.addExp[i] = revisedExp
        }

        let newExp = oldExp + revisedExp
        let update = {
            EXP: newExp
        }
        playerNewExp.push(newExp)
        console.log("[normal] player: " + uid + " [oldEXP => newEXP] :", oldExp, playerDatas.addExp[i], "(", playerDatas.vipData[i]["LVPointBonus"], ")", newExp)
        FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, uid, update);
    }
    console.log("[normal] playerNewExp: " + JSON.stringify(playerNewExp))

    // ----------------------結算臨時錢包 ↓
    const lang = "TW_TW"
    for (let i = 0; i < playerDatas.uid.length; i++) {
        let uid = playerDatas.uid[i]
        let playerIndex = uidToIndex[uid]

        const pAllBag = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.PlayerTmpBag, uid)
        if (!pAllBag) {
            continue
        }
        console.log("[tmpGameBag] player: " + uid + " allTmpGameBag" + JSON.stringify(pAllBag))
        let gameTmpBag = {}
        if ((pAllBag) &&
            (jsonData.DocUID in pAllBag)) {
            gameTmpBag = pAllBag[jsonData.DocUID]
        } else {
            continue
        }

        const notifyZeroOn = true
        let items = []
        for (let cType in gameTmpBag) {
            if (cType == "CreateTime") {
                continue
            }
            if (gameTmpBag[cType] <= 0) {
                console.log("[tmpGameBag] player:" + uid + " ,tmpGameBagItem:" + cType + ": " + gameTmpBag[cType] + " <= 0")
                continue
            }
            let item = {
                ItemType: cType,
                ItemValue: gameTmpBag[cType]
            }
            items.push(item)
        }
        console.log("[tmpGameBag] player:" + uid + " ,allMailItems: ", JSON.stringify(items))

        if (items.length > 0) {
            let mailItem = []
            let titleTemplate = GameDataManager.GetStr("UI_ReplaceMail_Item", lang);
            for (let i = 0; i < items.length; i++) {
                mailItem.push(items[i])
                console.log("[tmpGameBag] player:" + uid + "pushMailItem: ", JSON.stringify(mailItem), " length: ", mailItem.length)
                if ((mailItem.length > 0) && ((mailItem.length % 2 == 0) || (i == items.length - 1))) {
                    title = DealTmpGameBagBackItemMailTitle(titleTemplate, jsonData["GameDataRoomUID"], mailItem, lang)
                    let mailContent = {}
                    mailContent["Title"] = title
                    mailContent["OwnerUID"] = uid
                    mailContent["Items"] = mailItem
                    let mailResult = await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, mailContent);
                    console.log("[tmpGameBag] player: " + uid + " ,AMail: " + JSON.stringify(mailContent) + "=> Result: " + mailResult)
                    mailItem = []
                }
            }
        } else if (notifyZeroOn) {
            let titleTemplate = GameDataManager.GetStr("UI_ReplaceMail_None", lang);
            let title = DealTmpGameBagBackEmptyMailTitle(titleTemplate, jsonData["GameDataRoomUID"], lang)
            let mailContent = {}
            mailContent["Title"] = title
            mailContent["OwnerUID"] = uid
            let mailResult = await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, mailContent);
            console.log("[tmpGameBag] player: " + uid + " ,AMail: " + JSON.stringify(mailContent) + "=> Result: " + mailResult)
        }

        let docNum = 0
        for (let key in pAllBag) {
            if (key == "CreateTime") {
                continue
            }
            if (key == "UID") {
                continue
            }
            docNum = docNum + 1
        }
        if (docNum <= 1) {
            await FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.PlayerTmpBag, uid)
        } else {
            let deleteData = {}
            deleteData[jsonData.DocUID] = admin.firestore.FieldValue.delete()
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.PlayerTmpBag, uid, deleteData)
        }
    }

    // ----------------------LOG紀錄 && 回傳server ↓
    let playerAllReward = {}
    for (let i = 0; i < playerDatas.uid.length; i++) {
        let uid = playerDatas.uid[i]
        let reward = {}
        let playerIndex = uidToIndex[uid]

        // mainBet
        reward[roomBetType] = playerDatas.totalBet[playerIndex]

        // ball
        if (playerDatas.totalBall[playerIndex] > 0) {
            reward["Ball"] = playerDatas.totalBall[playerIndex]
        }

        // exp
        reward["EXP"] = playerDatas.addExp[playerIndex]

        playerAllReward[uid] = {}
        playerAllReward[uid] = reward
    }

    let endData = {
        PlayerAllReward: playerAllReward,
        BaseRound: jsonData.BaseRound,
        TotalRound: jsonData.TotalRound,
    }

    let UpdateSettlementData = {
        EndData: endData
    }

    await FirestoreManager.UpdateDoc(GameSetting.GameLogCols.MaJamGameSettlement, jsonData.DocUID, UpdateSettlementData);

    res.json({
        "Result": GameSetting.ResultTypes.Success,
        "Data": playerAllReward,
        "ExpData": playerNewExp
    });

    return;
});

// 必須參照程式相關Enum
const PlayerActionType = Object.freeze({
    "NoneAction": 0,
    "GroundingFlower": 1,
    "Draw": 2,
    "Discard": 4,
    "Skip": 8,
    "Chow": 16,
    "Pong": 32,
    "ExposedKong": 64,
    "ConcealedKong": 128,
    "AddKong": 256,
    "Win": 512,
    "GroundListen": 1024,
    "HeavenListen": 2048,
    "NotifyListen": 4096
});

// Lv2 (WinNameCounter) 項目
// 必須參照程式相關Enum
const WinNameEnum = Object.freeze({
    0: "Win_Dealer",
    1: "Win_ConcealedHand",
    2: "Win_ConcealedHandSelfDraw",
    3: "Win_SelfDraw",
    4: "Win_WordRed",
    5: "Win_WordWhite",
    6: "Win_WordGreen",
    7: "Win_WordEast",
    8: "Win_WordSouth",
    9: "Win_WordWest",
    10: "Win_WordNorth",
    11: "Win_RobbingAKong",
    12: "Win_HalfExposedHand",
    13: "Win_NotifyListen",
    14: "Win_ListenPartial",
    15: "Win_ListenMiddle",
    16: "Win_ListenOne",
    17: "Win_WinOnAKong",
    18: "Win_FinalDrawSelf",
    19: "Win_FinalDrawOther",
    20: "Win_Flower",
    21: "Win_AllSequence",
    22: "Win_ExposedHand",
    23: "Win_NoWordFlower",
    24: "Win_ThreeConcealedTriplets",
    25: "Win_AllTriplets",
    26: "Win_MixedOneSuit",
    27: "Win_SmallThreeDragons",
    28: "Win_ListenOfHeavenEarth",
    29: "Win_BigThreeDragon",
    30: "Win_FourWinds",
    31: "Win_PureOneSuit",
    32: "Win_AllHonors",
    33: "Win_BlessingOfHuman",
    34: "Win_BlessingOfHeavenEarth",
    35: "Win_EightFlower",
    36: "Win_RobbingAWin",
});


// 週期項目
const HistoryRecorder = Object.freeze([
    "MajamHistory",
    "WeeklyMajamHistory",
    "MonthlyMajamHistory",
]);

// Lv1屬性
const HistoryL1Content = Object.freeze([
    "GameTimes",
    "RoundTimes",
    "WinGameTimes",
    "WinRoundTimes",
    "RoundMaxWinPoint",
    "RoundMaxWinBall",
    "SelfDrawTimes",
    "ChunkTimes",
    "GoldBalance",
    "PointBalance",
    "VIPAdmitGameTimes",
    "VIPAdmitRoundTimes",
    "EarnGold",
    "EarnPoint",
    "EarnBall",
    "RoundMaxWinGold",
    "GameMaxWinGold",
]);

// Lv1中不適用累加的屬性
const HistoryL1ContentIgnoreIncrement = Object.freeze([
    "RoundMaxWinPoint", //最高台數
    "RoundMaxWinBall",
    "RoundMaxWinGold",
    "GameMaxWinGold",
]);

// Lv1中需要預讀來做比較或累加的項目
const HistoryL1ContentNeedPreLoad = Object.freeze([
    "RoundMaxWinPoint", //最高台數
    "RoundMaxWinBall",
    "RoundMaxWinGold",
    "GameMaxWinGold",
]);


// 單純標註記錄什麼
const PlayerWinPeriodDetail = Object.freeze([
    "LastWin",
    "KeepWin",
    "KeepPeace",
    "IntervalBalance",
]);


function CreateDefaultUpdate() {
    let defaultObj = {};
    for (let r = 0; r < HistoryRecorder.length; r++) {
        for (let l1c = 0; l1c < HistoryL1Content.length; l1c++) {
            defaultObj[HistoryRecorder[r] + "." + HistoryL1Content[l1c]] = 0
        }
        for (let l2c = 0; l2c < Object.values(WinNameEnum).length; l2c++) {
            defaultObj[HistoryRecorder[r] + "." + "WinNameCounter." + WinNameEnum[l2c]] = 0;
        }
    }
    return defaultObj;
}

function FormatToAddData(sourceData) {
    let addData = {}
    for (let path in sourceData) {
        let way = path.split('.')
        let last = way.pop();
        way.reduce(function (o, k, i, kk) {
            return o[k] = o[k] || (isFinite(i + 1 in kk ? kk[i + 1] : last) ? [] : {});
        }, addData)[last] = sourceData[path];
    }
    return addData
}

function CheckRound(round) {
    const dealer = round.Dealer;
    let tileStart = 0;
    let tileEnd = 143;
    let playerTile = [[], [], [], []];
    //先發牌
    for (let i = 0; i < 10; i++) { // 每人拿10張
        for (let j = 0; j < 4; j++) { // 4人輪流拿
            let playerIndex = (dealer + j) % 4;
            //每次拿1張
            for (let k = 0; k < 1; k++) {
                playerTile[playerIndex].push(round.TileSequence[tileStart++]);
            }
        }
    }
    playerTile[dealer].push(round.TileSequence[tileStart++]);
    //按照行動檢查摸牌對不對
    let lastAction = 0;
    let lastDiscardTile = -1;
    round.PlayerAction.forEach((action) => {
        switch (action.PlayerActionType) {
            case PlayerActionType.GroundingFlower://補花
            case PlayerActionType.Discard://打牌
            case PlayerActionType.GroundListen://地聽
            case PlayerActionType.NotifyListen://嗆聽
                let index = playerTile[action.PlayerIndex].indexOf(action.ActionTile);
                if (index < 0) {
                    console.warn("[ERROR] draw card Error" + JSON.stringify(action));
                    return false;//他沒有那張牌
                }
                lastDiscardTile = action.ActionTile;
                playerTile[action.PlayerIndex].splice(index, 1);
                break;
            case PlayerActionType.Draw://摸牌
                let tile = -1;
                if (lastAction == 1 || lastAction == 64 || lastAction == 128 || lastAction == 256)
                    tile = round.TileSequence[tileEnd--];
                else
                    tile = round.TileSequence[tileStart++];
                if (tile != action.ActionTile) {
                    console.warn("[ERROR] draw card Error" + JSON.stringify(action));
                    return false;//摸牌錯誤
                }
                playerTile[action.PlayerIndex].push(tile);
                break;
            case PlayerActionType.Chow://吃牌
            case PlayerActionType.Pong://碰牌
            case PlayerActionType.ExposedKong://槓牌
            case PlayerActionType.Win://胡牌
                playerTile[action.PlayerIndex].push(lastDiscardTile);
                break;
        }
        lastAction = action.PlayerActionType;
    });
    //檢查最後分數加總否正確
    if (round.ScoreChange != null) {
        let sumScore = 0;
        for (let i = 0; i < 4; i++) {
            sumScore += round.ScoreChange[i];
        }
        if (sumScore != 0) {
            functions.logger.log('round score Error' + JSON.stringify(round.ScoreChange));
            return false;
        }
    }
    return true;
}

// RecordMaJamHistory 檢查麻將遊玩資料
exports.RecordMaJamHistory = functions.region('asia-east1').firestore.document(GameSetting.GameLogCols.MaJamGameTrack + "/{roomID}").onCreate(async (snap, context) => {
    // Grab the current value of what was written to Firestore.
    // console.log(snap);
    console.log("[normal] RecordMaJamHistory, docUID: " + context.params.roomID);
    // GameData資料檢查
    let roomGameData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.MaJamRoom, snap.data().GameDataRoomUID);
    // console.log("roomGameData: " + JSON.stringify(roomGameData));
    const checkRoomGameDataKey = ["BetType"]
    checkRoomGameDataKey.forEach(key => {
        if ((roomGameData) && (!(key in roomGameData))) {
            let errMessage = "RoomUID: " + JSON.stringify(snap.data().GameDataRoomUID) + ", GameData with wrong " + key + ": " + JSON.stringify(roomGameData)
            console.warn("[ERROR] " + errMessage);
        }
    })
    const GameLog = snap.data().GameLog;
    const BetType = snap.data().BetType;
    let IsRoomVIPAdmit = false
    IsRoomVIPAdmit = (roomGameData) && (roomGameData["VIPAdmit"])
    let isValid = true;
    let updates = {}
    let needAdd = {}
    let updatePlayers = {}
    let needUpdatePlayer = {}

    const historyDatas = [];
    for (let index = 0; index < GameLog.Players.length; index++) {
        if (GameLog.IsAI[index]) {
            continue;
        }
        let uid = GameLog.Players[index]
        if (uid === null || uid == "") {
            console.warn('[ERROR] MaJamGameLog' + context.params.roomID + "ID NULL.");
            isValid = false;
            continue;
        }
        const p = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, uid)
        updatePlayers[uid] = {}
        updates[uid] = {}
        updates[uid] = CreateDefaultUpdate()
        needAdd[uid] = false
        needUpdatePlayer[uid] = false
        if (!p) {
            needAdd[uid] = true
        } else {
            historyDatas.push(p);
        }
    }

    //console.log("[ERROR] " + docDatas)
    for (let historyData of historyDatas) {
        if (!historyData) {
            return
        }

        // 週期麻將紀錄preLoad.
        let uid = historyData.UID
        for (let r = 0; r < HistoryRecorder.length; r++) {
            for (let l1c = 0; l1c < HistoryL1ContentNeedPreLoad.length; l1c++) {
                let fieldPath = HistoryRecorder[r] + "." + HistoryL1ContentNeedPreLoad[l1c]
                if ((HistoryRecorder[r] in historyData) &&
                    (HistoryL1ContentNeedPreLoad[l1c] in historyData[HistoryRecorder[r]])) {
                    updates[uid][fieldPath] = historyData[HistoryRecorder[r]][HistoryL1ContentNeedPreLoad[l1c]];
                } else {
                    updates[uid][fieldPath] = 0;
                }
            }
        }

        // 連續勝負狀況紀錄preLoad.
        for (let di = 0; di < PlayerWinPeriodDetail.length; di++) {
            let fieldPath = "WinPeriod." + PlayerWinPeriodDetail[di]
            if (("WinPeriod" in historyData) &&
                (PlayerWinPeriodDetail[di] in historyData["WinPeriod"])) {
                updates[uid][fieldPath] = historyData["WinPeriod"][PlayerWinPeriodDetail[di]];
            } else {
                updates[uid][fieldPath] = 0;
            }
        }
    }

    // console.log("[normal] needUpdatePlayer: ", JSON.stringify(needUpdatePlayer))
    // console.log("[normal] updatePlayers: ", JSON.stringify(updatePlayers))

    // Game統計
    GameLog.Players.forEach((uid, index, arr) => {
        console.log("[normal] GameRecord: " + uid + " index: " + index)
        if (GameLog.IsAI[index]) {
            return;
        }
        if (uid === null || uid == "") {
            console.warn('[ERROR] MaJamGameLog', context.params.roomID, "ID NULL.");
            isValid = false;
            return;
        }

        // 週期麻將紀錄
        for (let r = 0; r < HistoryRecorder.length; r++) {
            updates[uid][HistoryRecorder[r] + ".GameTimes"] = 1
            updates[uid][HistoryRecorder[r] + "." + BetType + "Balance"] = GameLog.FinalScore[index]
            updates[uid][HistoryRecorder[r] + ".RoundTimes"] = GameLog.Rounds.length
            if (GameLog.FinalScore[index] > 0) {
                updates[uid][HistoryRecorder[r] + ".WinGameTimes"] = 1
                updates[uid][HistoryRecorder[r] + "." + "Earn" + BetType] = GameLog.FinalScore[index]
            }
            if (GameLog.FinalScore[index] > updates[uid][HistoryRecorder[r] + ".GameMaxWinGold"]) {
                updates[uid][HistoryRecorder[r] + ".GameMaxWinGold"] = GameLog.FinalScore[index]
            }
            if (IsRoomVIPAdmit) {
                updates[uid][HistoryRecorder[r] + ".VIPAdmitGameTimes"] = 1
                updates[uid][HistoryRecorder[r] + ".VIPAdmitRoundTimes"] = GameLog.Rounds.length
            }
        }

        // 玩家連輸連贏紀錄
        console.log("[WinPeriod]Before Player: " + uid +
            " ,IntervalBalance:" + JSON.stringify(updates[uid]["WinPeriod.IntervalBalance"]) +
            " ,LastWin: " + JSON.stringify(updates[uid]["WinPeriod.LastWin"]) +
            " ,KeepWin: " + JSON.stringify(updates[uid]["WinPeriod.KeepWin"]) +
            " ,KeepPeace:" + JSON.stringify(updates[uid]["WinPeriod.KeepPeace"]))
        if (GameLog.FinalScore[index] == 0) {
            updates[uid]["WinPeriod.KeepPeace"] = admin.firestore.FieldValue.increment(1)
            updates[uid]["WinPeriod.LastWin"] = 0
        } else if (GameLog.FinalScore[index] > 0) {
            if (updates[uid]["WinPeriod.LastWin"] == 1) {
                updates[uid]["WinPeriod.KeepWin"] = admin.firestore.FieldValue.increment(1)
                updates[uid]["WinPeriod.IntervalBalance"] = admin.firestore.FieldValue.increment(GameLog.FinalScore[index])
            } else {
                updates[uid]["WinPeriod.KeepWin"] = 1
                updates[uid]["WinPeriod.IntervalBalance"] = GameLog.FinalScore[index]
            }
            updates[uid]["WinPeriod.LastWin"] = 1
        } else if (GameLog.FinalScore[index] < 0) {
            if (updates[uid]["WinPeriod.LastWin"] == -1) {
                updates[uid]["WinPeriod.KeepWin"] = admin.firestore.FieldValue.increment(-1)
                updates[uid]["WinPeriod.IntervalBalance"] = admin.firestore.FieldValue.increment(GameLog.FinalScore[index])
            } else {
                updates[uid]["WinPeriod.KeepWin"] = -1
                updates[uid]["WinPeriod.IntervalBalance"] = GameLog.FinalScore[index]
            }
            updates[uid]["WinPeriod.LastWin"] = -1
        }
        console.log("[WinPeriod]After Player: " + uid +
            " ,IntervalBalance:" + JSON.stringify(updates[uid]["WinPeriod.IntervalBalance"]) +
            " ,LastWin: " + JSON.stringify(updates[uid]["WinPeriod.LastWin"]) +
            " ,KeepWin: " + JSON.stringify(updates[uid]["WinPeriod.KeepWin"]) +
            " ,KeepPeace:" + JSON.stringify(updates[uid]["WinPeriod.KeepPeace"]))

        // ActiveVIPEXP
        if (IsRoomVIPAdmit) {
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
            let postResult = Postman.SendPost(VIP_AddEXP_URL, {
                "PlayerUID": uid,
                "VIPType": GameSetting.VIPType.Active,
                "Value": 1,
            })
        }
    });

    // Round統計
    GameLog.Rounds.forEach((round) => {
        isValid = isValid && CheckRound(round);
        let chuckRecord = false;
        round.WinnerInfos.forEach((winner) => {
            if (GameLog.IsAI[winner.PlayerIndex]) {
                // 玩家可能放槍給AI
                if (!chuckRecord) {
                    //應該只有一位玩家放槍 一砲三響應該也只算放一次槍
                    let loserUID = GameLog.Players[winner.LosePlayers[0]];
                    if (!GameLog.IsAI[winner.LosePlayers[0]]) {
                        for (let r = 0; r < HistoryRecorder.length; r++) {
                            updates[loserUID][HistoryRecorder[r] + ".ChunkTimes"] += 1
                        }
                    }
                    chuckRecord = true;
                }
                return;
            }

            const winnerUID = GameLog.Players[winner.PlayerIndex];

            for (let r = 0; r < HistoryRecorder.length; r++) {
                updates[winnerUID][HistoryRecorder[r] + ".WinRoundTimes"] += 1
                if (round.BallChange[winner.PlayerIndex] > 0) {
                    updates[winnerUID][HistoryRecorder[r] + ".EarnBall"] += round.BallChange[winner.PlayerIndex]
                }
                if (winner.WinPoint > updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinPoint"]) {
                    //console.log("[normal] (最高台數)WinPoint > MaxWinPoint: ", winner.WinPoint, " > ", updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinPoint"])
                    updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinPoint"] = winner.WinPoint;
                }
                if (round.BallChange[winner.PlayerIndex] > updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinBall"]) {
                    //console.log("[normal] (最高台數)WinBall > MaxWinBall: ", round.BallChange[winner.PlayerIndex], " > ", updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinBall"])
                    updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinBall"] = round.BallChange[winner.PlayerIndex];
                }
                if (round.ScoreChange[winner.PlayerIndex] > updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinGold"]) {
                    //console.log("[normal] WinGold > MaxWinGold: ", round.ScoreChange, " > ", updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinGold"])
                    updates[winnerUID][HistoryRecorder[r] + ".RoundMaxWinGold"] = round.ScoreChange[winner.PlayerIndex];
                }
                winner.MaJamWinName.forEach((winName) => {
                    updates[winnerUID][HistoryRecorder[r] + ".WinNameCounter." + WinNameEnum[winName]] += 1
                });
            }
            if (winner.IsSelfDraw) {
                for (let r = 0; r < HistoryRecorder.length; r++) {
                    updates[winnerUID][HistoryRecorder[r] + ".SelfDrawTimes"] += 1
                }
            } else if (!chuckRecord) {
                //應該只有一位玩家放槍 一砲三響應該也只算放一次槍
                let loserUID = GameLog.Players[winner.LosePlayers[0]];
                if (!GameLog.IsAI[winner.LosePlayers[0]]) {
                    for (let r = 0; r < HistoryRecorder.length; r++) {
                        updates[loserUID][HistoryRecorder[r] + ".ChunkTimes"] += 1
                    }
                }
                chuckRecord = true;
            }
        })
    });

    if (isValid) {
        GameLog.Players.forEach((uid, index, arr) => {
            if (GameLog.IsAI[index]) {
                return;
            }
            if (uid === null || uid == "") {
                return;
            }
            // 統一increment
            //console.log("[normal] NeedIgnore: ", HistoryL1ContentIgnoreIncrement)
            for (let r = 0; r < HistoryRecorder.length; r++) {
                for (let l1c = 0; l1c < HistoryL1Content.length; l1c++) {
                    if (HistoryL1ContentIgnoreIncrement.indexOf(HistoryL1Content[l1c]) >= 0) {
                        // console.log("[normal] Ignore: ", HistoryL1Content[l1c])
                        continue
                    }
                    updates[uid][HistoryRecorder[r] + "." + HistoryL1Content[l1c]] = admin.firestore.FieldValue.increment(updates[uid][HistoryRecorder[r] + "." + HistoryL1Content[l1c]]);
                }
                for (let l2c = 0; l2c < Object.values(WinNameEnum).length; l2c++) {
                    updates[uid][HistoryRecorder[r] + "." + "WinNameCounter." + WinNameEnum[l2c]] = admin.firestore.FieldValue.increment(updates[uid][HistoryRecorder[r] + "." + "WinNameCounter." + WinNameEnum[l2c]]);
                }
            }
            if (needAdd[uid]) {
                let addData = FormatToAddData(updates[uid])
                console.log("[normal] FirstTimeRecordMaJamHistory, :" + JSON.stringify(updates[uid]))
                FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, uid, addData);
            } else {
                console.log("[normal] UpdateRecordMaJamHistory, :" + JSON.stringify(updates[uid]))
                FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, uid, updates[uid]);
            }
            if (needUpdatePlayer[uid]) {
                console.log("[normal] UpdatePlayerData :" + JSON.stringify(updatePlayers[uid]))
                FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, uid, updatePlayers[uid]);
            }
        });
    }

    return snap.ref.set({ 'IsValidGame': isValid }, { merge: true });
});

