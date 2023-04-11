//基本設定
const admin = require('firebase-admin');

//自訂方法
const GameSetting = require('./GameSetting.js');
const FirestoreManager = require('../FirebaseTools/FirestoreManager.js');
const GameDataManager = require('./GameDataManager');


module.exports = {
    //寫Cound Function Log
    CFLog: function (log, logType) {
        let data = {
            Log: log,
            LogType: logType,
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.CFLog, data);
    },
    //帳號註冊
    Signup: function (playerUID) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["SignupType"] = GameSetting.SignupType.Game;
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Signup, data);
    },
    //登入
    Signin: function (playerUID) {
        let data = GetDefaultPlayerLogData(playerUID);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Signin, data);
    },
    //登出
    Signout: function (playerUID) {
        let data = GetDefaultPlayerLogData(playerUID);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Signout, data);
    },
    //玩家更名
    PlayerChangeName: function (playerUID, nameBefore, nameAfter) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["NameBefore"] = nameBefore;
        data["NameAfter"] = nameAfter;
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.PlayerChangeName, data);
    },
    //玩家更改自介
    PlayerChangeIntro: function (playerUID, introBefore, introAfter) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["IntroBefore"] = introBefore;
        data["IntroAfter"] = introAfter;
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.PlayerChangeIntro, data);
    },
    //情境對話獎勵
    RoleCallReward: function (playerUID, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.RoleCallReward, data);
    },
    //腳色合成
    Role_Combine: function (playerUID, roleIDs) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["RoleIDs"] = roleIDs;
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.CombineRole, data);
    },
    //信件領取
    Mail_Claim: function (playerUID, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
    },
    //信件移除
    Mail_Remove: function (playerUID, mailData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //合併資料
        if (mailData != null)
            data = Object.assign({}, data, mailData);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.RemoveMail, data);
    },
    //商城購買開始
    ShopBuy_Start: async function (docName, playerUID, shopUID, buyCount, name, itemType, itemValue, beforeCurrencies) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["Progress"] = ["開始"];
        data["ShopUID"] = shopUID;
        data["BuyCount"] = buyCount;
        data["Name"] = name;
        data["ItemType"] = itemType;
        data["ItemValue"] = itemValue;
        data["ItemName"] = GetItemName(itemType, itemValue);
        data["BeforeCurrencies"] = beforeCurrencies;

        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.GameLogCols.ShopBuy, docName, data);
    },
    //商城購買追加進度
    ShopBuy_AddProgress: function (docName, addProgressStr, logData) {
        let data = {};
        data["Progress"] = admin.firestore.FieldValue.arrayUnion(addProgressStr);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
            }
            //合併資料
            data = Object.assign({}, data, logData);
        }
        //更新資料庫
        FirestoreManager.UpdateDoc(GameSetting.GameLogCols.ShopBuy, docName, data);
    },
    //儲值完後，驗證給商品處理LOG-開始
    Purchase_Start: async function (docName, playerUID, store, purchaseUID) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["Progress"] = ["開始"];
        data["PurchaseUID"] = purchaseUID;
        data["Store"] = store;
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.GameLogCols.Purchase, docName, data);
    },
    //儲值完後，驗證給商品處理LOG-追加進度
    Purchase_AddProgress: function (docName, addProgressStr, logData) {
        let data = {};
        data["Progress"] = admin.firestore.FieldValue.arrayUnion(addProgressStr);
        if (logData != null) {
            //設定獲得道具的名稱
            if ("ItemType" in logData && "ItemValue" in logData) {
                logData["ItemName"] = GetItemName(logData["ItemType"], logData["ItemValue"]);
            }
            //設定獲得道具的名稱
            if ("GainItems" in logData) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
            }
            //合併資料
            data = Object.assign({}, data, logData);
        }
        //更新資料庫
        FirestoreManager.UpdateDoc(GameSetting.GameLogCols.Purchase, docName, data);
    },
    //每日簽到獎勵
    DailyReward_GetReward: function (playerUID, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.DailyReward, data);
    },
    //擊娃娃
    DailyReward_HitTheDog: function (playerUID, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.HitTheDog, data);
    },
    //巷戰古惑仔獎勵
    DailyReward_StreetFighter: function (playerUID, winRound, payPoint, gainItems) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["WinRound"] = winRound;
        data["PayPoint"] = payPoint;
        //設定獲得道具的名稱
        if (gainItems != null) {
            if ("GainItems" in gainItems && gainItems["GainItems"] != null && gainItems["GainItems"] != undefined) {
                for (let gainItem of gainItems["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, gainItems);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.StreetFighter, data);
    },
    //領取廣告獎勵
    DailyReward_GetWatchADReward: function (playerUID, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.GetWatchADReward, data);
    },
    //好友贈送Point
    Trade_SendPT: function (playerUID, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.SendPoint, data);
    },
    //獲得生涯獎勵
    Achievement_GetLVReward: function (playerUID, startLv, endLv, choice, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["StartLv"] = admin.firestore.FieldValue.arrayUnion(startLv);
        data["EndLv"] = admin.firestore.FieldValue.arrayUnion(endLv);
        data["Choice"] = admin.firestore.FieldValue.arrayUnion(choice);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.GetLVReward, data);
    },
    //完成任務&成就進度
    Quest_FinishProgress: function (playerUID, updateAchievements, updateDailyQuests) {
        let data = GetDefaultPlayerLogData(playerUID);
        if (Object.keys(updateAchievements).length > 0)
            data["Achievements"] = updateAchievements;
        if (Object.keys(updateDailyQuests).length > 0)
            data["DailyQuests"] = updateDailyQuests;
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Quest_FinishProgress, data);
    },
    //領取任務&成就獎勵
    Quest_Claim: function (playerUID, questIDs, questType, rewardItemDatas) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["QuestID"] = questIDs;
        data["QuestType"] = questType;
        //設定獲得道具的名稱
        if (rewardItemDatas != null) {
            if ("GainItems" in rewardItemDatas && rewardItemDatas["GainItems"] != null && rewardItemDatas["GainItems"] != undefined) {
                for (let gainItem of rewardItemDatas["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, rewardItemDatas);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Quest_Claim, data);
    },
    //執行排程工作
    Schedule: function (Name, API, Params) {
        let data = {
            Name: Name,
            API: API,
            Params: Params
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.Schedule, data);
    },
    //刮刮卡獎勵
    ScratchCard_GetReward: function (playerUID, itemType, itemValue, logData) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["ItemType"] = itemType;
        data["ItemValue"] = itemValue;
        data["ItemName"] = GetItemName(itemType, itemValue);
        //設定獲得道具的名稱
        if (logData != null) {
            if ("GainItems" in logData && logData["GainItems"] != null && logData["GainItems"] != undefined) {
                for (let gainItem of logData["GainItems"]) {
                    gainItem["ItemName"] = GetItemName(gainItem["ItemType"], gainItem["ItemValue"]);
                }
                //合併資料
                data = Object.assign({}, data, logData);
            }
        }
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.ScratchCardReward, data);
    },
    //排行榜發送獎勵信件
    LeaderBoardReward: async function (logData) {
        await FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.LeaderBoardReward, logData);
    },
    // VIP經驗
    VIP_AddEXP: function (logData) {
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.VIP_AddEXP, logData);
    },
    //取得道具名稱
    GetItemName: function (itemType, itemValue) {
        return GetItemName(itemType, itemValue);
    },
    //輸入序號的人收到禮物信件的Log
    GetInvitationReward: function (playerUID, name, giftCodeUID, itemType, itemValue) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["ItemType"] = itemType;
        data["ItemValue"] = itemValue;
        data["ItemName"] = GetItemName(itemType, itemValue);
        data["Name"] = name;
        data["GiftCodeUID"] = giftCodeUID;
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.GiftCode, data);
    },
    //輸入序號的人收到禮物信件的Log
    GetReceiverReward: function (receiverUID, playerUID, name, giftCodeUID, gainRewards) {
        let data = GetDefaultPlayerLogData(playerUID);
        data["Name"] = name;
        data["GiftCodeUID"] = giftCodeUID;
        data["ReceiverUID"] = receiverUID;
        data["GainItems"] = [];
        if (gainRewards.length > 0) {
            for (let i = 0; i < gainRewards.length; i++) {
                let itemName = GetItemName(gainRewards[i]["ItemType"], gainRewards[i]["ItemValue"]);
                gainRewards[i]["ItemName"] = itemName;
            }
            data["GainItems"] = gainRewards;
        }
        console.log("Add Doc: " + GameSetting.GameLogCols.InvitationCode);
        FirestoreManager.AddDoc_DontWait(GameSetting.GameLogCols.InvitationCode, data);
    },
}

function GetDefaultPlayerLogData(playerUID) {
    let data = {
        PlayerUID: playerUID,
    }
    return data;
}
//取得道具中文名稱
function GetItemName(itemType, itemValue) {
    //檢查道具類型
    if (!(itemType in GameSetting.ItemTypes)) {
        return "錯誤的道具類型: " + itemType;
    }
    //資源類道具
    if (itemType in GameSetting.CurrencyTypes) {
        return GameDataManager.GetStr("UI_Item_" + itemType, "TW_TW") + "x" + itemValue;
    }
    let strID = itemType + "_" + itemValue;
    return GameDataManager.GetStr(strID, "Name_TW");
}
