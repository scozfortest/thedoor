//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const Logger = require('./GameTools/Logger.js');
const Probability = require('./Scoz/Probability.js');
const MyTime = require('./Scoz/MyTime.js');
const GCloudProject = process.env.GCLOUD_PROJECT//專案ID

//取得生涯獎勵(LVReward)
exports.Achievement_GetLVReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    //console.log("Achievement_GetLVReward had been call!");

    //取資料 & 驗證資料
    if (!("StartLV" in data) || !("EndLV" in data) || !("Choice" in data)) {
        //console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤"
        };
    }

    let playerHistoryDoc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家生涯紀錄
    let playerLVJson = GameDataManager.GetJson(GameSetting.GameJsonNames.PlayerLv);//取得生涯json表

    //先指派數字觸發隱性轉型再指派data 以免出現取data是NaN的情況
    let startLv = 1;//起始領取等級
    let endLv = 1;//結束領取等級
    let choice = 0;//選擇 目前只有0 or 1

    startLv = data["StartLV"];
    endLv = data["EndLV"];
    choice = data["Choice"];

    if (choice >= 2 || choice < 0)
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "選擇項目錯誤"
        }
    if (startLv > endLv)
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "領取等級錯誤"
        }

    if (playerHistoryDoc != null && "LVReward" in playerHistoryDoc) {//玩家生涯紀錄存在就比對資料驗證取得目標
        if (startLv <= Number(playerHistoryDoc["LVReward"]["LV"])) {
            //console.log("資料已建立 start lv: " + startLv + " record lv: " + Number(playerHistoryDoc["LVReward"]["LV"]));
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "起始等級資料錯誤"
            };
        }
    } else {
        if (startLv != 1) {
            //console.log("資料未建立 start lv: " + startLv);
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "起始等級資料錯誤"
            };
        }
    }

    //處理發放邏輯
    //取出要給予的獎勵資料
    let rewardJsonDatas = [];//生涯獎勵的json資料陣列
    let rewardDataCount = 0;
    for (let lvRewardJsonData of playerLVJson) {
        if (lvRewardJsonData["TravelLevel"] >= startLv) {
            if (lvRewardJsonData["TravelLevel"] <= endLv) {
                rewardDataCount = rewardJsonDatas.push(lvRewardJsonData);
            } else {
                break;//該取出的資料已經取完就跳出迭代
            }
        }
    }

    //TODO:之後有旅途點數資料這裡要去判斷是否達到領取目標點數

    //驗證獎勵資料數量
    if (rewardDataCount != endLv - startLv + 1)
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "獎勵數量資料錯誤"
        };

    //根據Choice發放獎勵    
    let choiceRewardPos = choice + 1;
    let returnGainItems = [];//獲得獎勵清單
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    let returnRewardCount = 0;
    for (let _rewardJsonData of rewardJsonDatas) {
        if (("ItemType" + choiceRewardPos) in _rewardJsonData && ("ItemValue" + choiceRewardPos in _rewardJsonData)) {
            let itemType = _rewardJsonData["ItemType" + choiceRewardPos];
            let itemValue = _rewardJsonData["ItemValue" + choiceRewardPos];

            let tmpReplaceGainItems = [];
            let tempReturnGainItems = await PlayerItemManager.GiveItem(itemType, itemValue, 1, context.auth.uid, tmpReplaceGainItems);
            returnGainItems = returnGainItems.concat(tempReturnGainItems);
            replaceGainItems = replaceGainItems.concat(tmpReplaceGainItems);
            //console.log("第一筆獎勵資料添加");
            returnRewardCount += 1;
        } else {
            //console.log("獎勵目標不存在 請檢查表格. 獎勵位置: " + choiceRewardPos);
            return {
                Result: GameSetting.ResultTypes.Fail,
                Data: "獎勵項目資料錯誤"
            };
        }
    }

    //console.log("returnRewardCount: " + returnRewardCount);

    //寫入DB
    //更新玩家生涯紀錄
    //已測試過情況三種1.完全沒資料新增 2.已經有玩家資料 但沒有LVReward 3.已經有完整資料更新寫入
    if (playerHistoryDoc != null && "LVReward" in playerHistoryDoc) {//玩家生涯紀錄存在
        let choices = playerHistoryDoc["LVReward"]["Choices"];
        for (let i = 0; i < returnRewardCount; i++)
            choices.push(choice);
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
            LVReward: {
                Choices: choices,
                LV: endLv,
            }
        });
        //console.log("History資料欄位都存在! 更新資料");
    } else {
        let createChoices = [];
        for (let i = 0; i < returnRewardCount; i++)
            createChoices.push(choice);
        //console.log("create choice is: " + createChoices);
        let updateData = {
            LVReward: {
                Choices: createChoices,
                LV: endLv,
            }
        }
        if (playerHistoryDoc == null) {//玩家紀錄資料不存在就新增新的一筆
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
            //console.log("History資料不存在! 建立新資料");
        } else {//玩家生涯紀錄不存在就新增欄位
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
            //console.log("History資料存在! 但欄位不存在 建立欄位");
        }
    }

    //寫log
    let logData = {
        GainItems: returnGainItems,
    }
    Logger.Achievement_GetLVReward(context.auth.uid, startLv, endLv, choice, logData);

    //回傳結果
    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});