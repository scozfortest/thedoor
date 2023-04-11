//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
const axios = require("axios");
//自訂方法
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameSetting = require('./GameTools/GameSetting.js');
const MyTime = require('./Scoz/MyTime.js');
const Logger = require('./GameTools/Logger.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const { GameDataCols } = require('./GameTools/GameSetting.js');

//一段時間自動把超時未更新的上線玩家設回下線
exports.Crontab_PlayerOnlineStateUpdate = functions.region('asia-east1').runWith({ timeoutSeconds: 540, memory: '8GB' }).pubsub.schedule('* * * * *')//* * * * * (Every minute)
    .timeZone('Asia/Taipei').onRun(async context => {


        let playerDocs = await FirestoreManager.GetDocs_WhereOperation(GameSetting.PlayerDataCols.Player, "State", '!=', "Offline");
        if (playerDocs == null) return;//沒有上線中的玩家就直接返回
        //console.log("上線中的玩家數=" + playerDocs.length)

        let onlinePlayerUIDs = [];//現在上線的玩家UID清單
        for (let doc of playerDocs) {
            onlinePlayerUIDs.push(doc.id);
        }

        if (onlinePlayerUIDs.length <= 0) return;
        let gameSettingTimerDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "Timer");
        let onlineTimeStampCD = gameSettingTimerDocData["OnlineTimeStampCD"];//玩家上線時間戳更新CD

        //取得上線中的玩家TimestampDocs
        let onlineTimestampDocs = await FirestoreManager.GetDocs_WhereIn(GameSetting.PlayerDataCols.OnlineTimestamp, "UID", onlinePlayerUIDs);
        if (onlineTimestampDocs == null) return;
        let now = admin.firestore.Timestamp.now().toDate();//取得現在時間
        //上線中的玩家TimestampDocs的上次更新時間超過一定時間就代表玩家下線了，設為下線狀態並寫入登出時間
        for (let doc of onlineTimestampDocs) {
            let updateTime = doc.data()["UpdateTime"];
            let checkTime = MyTime.AddSecs(updateTime.toDate(), onlineTimeStampCD * 60);//上次登入時間戳距離現在GameData-Setting->Timer->OnlineTimeStampCD設定的分鐘數*60秒以上就標示為離線
            if (now > checkTime) {
                //console.log("玩家設為下線:" + doc.id);
                let updateData = {
                    State: "Offline",
                    LastSignoutTime: admin.firestore.Timestamp.now(),
                }
                FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, doc.id, updateData);
                Logger.Signout(doc.id);//寫登出LOG
            }
        }

    });

//一段時間自動移除超時未更新的好友配對房間
exports.Crontab_MaJamMatchingRoomTimeout = functions.region('asia-east1').runWith({ timeoutSeconds: 540, memory: '8GB' }).pubsub.schedule('* * * * *')//* * * * * (Every minute)
    .timeZone('Asia/Taipei').onRun(async context => {

        let maJamMatchingRoomDocs = await FirestoreManager.GetAllDocs(GameSetting.PlayerDataCols.MaJamMatchingRoom);

        if (maJamMatchingRoomDocs == null)
            return;//沒有配對中的房間就直接返回
        //console.log("配對中的房間數=" + maJamMatchingRoomDocs.length);
        let now = admin.firestore.Timestamp.now().toDate();//取得現在時間
        let gameSettingTimerDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "Timer");
        let maJamMatchingRoomTimeoutSecs = gameSettingTimerDocData["MaJamMatchingRoomTimeoutSecs"];//好友配對房間時間戳沒更新後經過這個秒數會自動移除
        //配對房間中的上次更新時間的上次更新時間超過一定時間就代表房間已經關閉或房主已經離線了，移除房間
        for (let doc of maJamMatchingRoomDocs) {
            let lastUpdateTime = doc.data().LastUpdateTime;
            let checkTime = MyTime.AddSecs(lastUpdateTime.toDate(), maJamMatchingRoomTimeoutSecs);
            if (now > checkTime) {
                FirestoreManager.DeleteDocByUID(GameSetting.PlayerDataCols.MaJamMatchingRoom, doc.id);
                //console.log("移除房間:" + doc.id);
            }
        }

    });


//移除未領取的限時信件
exports.Crontab_RemoveTimeLimitMail = functions.region('asia-east1').runWith({ timeoutSeconds: 540, memory: '8GB' }).pubsub.schedule('0 0 * * *')//* * * * * (Every Midnight At 00:00)
    .timeZone('Asia/Taipei').onRun(async context => {

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
                    Logger.Mail_Remove(mail.data()["OwnerUID"], mail.data());
                }
            }
        }
        if (removeUIDs.length > 0)
            await FirestoreManager.DeleteDocsByUIDs(GameSetting.PlayerDataCols.Mail, removeUIDs);


        console.log("執行完畢- Crontab_RemoveTimeLimitMail");

    });

//每分鐘更新排行榜資料
exports.Crontab_UpdateLeaderboard = functions.region('asia-east1').runWith({ timeoutSeconds: 540, memory: '8GB' }).pubsub.schedule('* * * * *')//每分鐘
    .timeZone('Asia/Taipei').onRun(async context => {

        console.log("開始執行- Crontab_UpdateLeaderboard");
        let now = admin.firestore.Timestamp.now();//取得現在時間

        ///////////////////////////////////////////////////////Gold排行榜
        let leaderBoardData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.LeaderBoard, "Gold");
        let leaderBoardSettingData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "LeaderBoard");
        let enable = leaderBoardSettingData["Gold"]["Enable"];//是否開啟此排行榜
        let gold_rankCount = leaderBoardSettingData["Gold"]["RankCount"];//排到第幾名
        let enableReward = leaderBoardSettingData["Gold"]["EnableReward"];//是否開啟結算給獎功能
        let sentReward = leaderBoardSettingData["Gold"]["SentReward"];//是否已經結算
        let settleTime = leaderBoardSettingData["Gold"]["SettleTime"];//結算時間
        let rewards = leaderBoardSettingData["Gold"]["Rewards"];//獎勵清單

        //有開啟此排行榜才需要跑後面
        console.log("Gold排行榜是否開啟: " + enable);
        if (enable) {
            let playerDocs = await FirestoreManager.GetDocs_OrderLimit(GameSetting.PlayerDataCols.Player, "Gold", true, gold_rankCount);
            if (playerDocs != null) {
                let tmpDatas = [];//紀錄排行榜內玩家清單
                playerDocs.forEach(playerDoc => {
                    let Data = {
                        PlayerUID: playerDoc.data().UID,
                        Value: playerDoc.data().Gold,
                        Name: playerDoc.data().Name,
                    }
                    tmpDatas.push(Data);
                });

                //發送結算獎勵
                let rewardMailDatas = [];
                let logData = null;
                if (enableReward == true && sentReward == false && now >= settleTime &&
                    rewards != null && rewards.length > 0) {
                    console.log("開始發送Gold結算獎勵");
                    logData = {
                        Type: "Gold",
                        RankCount: gold_rankCount,
                        SendMailDatas: [],
                    }
                    for (let i = 0; i < rewards.length; i++) {
                        let itemType = rewards[i]["ItemType"];
                        let itemValue = rewards[i]["ItemValue"];
                        let mailName = rewards[i]["MailName"];
                        let rankStart = Number(rewards[i]["RankStart"]) - 1;
                        if (rankStart < 0)
                            rankStart = 0;
                        let rankEnd = Number(rewards[i]["RankEnd"]);
                        if (rankEnd > tmpDatas.length)
                            rankEnd = tmpDatas.length;
                        for (let j = rankStart; j < rankEnd; j++) {
                            if (typeof tmpDatas[j] === 'undefined')
                                continue;
                            //信件資料設定
                            rewardMailDatas.push({
                                ColName: GameSetting.PlayerDataCols.Mail,
                                OwnerUID: tmpDatas[j]["PlayerUID"],
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                                Title: mailName,
                            });
                            //log資料設定
                            logData["SendMailDatas"].push({
                                PlayerUID: tmpDatas[j]["PlayerUID"],
                                Title: mailName,
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                            });
                        }
                    }
                    await FirestoreManager.UpdateDoc(GameDataCols.Setting, "LeaderBoard", { "Gold.SentReward": true });//DB寫入為已經結算狀態
                    if (rewardMailDatas.length > 0)
                        await FirestoreManager.AddDocs(rewardMailDatas);//發送獎勵信件
                    await Logger.LeaderBoardReward(logData);//寫Log資料
                    console.log("結束發送Gold結算獎勵");
                }



                //排行榜有玩家進到前三名且名次大於上次的名次就送跑馬燈
                for (let i = 0; i < 3; i++) {
                    if (i < tmpDatas.length) {
                        let rankUp = false;//此玩家是否比上次名次還高
                        if (leaderBoardData == null || leaderBoardData["Datas"] == null) {//沒有上一次的排行榜 代表玩家名次一定高於上一次
                            rankUp = true;
                        } else {
                            if (i >= leaderBoardData["Datas"].length) {//上次排行榜沒有資料代表玩家名次一定高於上一次
                                rankUp = true;
                            } else {
                                let lastInRank = false;//上次是否也在前三名
                                let lastRank = 99999;//上次排名
                                for (let j = 0; j < 3; j++) {
                                    if (j >= leaderBoardData["Datas"].length)
                                        break;
                                    //如果上一次前三名也有此玩家 但排名比這次還後面
                                    //console.log("比較 " + tmpDatas[i]["PlayerUID"] + "  ,  " + leaderBoardData["Datas"][j]["PlayerUID"]);
                                    if (tmpDatas[i]["PlayerUID"] == leaderBoardData["Datas"][j]["PlayerUID"]) {
                                        lastInRank = true;
                                        lastRank = j;
                                        //console.log(tmpDatas[i]["PlayerUID"] + "這次是" + i + "名" + "上次是" + lastRank + "名");
                                        break;
                                    }
                                }
                                //console.log("lastInRank=" + lastInRank);
                                //console.log("lastRank=" + lastRank);
                                //console.log("curRank=" + i);
                                if (!lastInRank || i < lastRank)//上次不在前三或是上次排名比這次還差
                                    rankUp = true;
                            }
                        }
                        if (rankUp) {//名次比上次還高才送跑馬燈
                            //新玩家送跑馬燈公告
                            let addData = {
                                PlayerUID: tmpDatas[i]["PlayerUID"],
                                MsgType: "RankUp_GoldMsg",
                                MsgParams: [tmpDatas[i]["Name"], (i + 1)],//名次為i+1
                            }
                            await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addData);
                        }
                    }
                }



                let updateData = {
                    UID: "Gold",
                    Datas: tmpDatas,
                }
                await FirestoreManager.UpdateDoc(GameSetting.GameDataCols.LeaderBoard, "Gold", updateData);
            }
        }



        ///////////////////////////////////////////////////////////GameMaxWinGold排行榜(單場最高台數)
        leaderBoardData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.LeaderBoard, "GameMaxWinGold");
        playerDocs = null;
        enable = leaderBoardSettingData["GameMaxWinGold"]["Enable"];//排到第幾名
        gold_rankCount = leaderBoardSettingData["GameMaxWinGold"]["RankCount"];//排到第幾名
        enableReward = leaderBoardSettingData["GameMaxWinGold"]["EnableReward"];//是否開啟結算給獎功能
        sentReward = leaderBoardSettingData["GameMaxWinGold"]["SentReward"];//是否已經結算
        settleTime = leaderBoardSettingData["GameMaxWinGold"]["SettleTime"];//結算時間
        rewards = leaderBoardSettingData["GameMaxWinGold"]["Rewards"];//獎勵清單
        let gameMaxWinGold_rankCount = leaderBoardSettingData["GameMaxWinGold"]["RankCount"];

        //有開啟此排行榜才需要跑後面
        console.log("GameMaxWinGold排行榜是否開啟: " + enable);
        if (enable) {

            playerDocs = await FirestoreManager.GetDocs_OrderLimit(GameSetting.PlayerDataCols.History, "MajamHistory.GameMaxWinGold", true, gameMaxWinGold_rankCount);
            if (playerDocs != null) {
                let tmpDatas = [];
                for (let i = 0; i < playerDocs.length; i++) {
                    let docData = playerDocs[i].data();
                    let data = {
                        PlayerUID: docData.UID,
                        Value: Number(docData["MajamHistory"]["GameMaxWinGold"]),
                    }
                    tmpDatas.push(data);
                }


                //發送結算獎勵
                let rewardMailDatas = [];
                let logData = null;
                if (enableReward == true && sentReward == false && now >= settleTime &&
                    rewards != null && rewards.length > 0) {
                    console.log("開始發送GameMaxWinGold結算獎勵");
                    logData = {
                        Type: "GameMaxWinGold",
                        RankCount: gold_rankCount,
                        SendMailDatas: [],
                    }
                    for (let i = 0; i < rewards.length; i++) {
                        let itemType = rewards[i]["ItemType"];
                        let itemValue = rewards[i]["ItemValue"];
                        let mailName = rewards[i]["MailName"];
                        let rankStart = Number(rewards[i]["RankStart"]) - 1;
                        if (rankStart < 0)
                            rankStart = 0;
                        let rankEnd = Number(rewards[i]["RankEnd"]);
                        if (rankEnd > tmpDatas.length)
                            rankEnd = tmpDatas.length;
                        for (let j = rankStart; j < rankEnd; j++) {
                            if (typeof tmpDatas[j] === 'undefined')
                                continue;
                            //信件資料設定
                            rewardMailDatas.push({
                                ColName: GameSetting.PlayerDataCols.Mail,
                                OwnerUID: tmpDatas[j]["PlayerUID"],
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                                Title: mailName,
                            });
                            //log資料設定
                            logData["SendMailDatas"].push({
                                PlayerUID: tmpDatas[j]["PlayerUID"],
                                Title: mailName,
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                            });
                        }
                    }
                    await FirestoreManager.UpdateDoc(GameDataCols.Setting, "LeaderBoard", { "GameMaxWinGold.SentReward": true });//DB寫入為已經結算狀態
                    if (rewardMailDatas.length > 0)
                        await FirestoreManager.AddDocs(rewardMailDatas);//發送獎勵信件
                    await Logger.LeaderBoardReward(logData);//寫Log資料
                    console.log("結束發送GameMaxWinGold結算獎勵");
                }

                //排行榜有玩家進到前三名且名次大於上次的名次就送跑馬燈
                for (let i = 0; i < 3; i++) {
                    if (i < tmpDatas.length) {
                        let rankUp = false;//此玩家是否比上次名次還高
                        if (leaderBoardData == null || leaderBoardData["Datas"] == null) {//沒有上一次的排行榜 代表玩家名次一定高於上一次
                            rankUp = true;
                        } else {
                            if (i >= leaderBoardData["Datas"].length) {//上次排行榜沒有資料代表玩家名次一定高於上一次
                                rankUp = true;
                            } else {
                                let lastInRank = false;//上次是否也在前三名
                                let lastRank = 99999;//上次排名
                                for (let j = 0; j < 3; j++) {
                                    if (j >= leaderBoardData["Datas"].length)
                                        break;
                                    //如果上一次前三名也有此玩家 但排名比這次還後面
                                    //console.log("比較 " + tmpDatas[i]["PlayerUID"] + "  ,  " + leaderBoardData["Datas"][j]["PlayerUID"]);
                                    if (tmpDatas[i]["PlayerUID"] == leaderBoardData["Datas"][j]["PlayerUID"]) {
                                        lastInRank = true;
                                        lastRank = j;
                                        //console.log(tmpDatas[i]["PlayerUID"] + "這次是" + i + "名" + "上次是" + lastRank + "名");
                                        break;
                                    }
                                }
                                //console.log("lastInRank=" + lastInRank);
                                //console.log("lastRank=" + lastRank);
                                //console.log("curRank=" + i);
                                if (!lastInRank || i < lastRank)//上次不在前三或是上次排名比這次還差
                                    rankUp = true;
                            }
                        }
                        if (rankUp) {//名次比上次還高才送跑馬燈
                            let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, tmpDatas[i]["PlayerUID"]);
                            if (playerDocData != null) {
                                //新玩家送跑馬燈公告
                                let addData = {
                                    PlayerUID: tmpDatas[i]["PlayerUID"],
                                    MsgType: "RankUp_GameMaxWinGold",
                                    MsgParams: [playerDocData["Name"], (i + 1)],//名次為i+1
                                }
                                await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addData);
                            }

                        }
                    }
                }


                let updateData = {
                    UID: "GameMaxWinGold",
                    Datas: tmpDatas,
                }
                await FirestoreManager.UpdateDoc(GameSetting.GameDataCols.LeaderBoard, "GameMaxWinGold", updateData);
            }
        }


        /////////////////////////////////////////////////////////RoundMaxWinGold排行榜(單局最高台數)
        leaderBoardData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.LeaderBoard, "RoundMaxWinGold");
        playerDocs = null;
        enable = leaderBoardSettingData["RoundMaxWinGold"]["Enable"];//排到第幾名
        gold_rankCount = leaderBoardSettingData["RoundMaxWinGold"]["RankCount"];//排到第幾名
        enableReward = leaderBoardSettingData["RoundMaxWinGold"]["EnableReward"];//是否開啟結算給獎功能
        sentReward = leaderBoardSettingData["RoundMaxWinGold"]["SentReward"];//是否已經結算
        settleTime = leaderBoardSettingData["RoundMaxWinGold"]["SettleTime"];//結算時間
        rewards = leaderBoardSettingData["RoundMaxWinGold"]["Rewards"];//獎勵清單
        let roundMaxWinGold_rankCount = leaderBoardSettingData["RoundMaxWinGold"]["RankCount"];

        //有開啟此排行榜才需要跑後面
        console.log("RoundMaxWinGold排行榜是否開啟: " + enable);
        if (enable) {

            playerDocs = await FirestoreManager.GetDocs_OrderLimit(GameSetting.PlayerDataCols.History, "MajamHistory.RoundMaxWinGold", true, roundMaxWinGold_rankCount);
            if (playerDocs != null) {
                let tmpDatas = [];
                for (let i = 0; i < playerDocs.length; i++) {
                    let docData = playerDocs[i].data();
                    let data = {
                        PlayerUID: docData.UID,
                        Value: Number(docData["MajamHistory"]["RoundMaxWinGold"]),
                    }
                    tmpDatas.push(data);
                }



                //發送結算獎勵
                let rewardMailDatas = [];
                let logData = null;
                if (enableReward == true && sentReward == false && now >= settleTime &&
                    rewards != null && rewards.length > 0) {
                    console.log("開始發送RoundMaxWinGold結算獎勵");
                    logData = {
                        Type: "RoundMaxWinGold",
                        RankCount: gold_rankCount,
                        SendMailDatas: [],
                    }
                    for (let i = 0; i < rewards.length; i++) {
                        let itemType = rewards[i]["ItemType"];
                        let itemValue = rewards[i]["ItemValue"];
                        let mailName = rewards[i]["MailName"];
                        let rankStart = Number(rewards[i]["RankStart"]) - 1;
                        if (rankStart < 0)
                            rankStart = 0;
                        let rankEnd = Number(rewards[i]["RankEnd"]);
                        if (rankEnd > tmpDatas.length)
                            rankEnd = tmpDatas.length;
                        for (let j = rankStart; j < rankEnd; j++) {
                            if (typeof tmpDatas[j] === 'undefined')
                                continue;
                            //信件資料設定
                            rewardMailDatas.push({
                                ColName: GameSetting.PlayerDataCols.Mail,
                                OwnerUID: tmpDatas[j]["PlayerUID"],
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                                Title: mailName,
                            });
                            //log資料設定
                            logData["SendMailDatas"].push({
                                PlayerUID: tmpDatas[j]["PlayerUID"],
                                Title: mailName,
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                            });
                        }
                    }
                    await FirestoreManager.UpdateDoc(GameDataCols.Setting, "LeaderBoard", { "RoundMaxWinGold.SentReward": true });//DB寫入為已經結算狀態
                    if (rewardMailDatas.length > 0)
                        await FirestoreManager.AddDocs(rewardMailDatas);//發送獎勵信件
                    await Logger.LeaderBoardReward(logData);//寫Log資料
                    console.log("結束發送RoundMaxWinGold結算獎勵");
                }



                //排行榜有玩家進到前三名且名次大於上次的名次就送跑馬燈
                for (let i = 0; i < 3; i++) {
                    if (i < tmpDatas.length) {
                        let rankUp = false;//此玩家是否比上次名次還高
                        if (leaderBoardData == null || leaderBoardData["Datas"] == null) {//沒有上一次的排行榜 代表玩家名次一定高於上一次
                            rankUp = true;
                        } else {
                            if (i >= leaderBoardData["Datas"].length) {//上次排行榜沒有資料代表玩家名次一定高於上一次
                                rankUp = true;
                            } else {
                                let lastInRank = false;//上次是否也在前三名
                                let lastRank = 99999;//上次排名
                                for (let j = 0; j < 3; j++) {
                                    if (j >= leaderBoardData["Datas"].length)
                                        break;
                                    //如果上一次前三名也有此玩家 但排名比這次還後面
                                    //console.log("比較 " + tmpDatas[i]["PlayerUID"] + "  ,  " + leaderBoardData["Datas"][j]["PlayerUID"]);
                                    if (tmpDatas[i]["PlayerUID"] == leaderBoardData["Datas"][j]["PlayerUID"]) {
                                        lastInRank = true;
                                        lastRank = j;
                                        //console.log(tmpDatas[i]["PlayerUID"] + "這次是" + i + "名" + "上次是" + lastRank + "名");
                                        break;
                                    }
                                }
                                //console.log("lastInRank=" + lastInRank);
                                //console.log("lastRank=" + lastRank);
                                //console.log("curRank=" + i);
                                if (!lastInRank || i < lastRank)//上次不在前三或是上次排名比這次還差
                                    rankUp = true;
                            }
                        }
                        if (rankUp) {//名次比上次還高才送跑馬燈
                            let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, tmpDatas[i]["PlayerUID"]);
                            if (playerDocData != null) {
                                //新玩家送跑馬燈公告
                                let addData = {
                                    PlayerUID: tmpDatas[i]["PlayerUID"],
                                    MsgType: "RankUp_RoundMaxWinGold",
                                    MsgParams: [playerDocData["Name"], (i + 1)],//名次為i+1
                                }
                                await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addData);
                            }

                        }
                    }
                }


                let updateData = {
                    UID: "RoundMaxWinGold",
                    Datas: tmpDatas,
                }
                await FirestoreManager.UpdateDoc(GameSetting.GameDataCols.LeaderBoard, "RoundMaxWinGold", updateData);
            }

        }






        /////////////////////////////////////////////////////////RoundMaxWinGold_Weekly週排行榜(週排行-單局最高台數)
        leaderBoardData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.LeaderBoard, "RoundMaxWinGold_Weekly");
        playerDocs = null;
        enable = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["Enable"];//排到第幾名
        gold_rankCount = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["RankCount"];//排到第幾名
        enableReward = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["EnableReward"];//是否開啟結算給獎功能
        sentReward = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["SentReward"];//是否已經結算
        settleTime = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["SettleTime"];//結算時間
        rewards = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["Rewards"];//獎勵清單
        let RoundMaxWinGold_Weekly_rankCount = leaderBoardSettingData["RoundMaxWinGold_Weekly"]["RankCount"];

        //有開啟此排行榜才需要跑後面
        console.log("RoundMaxWinGold_Weekly排行榜是否開啟: " + enable);
        if (enable) {

            playerDocs = await FirestoreManager.GetDocs_OrderLimit(GameSetting.PlayerDataCols.History, "WeeklyMajamHistory.RoundMaxWinGold", true, RoundMaxWinGold_Weekly_rankCount);
            if (playerDocs != null) {
                let tmpDatas = [];
                for (let i = 0; i < playerDocs.length; i++) {
                    let docData = playerDocs[i].data();
                    let data = {
                        PlayerUID: docData.UID,
                        Value: Number(docData["WeeklyMajamHistory"]["RoundMaxWinGold"]),
                    }
                    tmpDatas.push(data);
                }



                //發送結算獎勵
                let rewardMailDatas = [];
                let logData = null;
                if (enableReward == true && sentReward == false && now >= settleTime &&
                    rewards != null && rewards.length > 0) {
                    console.log("開始發送RoundMaxWinGold_Weekly結算獎勵");
                    logData = {
                        Type: "RoundMaxWinGold_Weekly",
                        RankCount: gold_rankCount,
                        SendMailDatas: [],
                    }
                    for (let i = 0; i < rewards.length; i++) {
                        let itemType = rewards[i]["ItemType"];
                        let itemValue = rewards[i]["ItemValue"];
                        let mailName = rewards[i]["MailName"];
                        let rankStart = Number(rewards[i]["RankStart"]) - 1;
                        if (rankStart < 0)
                            rankStart = 0;
                        let rankEnd = Number(rewards[i]["RankEnd"]);
                        if (rankEnd > tmpDatas.length)
                            rankEnd = tmpDatas.length;
                        for (let j = rankStart; j < rankEnd; j++) {
                            if (typeof tmpDatas[j] === 'undefined')
                                continue;
                            //信件資料設定
                            rewardMailDatas.push({
                                ColName: GameSetting.PlayerDataCols.Mail,
                                OwnerUID: tmpDatas[j]["PlayerUID"],
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                                Title: mailName,
                            });
                            //log資料設定
                            logData["SendMailDatas"].push({
                                PlayerUID: tmpDatas[j]["PlayerUID"],
                                Title: mailName,
                                Items: [{ ItemType: itemType, ItemValue: itemValue }],
                            });
                        }
                    }
                    await FirestoreManager.UpdateDoc(GameDataCols.Setting, "LeaderBoard", { "RoundMaxWinGold_Weekly.SentReward": true });//DB寫入為已經結算狀態
                    if (rewardMailDatas.length > 0)
                        await FirestoreManager.AddDocs(rewardMailDatas);//發送獎勵信件
                    await Logger.LeaderBoardReward(logData);//寫Log資料
                    console.log("結束發送RoundMaxWinGold_Weekly結算獎勵");
                }



                //排行榜有玩家進到前三名且名次大於上次的名次就送跑馬燈
                for (let i = 0; i < 3; i++) {
                    if (i < tmpDatas.length) {
                        let rankUp = false;//此玩家是否比上次名次還高
                        if (leaderBoardData == null || leaderBoardData["Datas"] == null) {//沒有上一次的排行榜 代表玩家名次一定高於上一次
                            rankUp = true;
                        } else {
                            if (i >= leaderBoardData["Datas"].length) {//上次排行榜沒有資料代表玩家名次一定高於上一次
                                rankUp = true;
                            } else {
                                let lastInRank = false;//上次是否也在前三名
                                let lastRank = 99999;//上次排名
                                for (let j = 0; j < 3; j++) {
                                    if (j >= leaderBoardData["Datas"].length)
                                        break;
                                    //如果上一次前三名也有此玩家 但排名比這次還後面
                                    //console.log("比較 " + tmpDatas[i]["PlayerUID"] + "  ,  " + leaderBoardData["Datas"][j]["PlayerUID"]);
                                    if (tmpDatas[i]["PlayerUID"] == leaderBoardData["Datas"][j]["PlayerUID"]) {
                                        lastInRank = true;
                                        lastRank = j;
                                        //console.log(tmpDatas[i]["PlayerUID"] + "這次是" + i + "名" + "上次是" + lastRank + "名");
                                        break;
                                    }
                                }
                                //console.log("lastInRank=" + lastInRank);
                                //console.log("lastRank=" + lastRank);
                                //console.log("curRank=" + i);
                                if (!lastInRank || i < lastRank)//上次不在前三或是上次排名比這次還差
                                    rankUp = true;
                            }
                        }
                        if (rankUp) {//名次比上次還高才送跑馬燈
                            let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, tmpDatas[i]["PlayerUID"]);
                            if (playerDocData != null) {
                                //新玩家送跑馬燈公告
                                let addData = {
                                    PlayerUID: tmpDatas[i]["PlayerUID"],
                                    MsgType: "RankUp_RoundMaxWinGold_Weekly",
                                    MsgParams: [playerDocData["Name"], (i + 1)],//名次為i+1
                                }
                                await FirestoreManager.AddDoc(GameSetting.GameDataCols.NewsTicker, addData);
                            }

                        }
                    }
                }


                let updateData = {
                    UID: "RoundMaxWinGold_Weekly",
                    Datas: tmpDatas,
                }
                await FirestoreManager.UpdateDoc(GameSetting.GameDataCols.LeaderBoard, "RoundMaxWinGold_Weekly", updateData);
            }
        }





        console.log("執行完畢- Crontab_UpdateLeaderboard");

    });


//每2分鐘檢查是否有後臺排程需要執行，若"是"則呼叫後臺API
exports.Crontab_CheckBackStageSchedule = functions.region('asia-east1').runWith({ timeoutSeconds: 540, memory: '8GB' }).pubsub.schedule('*/2 * * * *') //每兩分鐘觸發
    .timeZone('Asia/Taipei').onRun(async context => {

        console.log("開始執行- Crontab_CheckBackStageSchedule");
        let backendURL = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "BackendURL");
        let nowTime = admin.firestore.Timestamp.now();
        let snapshot = await FirestoreManager.GetDocs_WhereOperation(GameSetting.BackstageDataCols.Schedule, "RunTime", "<", nowTime);
        if (snapshot != null) {
            console.log("有工作需要執行");
            for (let doc of snapshot) {
                let data = doc.data();
                if (data.Type == "Time") {
                    // 呼叫目標API
                    console.log("執行工作", backendURL.BackstageAddress + data.API);
                    console.log(data.Params)
                    await callAPI('post', `${backendURL.BackstageAddress}${data.API}`, data.Params);
                    // 紀錄排程工作LOG
                    Logger.Schedule(data.Name, data.API, data.Params);
                    // // 移除已執行完工作
                    await FirestoreManager.DeleteDocByUID(GameSetting.BackstageDataCols.Schedule, doc.id);
                    console.log("移除已執行完工作");
                }
            }
        }
        console.log("執行完畢- Crontab_CheckBackStageSchedule");
    });


// 呼叫API
async function callAPI(method, url, params) {
    console.log(params);
    if (method.toLowerCase() == 'get') {
        return await axios.get(url, { params: params })
            .then((res) => {
                console.table(res.data);
                return res.data;
            })
            .catch((error) => {
                console.error(error);
                return error;
            })
    } else {
        return await axios.post(url, params)//這裡跟GET布一樣
            .then((res) => {
                console.table(res.data);
                return res.data;
            })
            .catch((error) => {
                console.error(error);
                return error;
            })
    }
}