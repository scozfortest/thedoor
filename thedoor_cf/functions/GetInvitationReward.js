//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
const db = admin.firestore();
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const Logger = require('./GameTools/Logger.js');

//玩家B輸入玩家A的邀請碼後 雙方會獲得獎勵
exports.GetInvitationReward = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }


    if (!("Code" in data)) {
        console.log("格式錯誤");
        return {
            Result: GameSetting.ResultTypes.Fail,
            Data: "格式錯誤",
        };
    }

    let code = data["Code"];



    let giftCodeDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.GiftCode, code);
    if (giftCodeDocData == null) {
        console.log("無此邀請碼");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "無此邀請碼",
        };
    }

    if ("Invitees" in giftCodeDocData && giftCodeDocData["Invitees"].includes(context.auth.uid)) {
        console.log("已領取過此玩家的邀請碼了");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "已領取過此玩家的邀請碼了",
        };
    }



    //取GiftCodeSettingDoc
    let giftCodeSettingData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.GiftCodeSetting, giftCodeDocData["GiftCodeSettingUID"]);
    if (giftCodeSettingData == null) {
        console.log("無此GiftCodeSetting");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "GiftCodeSetting",
        };
    }
    let playerHistoryDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);//取得玩家紀錄
    if (playerHistoryDocData != null && "GiftCodeUse" in playerHistoryDocData) {
        let giftCodeUse = playerHistoryDocData["GiftCodeUse"]
        if (giftCodeDocData["GiftCodeSettingUID"] in giftCodeUse && giftCodeUse[giftCodeDocData["GiftCodeSettingUID"]] >= giftCodeSettingData["MaxInputTimes"]) {

            console.log("此邀請碼輸入次數已達上限");
            return {
                Result: GameSetting.ResultTypes.Success,
                Data: "此邀請碼輸入次數已達上限",
            };
        }
    }

    if (giftCodeDocData["Type"] != "Invitation") {
        console.log("必須輸入玩家的邀請碼");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "必須輸入玩家的邀請碼",
        };
    }
    if (giftCodeDocData["OwnerUID"] == context.auth.uid) {
        console.log("無法領取自己的邀請碼");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "無法領取自己的邀請碼",
        };
    }
    let nowTimestamp = admin.firestore.Timestamp.now();
    if ("StartTime" in giftCodeSettingData && nowTimestamp < giftCodeSettingData["StartTime"]) {
        console.log("邀請碼尚不可領取");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "邀請碼尚不可領取",
        };
    }
    if ("EndTime" in giftCodeSettingData && nowTimestamp > giftCodeSettingData["EndTime"]) {
        console.log("邀請碼過期");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "邀請碼過期",
        };
    }
    let curClaimTimes = giftCodeDocData["CurClaimTimes"];
    if (giftCodeDocData["MaxClaimTimes"] != 0 && curClaimTimes >= giftCodeDocData["MaxClaimTimes"]) {
        console.log("領取數量已達上限");
        return {
            Result: GameSetting.ResultTypes.Success,
            Data: "領取數量已達上限",
        };
    }
    curClaimTimes++;

    let itemType = "";
    let itemValue = 0;
    if ("ItemType" in giftCodeDocData && "ItemValue" in giftCodeDocData) {
        itemType = giftCodeDocData["ItemType"];
        itemValue = giftCodeDocData["ItemValue"];
    } else if ("ItemType" in giftCodeSettingData && "ItemValue" in giftCodeSettingData) {
        itemType = giftCodeSettingData["ItemType"];
        itemValue = giftCodeSettingData["ItemValue"];
    }
    console.log("itemType=" + itemType);
    console.log("itemValue=" + itemValue);
    //輸入序號碼的玩家收到獎勵信件
    if (itemType != "") {
        //設定信件資料
        let mailData = {
            OwnerUID: context.auth.uid,
            Items: [{
                ItemType: itemType,
                ItemValue: itemValue,
            }],
            Title: giftCodeSettingData["InviteeMailTitle"],
        }
        await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, mailData);
        //領取紀錄+1且紀錄領取人
        await FirestoreManager.UpdateDoc(GameSetting.GameDataCols.GiftCode, code, {
            CurClaimTimes: admin.firestore.FieldValue.increment(1),
            Invitees: admin.firestore.FieldValue.arrayUnion(context.auth.uid),
        });

        //玩家歷史紀錄 寫領取紀錄
        if (playerHistoryDocData != null) {
            if ("GiftCodeUse" in playerHistoryDocData && playerHistoryDocData["GiftCodeUse"] != null) {

                let giftCodeUse = playerHistoryDocData["GiftCodeUse"];
                giftCodeUse[giftCodeDocData["GiftCodeSettingUID"]] += 1;
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, {
                    GiftCodeUse: giftCodeUse,
                });
            } else {

                let updateData = {
                    GiftCodeUse: {
                        [giftCodeDocData["GiftCodeSettingUID"]]: 1,
                    }
                }
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
            }
        } else {

            let updateData = {
                GiftCodeUse: {
                    [giftCodeDocData["GiftCodeSettingUID"]]: 1,
                }
            }
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateData);
        }

        Logger.GetInvitationReward(context.auth.uid, giftCodeDocData["Name"], code, itemType, itemValue);//寫Log

    }


    //被輸入方領取獎勵

    let rewards = giftCodeSettingData["Rewards"];
    if (rewards.length > 0) {
        let gainRewards = [];
        //console.log("curClaimTimes=" + curClaimTimes);
        for (let i = 0; i < rewards.length; i++) {
            //console.log("RequireStartValue=" + rewards[i]["RequireStartValue"]);
            //console.log("RequireEndValue=" + rewards[i]["RequireEndValue"]);
            if (curClaimTimes >= rewards[i]["RequireStartValue"] && curClaimTimes <= rewards[i]["RequireEndValue"]) {
                gainRewards.push({
                    ItemType: rewards[i]["ItemType"],
                    ItemValue: rewards[i]["ItemValue"],
                });
            }
        }
        //console.log("gainRewards.length=" + gainRewards.length);
        if (gainRewards.length > 0) {
            //設定信件資料
            let mailData = {
                OwnerUID: giftCodeDocData["OwnerUID"],
                Items: gainRewards,
                Title: giftCodeSettingData["InviteeMailTitle"],
            }
            await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, mailData);//被輸入序號碼的玩家收到獎勵信件
            Logger.GetReceiverReward(context.auth.uid, giftCodeDocData["OwnerUID"], giftCodeDocData["Name"], code, gainRewards);//寫Log
        }

    }


    //回傳結果
    return {
        Result: GameSetting.ResultTypes.Success,
        Data: "領取成功，獎勵已發至信箱",
    };

});

