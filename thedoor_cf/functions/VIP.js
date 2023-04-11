//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');

//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const Logger = require('./GameTools/Logger.js');
const MyTime = require('./Scoz/MyTime.js');

exports.VIPLevelDiff = function VIPLevelDiff(vipType, oldValue, valueChange) {
    let oldLv = GetVIPLevel(vipType, oldValue)
    let newValue = oldValue + valueChange
    let newLv = GetVIPLevel(vipType, newValue)
    return {
        oldLv: oldLv,
        newLv: newLv,
        lvDiff: newLv - oldLv,
    }
}

function GetVIPLevel(vipType, value) {
    let VIPJson = GameDataManager.GetJson(GameSetting.GameJsonNames.VIP);//取得VIP表
    let lv = -1
    for (let vipData of VIPJson) {
        if (vipData["VIPType"] != vipType) {
            continue
        }
        let dataLv = Number(vipData["VIPLevel"])
        if ((lv < dataLv) && (value >= vipData["RequireValue"])) {
            lv = dataLv
        }
    }
    return lv
}

exports.GetVIPType = function GetVIPType(lv) {
    let VIPJson = GameDataManager.GetJson(GameSetting.GameJsonNames.VIP);//取得VIP表
    for (let vipData of VIPJson) {
        let dataLv = Number(vipData["VIPLevel"])
        if (lv == dataLv) {
            return vipData["VIPType"]
        }
    }
    return "none"
}

exports.GetVIPData = function GetVIPData(lv) {
    let VIPJson = GameDataManager.GetJson(GameSetting.GameJsonNames.VIP);//取得VIP表
    for (let vipData of VIPJson) {
        let dataLv = Number(vipData["VIPLevel"])
        if (lv == dataLv) {
            return vipData
        }
    }
    return null
}

function IsLevelStay(lv, value) {
    let VIPJson = GameDataManager.GetJson(GameSetting.GameJsonNames.VIP);//取得VIP表
    for (let vipData of VIPJson) {
        let dataLv = Number(vipData["VIPLevel"])
        if ((lv == dataLv) && (value >= vipData["StayLVRequireValue"])) {
            return true
        }
    }
    return false
}

function CheckBonus(bonusData, checkDateUTC0) {
    const createTime = bonusData["CreateTime"]
    let createDateUTC0 = createTime.toDate()
    let createDateUTC8 = MyTime.AddHours(createDateUTC0, 8)
    let checkDateUTC8 = MyTime.AddHours(checkDateUTC0, 8)
    let thisIndex = DiffMonth(checkDateUTC8, createDateUTC8) - 1
    let thisOK = Boolean(bonusData["PerBonus"][thisIndex])
    let nextOK = Boolean(bonusData["PerBonus"][thisIndex + 1])
    console.log("[normal] BonusCreateTime: UTC0 ", createDateUTC0, " ,UTC8 ", createDateUTC8)
    console.log("[normal] BonusCheckTime: UTC0", checkDateUTC0, " ,UTC8 ", checkDateUTC8)
    return {
        thisIndex: thisIndex,
        okThisMonth: thisOK,
        okNextMonth: nextOK,
    }
}

function DiffMonth(date1, date2) {
    let date1m = date1.getFullYear() * 12 + date1.getMonth()
    let date2m = date2.getFullYear() * 12 + date2.getMonth()
    return Math.abs(date1m - date2m)
}

function GetBasicVIPGiftMailContent(lv) {
    let mailContent = {}
    let VIPJson = GameDataManager.GetJson(GameSetting.GameJsonNames.VIP);//取得VIP表
    for (let vipData of VIPJson) {
        let dataLv = Number(vipData["VIPLevel"])
        if (lv == dataLv) {
            let mailTitle = "月初VIP專屬禮"
            mailContent["Title"] = mailTitle
            let reward = {
                ItemType: vipData["ResetItemType"],
                ItemValue: Number(vipData["ResetItemValue"]),
            }
            mailContent["Items"] = [reward]
            if (reward.ItemValue == 0) {
                return null
            }
            return mailContent
        }
    }
    return null
}


// Update支援dot notation; Add不支援, 所以要轉格式
// UpdateData { l1.l2.l3.l4: Value } => AddData[l1][l2][l3][l4] = Value
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

exports.VIP_MonthlyResetVIPEXP = functions.region('asia-east1').https.onRequest(async (req, res) => {
    let jsonData = req.body;
    console.log("jsonData: " + JSON.stringify(jsonData));

    // 撈資料&檢查
    const checkJsonDataKey = ["Players"]
    checkJsonDataKey.forEach(key => {
        if (!(key in jsonData)) {
            let errMessage = "MonthlyResetVIPEXP loss" + key + ": " + JSON.stringify(jsonData)
            console.log(errMessage);
            res.json({ "Result": "ERR" + errMessage });
            return;
        }
    })
    let checkDate = admin.firestore.Timestamp.now().toDate()

    // 真實玩家資料
    const playerSnapshots = await FirestoreManager.GetDocs_WhereIn(GameSetting.PlayerDataCols.Player, "UID", jsonData.Players)
    //console.log("player: "+ JSON.stringify(playerSnapshot));
    if (playerSnapshots.empty) {
        console.log("找不到任何玩家");
        res.json({ "Result": "找不到任何玩家" });
        return;
    }

    let lossPlayer = []
    let lvStayPlayer = []
    let lvDownPlayer = []
    let lvZeroPlayer = []
    for (let playerSnapShot of playerSnapshots) {
        const player = playerSnapShot.data()
        const uid = player["UID"]
        let history = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, uid);//取得玩家紀錄

        let oriVIP = 0
        if ("VIP" in player) {
            oriVIP = player["VIP"]
        }

        let updatePlayer = {}
        let updateHistory = {}

        // VIP等級
        const vipType = exports.GetVIPType(oriVIP)
        let myValue = 0
        switch (vipType) {
            case GameSetting.VIPType.Purchase:
                if ("VIPEXP" in player) {
                    myValue = player["VIPEXP"]
                }
                break
            case GameSetting.VIPType.Active:
                if ("ActiveVIPEXP" in player) {
                    myValue = player["ActiveVIPEXP"]
                }
                break
            default:
                console.log("VIPType out of expect?", JSON.stringify(player), " oriVIP: ", oriVIP)
                continue
        }
        let lvStay = IsLevelStay(oriVIP, myValue)
        let afterVIP = 0
        if (!lvStay) {
            updatePlayer["VIP"] = admin.firestore.FieldValue.increment(-1)
            lvDownPlayer.push(uid)
            afterVIP = oriVIP - 1
        } else {
            if (oriVIP == 0) {
                lvZeroPlayer.push(uid)
            } else {
                lvStayPlayer.push(uid)
            }
            afterVIP = oriVIP
        }

        // VIPEXP
        // 歸0
        let finalVIPEXP = 0
        // 每月Bonus
        let bonusWord = ""
        if ((history) && ("MonthlyVIPEXPBonus" in history)) {
            let allBonus = history["MonthlyVIPEXPBonus"]
            console.log("allBonus: ", JSON.stringify(allBonus))
            if (allBonus) {
                bonusWord = ", "
                for (let bonusType in allBonus) {
                    let bonusData = allBonus[bonusType]
                    console.log("bonusData: ", JSON.stringify(bonusData))
                    if ((!bonusData["Enable"]) || (bonusData["Enable"] == false)) {
                        continue
                    }
                    // 執行
                    let checkData = CheckBonus(bonusData, checkDate)
                    console.log("checkData: ", JSON.stringify(checkData))

                    let give = false
                    if (checkData["okThisMonth"]) {
                        if (checkData["thisIndex"] != bonusData["Counter"]) {
                            lossPlayer.push(uid)
                            console.log("Not match between BonusThisIndex and Bonus[Counter], checkData: ", JSON.stringify(checkData), ", bonusData: ", JSON.stringify(bonusData))
                        }
                        give = true
                        const bIndex = checkData["thisIndex"]
                        if (bonusData["PerBonus"][bIndex]) {
                            finalVIPEXP = finalVIPEXP + Number(bonusData["PerBonus"][bIndex])
                        }
                    }
                    // 執行後更新內容
                    if (give) {
                        bonusWord = bonusWord + bonusType
                        updateHistory["MonthlyVIPEXPBonus." + bonusType + ".Counter"] = admin.firestore.FieldValue.increment(1)
                    }
                    updateHistory["MonthlyVIPEXPBonus." + bonusType + ".Enable"] = checkData["okNextMonth"]
                }
            }
        }
        updatePlayer["VIPEXP"] = finalVIPEXP
        updatePlayer["ActiveVIPEXP"] = 0

        console.log("uid: ", uid, ", oriVIP:", oriVIP, ", finalVIPEXP: ", finalVIPEXP, ", VIPType: ", vipType, ", myValue: ", myValue, ", lvStay:", lvStay)

        let vipChange = {
            PlayerUID: uid,
            VIPType: "MonthlyReset, " + vipType + bonusWord,
            BeforeVIP: oriVIP,
            AfterVIP: afterVIP,
            BeforeEXP: myValue,
            AddEXP: -myValue + finalVIPEXP,
            NewEXP: finalVIPEXP,
        };
        Logger.VIP_AddEXP(vipChange)

        // 發獎
        let mailContent = GetBasicVIPGiftMailContent(oriVIP)
        console.log("basicGift: ", JSON.stringify(mailContent))
        if (mailContent != null) {
            mailContent["OwnerUID"] = uid
        }

        // updateFirestore
        // player
        if (Object.keys(updatePlayer) != 0) {
            console.log("uPlayer: " + JSON.stringify(updatePlayer))
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, uid, updatePlayer);
        }

        // history
        console.log("uHistory: " + JSON.stringify(updateHistory))
        if (Object.keys(updateHistory).length != 0) {
            if (history != null) {
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, uid, updateHistory);
            } else {
                let addData = FormatToAddData(updateHistory)
                console.log("uHistory => aHistory: " + JSON.stringify(addData))
                await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, uid, addData);
            }
        }

        // mail
        if (mailContent) {
            let mailResult = await FirestoreManager.AddDoc(GameSetting.PlayerDataCols.Mail, mailContent);
            console.log("aMail: " + JSON.stringify(mailContent) + "=> Result: " + mailResult)
        }
    }

    let logDateUTC8 = MyTime.AddHours(checkDate, 8)
    let docName = [logDateUTC8.getFullYear(), logDateUTC8.getMonth() > 9 ? '' : '0', logDateUTC8.getMonth() + 1].join('')
    // VIP升級降級LOG
    let updateData = {
        LvStay: admin.firestore.FieldValue.increment(lvStayPlayer.length),
        LvDown: admin.firestore.FieldValue.increment(lvDownPlayer.length),
        LvZero: admin.firestore.FieldValue.increment(lvZeroPlayer.length)
    }
    let playerDocData = await FirestoreManager.GetDocData(GameSetting.GameReportCols.VIP_Month, docName)
    if (playerDocData) {
        await FirestoreManager.UpdateDoc(GameSetting.GameReportCols.VIP_Month, docName, updateData)
    } else {
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.GameReportCols.VIP_Month, docName, updateData);
    }
    res.json({ "Result": "Success" });
    return
});


exports.VIP_AddEXP = functions.region('asia-east1').https.onRequest(async (req, res) => {
    let jsonData = req.body;
    console.log("jsonData: " + JSON.stringify(jsonData));
    // {
    //     "PlayerUID" : "playerUID",
    //     "VIPType" : "Active"/"Purchase",
    //     "Value": xx,
    // }

    // 撈資料&檢查
    const checkJsonDataKey = ["PlayerUID", "VIPType", "Value"]
    checkJsonDataKey.forEach(key => {
        if (!(key in jsonData)) {
            let errMessage = "[ERROR] VIP_AddEXP loss key: " + key + " ,jsonData: " + JSON.stringify(jsonData)
            console.log(errMessage);
            res.json({
                "Result": errMessage,
                "Data": null
            });
            return;
        }
    })

    const playerUID = jsonData["PlayerUID"]
    const vipType = jsonData["VIPType"]
    const value = jsonData["Value"]
    const playerData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, playerUID)
    if (!playerData) {
        let errMessage = "[ERROR] Player not found: " + playerUID
        console.log(errMessage);
        res.json({
            "Result": errMessage,
            "Data": null
        });
        return;
    }
    const playerHistory = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, playerUID)
    if (!playerHistory) {
        console.log("[warning] PlayerHistory do not exist: " + playerUID)
    }

    let updatePlayerData = {
        // VIP (after)
        // MaxVIP (after)
        // ActiveVIPEXP (after)
        // VIPEXP (after)
    };
    let updateHistoryData = {
        // MonthlyVIPEXPBonus (after)
        // TotalPurchase (after)
        // MonthlyPurchase  (after)
    };

    // beforeVIP
    let beforeVIP = 0
    if ("VIP" in playerData) {
        beforeVIP = playerData["VIP"]
    }
    // beforeMaxVIP
    let beforeMaxVIP = 0
    if ("MaxVIP" in playerData) {
        beforeMaxVIP = playerData["MaxVIP"]
    }

    let beforeEXP = 0
    switch (vipType) {
        case GameSetting.VIPType.Active:
            if ("ActiveVIPEXP" in playerData) {
                beforeEXP = playerData["ActiveVIPEXP"]
            } else {
                console.log("[normal/ERROR] Player: " + playerUID + "Get ActiveVIPEXP failed?", JSON.stringify(playerData))
            }
            const ActiveVIPLevelChange = exports.VIPLevelDiff(vipType, beforeEXP, value)
            console.log("[normal] Player Active VIP: " + playerUID + " ,LvDiff: " + JSON.stringify(ActiveVIPLevelChange) + " ,beforeEXP: " + beforeEXP + " + " + value + " ,beforeVIP: ", beforeVIP)
            if (ActiveVIPLevelChange["newLv"] > beforeVIP) {
                updatePlayerData["VIP"] = ActiveVIPLevelChange["newLv"]
                if (ActiveVIPLevelChange["newLv"] > beforeMaxVIP) {
                    updatePlayerData["MaxVIP"] = ActiveVIPLevelChange["newLv"]
                } else {
                    updatePlayerData["MaxVIP"] = beforeMaxVIP
                }
            } else {
                updatePlayerData["VIP"] = beforeVIP
                updatePlayerData["MaxVIP"] = beforeMaxVIP
            }
            updatePlayerData["ActiveVIPEXP"] = admin.firestore.FieldValue.increment(value)
            break
        case GameSetting.VIPType.Purchase:
            if ("VIPEXP" in playerData) {
                beforeEXP = playerData["VIPEXP"]
            } else {
                console.log("[normal/ERROR] Player: " + playerUID + " Get VIPEXP failed?", JSON.stringify(playerData))
            }

            // 首儲判斷
            if ((!playerHistory) ||
                (!("MonthlyVIPEXPBonus" in playerHistory)) ||
                (!(GameSetting.MonthlyVIPEXPBonusType.FirstPurchase in playerHistory["MonthlyVIPEXPBonus"]))) {
                let nowTimestamp = admin.firestore.Timestamp.now();
                let firstPurchaseVipExpBonus = {
                    BonusType: GameSetting.MonthlyVIPEXPBonusType.FirstPurchase,
                    CreateTime: nowTimestamp,
                    PerBonus: [value],
                    Counter: 0,
                    AutoDel: false,
                    Enable: true,
                }
                updateHistoryData["MonthlyVIPEXPBonus"] = {}
                updateHistoryData["MonthlyVIPEXPBonus"][GameSetting.MonthlyVIPEXPBonusType.FirstPurchase] = firstPurchaseVipExpBonus
            }

            const PurchaseVIPLevelChange = exports.VIPLevelDiff(vipType, beforeEXP, value)
            console.log("[normal] Player Purchase VIP: " + playerUID + " ,LvDiff: " + JSON.stringify(PurchaseVIPLevelChange) + " ,beforeEXP: " + beforeEXP + " + " + value + " ,beforeVIP: ", beforeVIP)
            if (PurchaseVIPLevelChange["newLv"] > beforeVIP) {
                updatePlayerData["VIP"] = PurchaseVIPLevelChange["newLv"]
                if (PurchaseVIPLevelChange["newLv"] > beforeMaxVIP) {
                    updatePlayerData["MaxVIP"] = PurchaseVIPLevelChange["newLv"]
                } else {
                    updatePlayerData["MaxVIP"] = beforeMaxVIP
                }
            } else {
                updatePlayerData["VIP"] = beforeVIP
                updatePlayerData["MaxVIP"] = beforeMaxVIP
            }
            updateHistoryData["TotalPurchase"] = admin.firestore.FieldValue.increment(value);
            updateHistoryData["MonthlyPurchase"] = admin.firestore.FieldValue.increment(value);
            updatePlayerData["VIPEXP"] = admin.firestore.FieldValue.increment(value)
            break
        default:
            break
    }

    let vipChange = {
        PlayerUID: playerUID,
        VIPType: vipType,
        BeforeVIP: beforeVIP,
        AfterVIP: updatePlayerData["VIP"],
        BeforeEXP: beforeEXP,
        AddEXP: value,
        NewEXP: beforeEXP + value,
        BeforeMaxVIP: beforeMaxVIP,
        AfterMaxVIP: updatePlayerData["MaxVIP"],
    };

    if (playerHistory) {
        if (Object.keys(updateHistoryData).length != 0)
            await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, playerUID, updateHistoryData);
    } else {
        if (Object.keys(updateHistoryData).length != 0)
            await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, playerUID, updateHistoryData);
    }

    //寫入玩家PlayerData-Player資料
    if (Object.keys(updatePlayerData).length != 0) {
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, playerUID, updatePlayerData);
    }

    Logger.VIP_AddEXP(vipChange)
    res.json({ "Result": "Success" });
    return
});