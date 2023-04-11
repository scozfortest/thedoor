//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const Logger = require('./GameTools/Logger.js');


//完成任務&成就進度
exports.Quest_FinishProgress = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    //取資料 & 驗證資料
    if (!("TaskType1" in data) || !("TaskType2" in data) || !("TaskValue" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤"
        };
    }

    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家任務&成就紀錄
    let questJson = GameDataManager.GetJson(GameSetting.GameJsonNames.Quest);//取得任務json表

    let achievementCategoryStr = GameSetting.QuestCategoryType.Achievement + "s";
    let questCategoryStr = GameSetting.QuestCategoryType.DailyQuest + "s";

    //處理紀錄邏輯
    //紀錄規則 添加符合條件的任務完成次數 已完成者則不變動(不添加)
    let updateAchievements = {};//更新成就資料
    let updateQuests = {};//更新任務資料
    let existAchievementData = {};
    let existQuestData = {};
    //取出已存在紀錄
    if (playerHistoryDoc != null && achievementCategoryStr in playerHistoryDoc)
        existAchievementData = playerHistoryDoc[achievementCategoryStr];
    if (playerHistoryDoc != null && questCategoryStr in playerHistoryDoc)
        existQuestData = playerHistoryDoc[questCategoryStr];
    for (let _quest of questJson) {
        //console.log("-------------------------------------------");
        //console.log("check quest id: " + _quest["ID"] + " category: " + _quest["Category"]);
        //console.log("quest type1: " + _quest["TaskType1"] + " data type1: " + data["TaskType1"] + " equal: " + (_quest["TaskType1"] == data["TaskType1"]));
        //console.log("quest type2: " + _quest["TaskType2"] + " data type2: " + data["TaskType2"] + " equal: " + (_quest["TaskType2"] == data["TaskType2"]));
        if (_quest["TaskType1"] == data["TaskType1"] && (_quest["TaskType2"] == data["TaskType2"] ||
            typeof _quest["TaskType2"] === "undefined")) {
            let existProgressValue = 0;//已完成進度
            let finProgressValue = Number(_quest["TaskValue"]);//完成進度目標
            let recordProgressValue = 0;//紀錄進度
            if (_quest["Category"] == GameSetting.QuestCategoryType.Achievement) {
                if (_quest["ID"] in existAchievementData) {
                    existProgressValue = existAchievementData[_quest["ID"]]["ProgressValue"];
                }
                //console.log("已完成進度: " + existProgressValue + " 完成目標進度: " + finProgressValue);
                if (existProgressValue >= finProgressValue)//已完成進度>=完成進度目標->無須更新
                    continue;

                if (Number(data["TaskValue"]) + existProgressValue > finProgressValue)
                    recordProgressValue = finProgressValue;
                else
                    recordProgressValue = Number(data["TaskValue"]) + existProgressValue;

                updateAchievements[_quest["ID"]] = {
                    ProgressValue: recordProgressValue,
                };
                //console.log("添加目錄對象:成就 ID: " + _quest["ID"] + " 完成進度: " + updateAchievements[_quest["ID"]]["ProgressValue"]);
            } else {
                if (_quest["ID"] in existQuestData) {
                    existProgressValue = existQuestData[_quest["ID"]]["ProgressValue"];
                }
                //console.log("已完成進度: " + existProgressValue + " 完成目標進度: " + finProgressValue);
                if (existProgressValue >= finProgressValue)//已完成進度>=完成進度目標->無須更新
                    continue;

                if (Number(data["TaskValue"]) + existProgressValue > finProgressValue)
                    recordProgressValue = finProgressValue;
                else
                    recordProgressValue = Number(data["TaskValue"]) + existProgressValue;

                updateQuests[_quest["ID"]] = {
                    ProgressValue: recordProgressValue,
                };
                //console.log("添加目錄對象:任務 ID: " + _quest["ID"] + " 完成進度: " + updateQuests[_quest["ID"]]["ProgressValue"]);
            }
            //console.log("Task value: " + data["TaskValue"]);
        }
    }
    //console.log("-------------------------------------------");

    //寫入DB
    //成就部分
    if (Object.keys(updateAchievements).length > 0) {
        if (playerHistoryDoc != null && achievementCategoryStr in playerHistoryDoc) {//玩家成就紀錄存在
            let combineUpdateData = {};
            for (let achievementKey of Object.keys(updateAchievements)) {//加入此次更新的資料
                combineUpdateData[achievementKey] = updateAchievements[achievementKey];
            }
            for (let achievementKey of Object.keys(existAchievementData)) {//已存在但此次未更新的資料塞回去更新資料內 不然會被覆蓋
                if (!(achievementKey in updateAchievements))
                    combineUpdateData[achievementKey] = existAchievementData[achievementKey];
            }
            let updateData = {
                Achievements: combineUpdateData,
            }
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
            //console.log("History資料欄位都存在! 更新資料");
        } else {
            let updateData = {
                Achievements: updateAchievements,
            };
            if (playerHistoryDoc == null) {//玩家成就紀錄資料不存在就新增新的一筆
                await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
                //console.log("History資料不存在! 建立新資料");
            } else {//玩家成就紀錄不存在就新增欄位
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
                //console.log("History資料存在! 但欄位不存在 建立欄位");
            }
        }
    }

    //任務部分
    if (Object.keys(updateQuests).length > 0) {
        if (playerHistoryDoc != null && questCategoryStr in playerHistoryDoc) {//玩家任務紀錄存在
            let combineUpdateData = {};
            for (let questKey of Object.keys(updateQuests)) {//加入此次更新的資料
                combineUpdateData[questKey] = updateQuests[questKey];
            }
            for (let questKey of Object.keys(existQuestData)) {//已存在但此次未更新的資料塞回去更新資料內 不然會被覆蓋
                if (!(questKey in updateQuests))
                    combineUpdateData[questKey] = existQuestData[questKey];
            }
            let updateData = {
                DailyQuests: combineUpdateData,
            }
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
            //console.log("History資料欄位都存在! 更新資料");
        } else {
            let updateData = {
                DailyQuests: updateQuests,
            };
            if (playerHistoryDoc == null) {//玩家任務紀錄資料不存在就新增新的一筆
                await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
                //console.log("History資料不存在! 建立新資料");
            } else {//玩家任務紀錄不存在就新增欄位
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
                //console.log("History資料存在! 但欄位不存在 建立欄位");
            }
        }
    }

    //寫log
    if (Object.keys(updateAchievements).length > 0 || Object.keys(updateQuests).length > 0)
        Logger.Quest_FinishProgress(context.auth.uid, updateAchievements, updateQuests);

    //回傳結果
    return {
        Result: GameSetting.ResultTypes.Success,
    };

});


//領取任務&成就獎勵
exports.Quest_Claim = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    //取資料 & 驗證資料
    //現版本不會同時領取任務&成就 但因為共用同一個function 所以一起寫
    if (!("QuestIDs" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤"
        };
    }

    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家任務&成就紀錄
    let questJson = GameDataManager.GetJson(GameSetting.GameJsonNames.Quest);//取得任務json表

    if (playerHistoryDoc == null)
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "無完成資料"
        };

    let targetIDs = data["QuestIDs"];//領取獎勵ID
    let questDBs = playerHistoryDoc["DailyQuests"];//任務DB
    let achievementDBs = playerHistoryDoc["Achievements"];//成就DB
    let finishQuestIDs = [];//領取獎勵任務ID
    let finishAchievementIDs = [];//領取獎勵成就ID
    let rewardJsonDatas = [];//獎勵的json資料陣列
    for (let _questID of targetIDs) {
        //console.log("嘗試領取任務獎勵 ID: " + _questID + " type is: " + typeof _questID);
        let indexOfQuestJson = questJson.findIndex((e) => e.ID == _questID);
        //console.log("indexOfQuestJson is: " + indexOfQuestJson);
        if (indexOfQuestJson == -1) {
            //console.log("嘗試領取不存在表格資料的任務");
            continue;
        }
        if ((typeof questDBs != 'undefined') && questDBs[_questID]) {
            //console.log("目標為任務");
            if (questDBs[_questID].ProgressValue >= questJson[indexOfQuestJson].TaskValue && !(questDBs[_questID].ClaimTime)) {
                finishQuestIDs.push(_questID);
                rewardJsonDatas.push(questJson[indexOfQuestJson]);
                //console.log("領取任務獎勵. ID: " + _questID);
            }
        } else if ((typeof achievementDBs != 'undefined') && achievementDBs[_questID]) {
            //console.log("目標為成就");
            if (achievementDBs[_questID].ProgressValue >= questJson[indexOfQuestJson].TaskValue && !achievementDBs[_questID].ClaimTime) {
                finishAchievementIDs.push(_questID);
                rewardJsonDatas.push(questJson[indexOfQuestJson]);
                //console.log("領取成就獎勵. ID: " + _questID);
            }
        } else {
            console.log("嘗試領取不存在DB紀錄的任務");
        }
    }

    if (finishQuestIDs.length == 0 && finishAchievementIDs.length == 0) {
        //console.log("無可領取的成就與任務獎勵");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "無可領取的成就與任務獎勵"
        };
    }

    //處理紀錄邏輯
    //紀錄領取時間
    let nowTime = admin.firestore.Timestamp.now();
    for (let _questID of finishQuestIDs) {
        questDBs[_questID]["ClaimTime"] = nowTime;
    }
    for (let _questID of finishAchievementIDs) {
        achievementDBs[_questID]["ClaimTime"] = nowTime;
    }
    //發放獎勵
    let returnGainItems = [];//獲得獎勵清單
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    for (let _rewardJsonData of rewardJsonDatas) {
        if ("ItemType1" in _rewardJsonData && "ItemValue1" in _rewardJsonData) {
            let itemType = _rewardJsonData["ItemType1"];
            let itemValue = _rewardJsonData["ItemValue1"];

            let tmpReplaceGainItems = [];
            let returnGainItemsTemp = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, tmpReplaceGainItems);
            returnGainItems = returnGainItems.concat(returnGainItemsTemp);
            replaceGainItems = replaceGainItems.concat(tmpReplaceGainItems);
            //console.log("第一筆獎勵資料添加. ID: " + _rewardJsonData["ID"]);

        }
        if ("ItemType2" in _rewardJsonData && "ItemValue2" in _rewardJsonData) {
            let itemType = _rewardJsonData["ItemType2"];
            let itemValue = _rewardJsonData["ItemValue2"];

            let tmpReplaceGainItems = [];
            let returnGainItemsTemp = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, tmpReplaceGainItems);
            returnGainItems = returnGainItems.concat(returnGainItemsTemp);
            replaceGainItems = replaceGainItems.concat(tmpReplaceGainItems);
            //console.log("第一筆獎勵資料添加. ID: " + _rewardJsonData["ID"]);

        }
    }

    //寫入DB
    //成就部分
    if (finishAchievementIDs.length > 0) {
        let updateData = {
            Achievements: achievementDBs,
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
    }

    //任務部分
    if (finishQuestIDs.length > 0) {
        let updateData = {
            DailyQuests: questDBs,
        }
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
    }

    //寫log
    let logRewardData = {
        GainItems: returnGainItems,
    }
    if (finishQuestIDs.length > 0)
        Logger.Quest_Claim(context.auth.uid, finishQuestIDs, "DailyQuest", logRewardData);
    else if (finishAchievementIDs.length > 0)
        Logger.Quest_Claim(context.auth.uid, finishAchievementIDs, "Achievement", logRewardData);

    //回傳結果
    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});