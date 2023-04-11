//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const Logger = require('./GameTools/Logger.js');
const MyTime = require('./Scoz/MyTime.js');
const Probability = require('./Scoz/Probability.js');
//專案ID
const GCloudProject = process.env.GCLOUD_PROJECT


//領取當日簽到獎勵
exports.DailyReward_GetReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家簽到簿紀錄
    let dailyRewardJson = await GameDataManager.GetJson(GameSetting.GameJsonNames.DailyReward);//取得簽到簿json表
    let dailyRewardSettingDoc = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "DailyReward");//取得簽到簿後台設定
    let nowTime = admin.firestore.Timestamp.now();//目前玩家簽到時間




    let term = 0;//目前玩家是哪一期簽到簿
    let day = 1;//這次領取的獎勵是第幾天的獎勵
    if (playerHistoryDoc != null && "DailyReward" in playerHistoryDoc) {//玩家簽到簿紀錄存在就取玩家現在的期數
        let today = MyTime.AddHours(nowTime.toDate(), 8);//取得當日UTC+8時間
        let lastDay = MyTime.AddHours(playerHistoryDoc["DailyReward"]["LastTime"].toDate(), 8); //取得上次領取UTC+8時間
        //console.log("today=" + today);
        //console.log("lastDay=" + lastDay);
        if (MyTime.CheckIfDatesAreTheSameDay(today, lastDay)) {//如果跟Server上次領取的時間是同一天代表今日已經領過了
            //console.log("今日已領取過了");
            return { Result: GameSetting.ResultTypes.Success, };
        }

        term = playerHistoryDoc["DailyReward"]["Term"];
        day = playerHistoryDoc["DailyReward"]["Day"] + 1;
    } else {//玩家簽到簿紀錄不存在就取後台目前設定的期數且領取的是第一天獎勵
        term = dailyRewardSettingDoc["Term"];
        day = 1;
    }



    let thisRewardData = null;//本次要領取的簽到資料
    let termRewardDatas = [];//玩家這期的簽到簿的json資料陣列
    let tmpIndex = 1;
    for (let dailyRewardJsonData of dailyRewardJson) {
        if (dailyRewardJsonData["Group"] == term.toString()) {
            termRewardDatas.push(dailyRewardJsonData);
            if (tmpIndex == day)
                thisRewardData = dailyRewardJsonData;
            tmpIndex += 1;
        }
    }
    if (thisRewardData == null) {//要領取的簽到ID不在本期表格中
        console.log("資料錯誤");
        return { Result: "資料錯誤" };
    }
    let isLastDay = day >= (termRewardDatas.length - 1);//是否領取的是簽到簿最後一天的獎勵

    //給予獎勵
    let vip = Number(playerDocData["VIP"]);//取VIP等級
    let returnGainItems = [];//獲得獎勵清單
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    if (isNaN(vip))
        vip = 0;
    //獎勵1
    let itemNoStr = (1 + vip * 2);
    if (("ItemType" + itemNoStr) in thisRewardData) {
        let itemType = thisRewardData["ItemType" + itemNoStr];
        let itemValue = thisRewardData["ItemValue" + itemNoStr];
        returnGainItems = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, replaceGainItems);
    }
    let itemNoStr2 = (1 + vip * 2 + 1);
    //獎勵2
    if (("ItemType" + itemNoStr2) in thisRewardData) {
        let itemType = thisRewardData["ItemType" + itemNoStr2];
        let itemValue = thisRewardData["ItemValue" + itemNoStr2];
        let returnGainItems2 = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, replaceGainItems);
        returnGainItems = returnGainItems.concat(returnGainItems2);
    }

    //更新玩家簽到簿紀錄
    if (playerHistoryDoc != null && "DailyReward" in playerHistoryDoc) {//玩家簽到簿紀錄存在
        if (isLastDay == false) {//不是領到最後一天就將玩家現在的天數更新上玩家資料
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
                DailyReward: {
                    LastTime: nowTime,
                    Term: term,
                    Day: day,
                }
            });
        } else {//是最後一天就重回第0天並取目前後台設定的期數
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
                DailyReward: {
                    LastTime: nowTime,
                    Term: dailyRewardSettingDoc["Term"],
                    Day: 0,
                }
            });
        }
    } else {
        let updateData = {
            DailyReward: {
                LastTime: nowTime,
                Term: dailyRewardSettingDoc["Term"],
                Day: 1,
            }
        }
        if (playerHistoryDoc == null) {//玩家紀錄資料不存在就新增新的一筆
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        } else {//玩家簽到簿紀錄不存在就新增欄位
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }
    }

    //寫LOG
    let logData = {
        GainItems: returnGainItems,
    }
    Logger.DailyReward_GetReward(context.auth.uid, logData);

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    }
});

//領取擊娃娃獎勵
exports.HitTheDog_GetReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("HitCount" in data) || !("RewardType" in data)) {
        //data["HitCount"] , data["RewardType"]
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }
    let playerHitCount = Number(data["HitCount"]);
    let playerRewardType = Number(data["RewardType"]); //0 = Free , 1 = AD , 2 = Point



    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let playerHistoryDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家歷史紀錄
    let hitTheDogSettingDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "HitTheDog");//取得擊娃娃後台設定
    if (playerDocData == null) {
        console.log("玩家不存在");
        return { Result: "格式錯誤" };
    }

    let payPoint = Number(hitTheDogSettingDocData["PayPoint"]);
    let beforePT = playerDocData["Point"];
    if (playerRewardType == 2) {
        if (beforePT < payPoint) {//判斷point是否足夠
            console.log("點數不足");
            return { Result: "格式錯誤" };
        }
    }

    //計算獎勵
    let itemValue = 0;
    let settingHP = Number(hitTheDogSettingDocData["HP"]);
    let settingDmg = Number(hitTheDogSettingDocData["Dmg"]);
    let settingMaxGold = Number(hitTheDogSettingDocData["MaxGold"]);
    let tmpHitHP = (settingDmg * playerHitCount);
    if (tmpHitHP >= settingHP) {
        tmpHitHP = settingHP;
    }
    let getGold = (settingMaxGold * (tmpHitHP / settingHP));

    if (playerRewardType == 1) {//AD倍率
        let playlimitDown = Number(hitTheDogSettingDocData["Multiple_AD"][0]);
        let playlimitUp = Number(hitTheDogSettingDocData["Multiple_AD"][1]);
        let GerRand = Probability.GetRandFloatBetween(playlimitDown, playlimitUp, 1);//float
        itemValue = Math.floor((getGold * GerRand));
    } else if (playerRewardType == 2) {//Point倍率
        let playlimitDown = Number(hitTheDogSettingDocData["Multiple_Point"][0]);
        let playlimitUp = Number(hitTheDogSettingDocData["Multiple_Point"][1]);
        let GerRand = Probability.GetRandIntBetween(playlimitDown, playlimitUp);//int
        itemValue = Math.floor((getGold * GerRand));
    } else {//Free
        itemValue = Math.floor(getGold);
    }

    let playtimes = 0;//遊玩次數
    if (playerHistoryDocData != null && "HitTheDogPlayTimes" in playerHistoryDocData) {//玩家擊娃娃紀錄存在就取玩家現在的次數
        let playlimit = hitTheDogSettingDocData["DaliyLimitPlayTimes"];
        let vip = Number(playerDocData["VIP"]) + 1;//取VIP等級編號(等級+1就是表格編號)
        let vipJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.VIP, vip.toString());
        let extraMiniGameTimes = Number(vipJsonData["ExtraMiniGame"]);
        playlimit = playlimit + extraMiniGameTimes;
        playtimes = playerHistoryDocData["HitTheDogPlayTimes"];
        if (playtimes >= playlimit) {
            console.log("今日已到最大次數");
            return { Result: "格式錯誤" };
        }

        playtimes = playtimes + 1;
    } else {//玩家擊娃娃紀錄不存在就寫入第一次遊玩
        playtimes = 1;
    }

    //判斷及玩家扣PT
    if (playerRewardType == 2) {
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, {
            Point: admin.firestore.FieldValue.increment(-payPoint),
        });
    }

    //給予獎勵
    let itemType = "Gold";
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    let returnGainItems = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, replaceGainItems);
    //更新玩家擊娃娃紀錄
    if (playerHistoryDocData != null && "HitTheDogPlayTimes" in playerHistoryDocData) {//玩家擊娃娃紀錄存在
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            HitTheDogPlayTimes: playtimes,
        });
    } else {
        let updateData = {
            HitTheDogPlayTimes: playtimes,
        }
        if (playerHistoryDocData == null) {//玩家紀錄資料不存在就新增新的一筆
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        } else {//玩家擊娃娃紀錄不存在就新增欄位
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }
    }



    //寫LOG
    let logData = {};
    switch (playerRewardType) {
        case 1:
            logData["RewardType"] = "AD";
            break;
        case 2:
            logData["RewardType"] = "Point";
            logData["PayPoint"] = payPoint;
            break;
        default:
            logData["RewardType"] = "Free";
            break;
    }
    logData["GainItems"] = returnGainItems;
    Logger.DailyReward_HitTheDog(context.auth.uid, logData);

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});



//領取當日簽到獎勵
exports.DailyReward_GetWatchADReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }


    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄
    let alreadyWatchTimes = 0;//今日以觀看廣告次數
    if (playerHistoryDoc != null) {
        if ("WatchADTimes" in playerHistoryDoc) {
            alreadyWatchTimes = playerHistoryDoc["WatchADTimes"];
        }
    }
    let adRewardSettingData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "ADReward");//取得廣告後台設定

    if (alreadyWatchTimes >= adRewardSettingData["DailyLimitTimes"]) {
        console.log("今日已到最大次數");
        return { Result: "今日已到最大次數" };
    }

    //更新紀錄
    if (playerHistoryDoc != null && "WatchADTimes" in playerHistoryDoc) {//玩家已經觀看次數++
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            WatchADTimes: admin.firestore.FieldValue.increment(1),
        });
    } else {
        let updateData = {
            WatchADTimes: admin.firestore.FieldValue.increment(1),
        }
        if (playerHistoryDoc == null) {//玩家紀錄資料不存在就新增新的一筆
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        } else {//觀看廣告次數紀錄不存在就新增欄位
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }
    }

    //給予獎勵
    let stepRewards = adRewardSettingData["StepRewards"];
    //計算累積點擊廣告獎勵
    if (stepRewards.length > 0) {
        let itemType = "";
        let itemValue = 0;
        if (alreadyWatchTimes < stepRewards.length) {
            itemType = stepRewards[alreadyWatchTimes]["ItemType"];
            itemValue = stepRewards[alreadyWatchTimes]["ItemValue"];
        } else {
            itemType = stepRewards[stepRewards.length - 1]["ItemType"];
            itemValue = stepRewards[stepRewards.length - 1]["ItemValue"];
        }
        console.log("itemType=" + itemType);
        console.log("itemValue=" + itemValue);
        let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
        let returnGainItems = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, replaceGainItems);

        //寫LOG
        let logData = {};
        logData["GainItems"] = returnGainItems;
        Logger.DailyReward_GetWatchADReward(context.auth.uid, logData);
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: {
                ReturnGainItems: returnGainItems,
                ReplaceGainItems: replaceGainItems,
            }
        };

    } else {
        console.log("廣告獎勵為空");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: {
                ReturnGainItems: null,
                ReplaceGainItems: null,
            }
        };
    }





});

//巷戰古惑仔請求開始
exports.StreetFighter_Start = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let playerHistoryDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄
    let streetFighterSettingDoc = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "StreetFighter");//取得簽到簿後台設定

    //檢查後台設定資料是否存在
    if (streetFighterSettingDoc == null || !("DailyLimitPlayTimes" in streetFighterSettingDoc)) {
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "巷戰古惑仔資料錯誤"
        }
    }

    //檢查遊玩次數(前端會先判斷一次 避免被繞過再檢查一次)
    if (playerHistoryDocData != null && "StreetFighterPlayTimes" in playerHistoryDocData) {
        let playtimes = 0;//遊玩次數
        let playlimit = streetFighterSettingDoc["DailyLimitPlayTimes"];
        let vip = 1;//取VIP等級編號
        if (playerDocData != null && "VIP" in playerDocData) {
            vip = Number(playerDocData["VIP"]) + 1;//取VIP等級編號(等級+1就是表格編號)
        }
        let vipJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.VIP, vip.toString());
        let extraMiniGameTimes = 0;
        if (vipJsonData != null && "ExtraMiniGame" in vipJsonData) {
            extraMiniGameTimes = Number(vipJsonData["ExtraMiniGame"])
        }
        playlimit = playlimit + extraMiniGameTimes;
        playtimes = playerHistoryDocData["StreetFighterPlayTimes"];
        if (playtimes >= playlimit) {
            console.log("今日已到最大次數");
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "今日已到最大次數",
            };
        }
    }

    let maxWinRound = 0;
    if ("Rewards" in streetFighterSettingDoc) {
        maxWinRound = streetFighterSettingDoc["Rewards"].length;
        //console.log("最大勝利次數: " + maxWinRound);
    } else {
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "巷戰古惑仔獎勵資料錯誤"
        }
    }

    let streetFighterMaxWinRound = 0;//玩家本輪最大勝利次數
    let playerHitPoint = 0;
    //勝利次數計算
    for (let i = 1; i <= maxWinRound; i++) {
        playerHitPoint = Math.floor(Math.random() * maxWinRound) + 1;
        //測試用 拉高勝率場數
        //playerHitPoint = Math.floor(Math.random() * maxWinRound) + 10;
        //測試用 把所有項目測過
        //playerHitPoint = maxWinRound;
        //console.log("第" + i + "輪." + " 本輪玩家點數:" + playerHitPoint);
        if (playerHitPoint >= i) {
            //console.log("玩家勝利");
            streetFighterMaxWinRound = i;
        } else {
            //console.log("玩家輸了");
            break;
        }
    }
    //Dev版或Test版
    if (GCloudProject != GameSetting.GCloudProjects.Release) {
        var roundTest = Number(streetFighterSettingDoc["RoundTest"]);
        if (roundTest <= 20 && roundTest > 0) {
            streetFighterMaxWinRound = roundTest;
        }
    }

    //console.log("玩家最大勝利次數: " + streetFighterMaxWinRound);

    //寫入DB
    if (playerHistoryDocData != null && "StreetFighterMaxWinRound" in playerHistoryDocData) {//玩家紀錄存在
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            StreetFighterMaxWinRound: streetFighterMaxWinRound,
        });
    } else {
        let updateData = {
            StreetFighterMaxWinRound: streetFighterMaxWinRound,
        }
        if (playerHistoryDocData == null) {//玩家紀錄資料不存在就新增新的一筆
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        } else {//玩家紀錄不存在就新增欄位
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }
    }

    //更新遊玩次數(寫DB)
    if ("StreetFighterPlayTimes" in playerHistoryDocData) {
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            StreetFighterPlayTimes: admin.firestore.FieldValue.increment(1),
        });
    } else {
        let updateData = {
            StreetFighterPlayTimes: 1,
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
    }





    //回傳結果與保存
    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            StreetFighterMaxWinRound: streetFighterMaxWinRound,
        }
    };

});

exports.StreetFighter_GetReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    //檢查傳輸資料格式
    if (!("WinRound" in data) || !("IsWin" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤"
        }
    }

    let isWin = Number(data["IsWin"]);//0是輸 1是勝利
    let playerRound = Number(data["WinRound"]);//玩家場數(勝利則會小於等於最大勝場 輸則會是最大勝場+1)
    //console.log("玩家場數: " + playerRound);

    //勝敗資料內容檢查
    if (isWin < 0 || isWin > 1) {
        console.log("勝敗資料錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "勝利資料錯誤"
        }
    }

    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let playerHistoryDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家歷史紀錄
    let streetFighterSettingDoc = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "StreetFighter");//取得簽到簿後台設定

    //檢查資料是否存在(playerHistoryDocData一定要存在 不然無法取得最大可勝場數)
    if (playerDocData == null || playerHistoryDocData == null) {
        console.log("玩家不存在");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "玩家資料不存在"
        }
    }

    //檢查比對是否勝利次數有作弊
    let recordMaxWinRound = Number(playerHistoryDocData["StreetFighterMaxWinRound"]);
    //console.log("最大可勝利場數: " + recordMaxWinRound);
    //console.log("isWin=" + isWin);
    //console.log("playerRound=" + playerRound);
    //console.log("recordMaxWinRound=" + recordMaxWinRound);
    if ((isWin && playerRound > recordMaxWinRound) ||
        (!isWin && playerRound > recordMaxWinRound + 1)) {
        console.log("玩家勝利次數超過可勝利次數");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "勝利次數資料錯誤"
        }
    }

    //檢查是否有獎勵資料
    let maxWinRound = 0;
    if ("Rewards" in streetFighterSettingDoc) {
        maxWinRound = streetFighterSettingDoc["Rewards"].length;
        //console.log("最大勝利次數: " + maxWinRound);
    } else {
        console.log("巷戰古惑仔獎勵資料不存在");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "巷戰古惑仔獎勵資料錯誤"
        }
    }

    //計算獎勵與付費點數
    let costPoint = 0;//玩家總花費點數
    let getGold = 0;//獲得金版
    for (let i = 0; i < maxWinRound; i++) {
        let rewardData = streetFighterSettingDoc["Rewards"][i];
        if (!("PayType" in rewardData) || !("PayValue" in rewardData) || !("Lose" in rewardData) || !("Win" in rewardData)) {
            //console.log("取得第" + i + "筆獎勵資料錯誤");
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "巷戰古惑仔獎勵資料錯誤"
            }
        }
        let payType = rewardData["PayType"];
        let payValue = Number(rewardData["PayValue"]);
        //console.log("第" + i + "筆資料. 付費TYPE: " + payType + " 數值: " + payValue);
        if (i == playerRound - 1) {
            if (isWin == 0) {//輸只有一種可能 就是最大勝場的下一場(即是玩家總勝場的下一場)
                getGold = Number(rewardData["Lose"]);
                //console.log("玩家輸 取得失敗獎勵第" + i + "筆資料為: " + Number(rewardData["Lose"]));
                break;
            } else if (isWin == 1) {//勝利有兩種可能 1.達到最大全勝 2.提早放棄領取 但不管是哪種都會是取玩家勝場資料
                getGold = Number(rewardData["Win"]);
                //console.log("玩家勝利 取得勝利獎勵第" + i + "筆資料為: " + Number(rewardData["Win"]));
                break;
            }
        } else {
            if (payType === "Point") {
                costPoint = costPoint + payValue;
            }
        }
    }
    //console.log("獲得金塊: " + getGold + " 花費金額: " + costPoint);

    //測試付費部分 如果出問題在這裡做最後測試阻擋 以下會開始寫入DB
    /*
    return {
        Result: GameSetting.ResultTypes.Fail,
        Data: "測試付費部分(汗) 我邏輯不好"
    }
    */

    //檢查付費資源是否足夠
    let playerPT = playerDocData["Point"];
    if (playerPT < costPoint) {
        //console.log("需花費: " + costPoint + " 玩家持有: " + playerPT + " 餘額不足");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "資源不足"
        }
    }

    //計算遊玩次數(雖然前面檢查過 但這裡保險起見再檢查一次 以免有人發送假封包繞過前面驗證)
    let playTimes = 0;
    if ("StreetFighterPlayTimes" in playerHistoryDocData) {//有紀錄取出之前打的次數再+1
        let playlimit = streetFighterSettingDoc["DailyLimitPlayTimes"];
        let vip = 1;
        if ("VIP" in playerDocData) {
            vip = Number(playerDocData["VIP"]) + 1;//取VIP等級編號(等級+1就是表格編號)
        }
        let vipJsonData = GameDataManager.GetData(GameSetting.GameJsonNames.VIP, vip.toString());
        let extraMiniGameTimes = 0;
        if (vipJsonData != null && "ExtraMiniGame" in vipJsonData) {
            extraMiniGameTimes = Number(vipJsonData["ExtraMiniGame"])
        }
        playlimit = playlimit + extraMiniGameTimes;
        playTimes = playerHistoryDocData["StreetFighterPlayTimes"];
        if (playTimes > playlimit) {
            console.log("今日已到最大次數");
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "今日已到最大次數",
            };
        }
        playTimes = playTimes + 1;
    } else {//沒紀錄表示第一次打
        playTimes = 1;
    }
    //console.log("遊玩次數: " + playTimes);

    //付費(寫DB)
    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, {
        Point: admin.firestore.FieldValue.increment(-costPoint),
    });

    //給予獎勵(寫DB)
    let itemType = "Gold";
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    let returnGainItems = await PlayerItemManager.GiveItem(itemType, getGold, 1, context.auth.uid, replaceGainItems);
    /*
        //更新遊玩次數(寫DB)
        if ("StreetFighterPlayTimes" in playerHistoryDocData) {
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
                StreetFighterPlayTimes: playTimes,
            });
        } else {
            let updateData = {
                StreetFighterPlayTimes: playTimes,
            }
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }
    */

    //寫log
    let logData = {};
    logData["GainItems"] = returnGainItems;
    Logger.DailyReward_StreetFighter(context.auth.uid, playerRound, costPoint, logData);

    //回傳
    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    }

});