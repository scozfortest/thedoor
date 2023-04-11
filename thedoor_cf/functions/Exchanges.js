//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
const express = require('express');
const jwt = require('jsonwebtoken');
const cors = require('cors');
//專案ID
const GCloudProject = process.env.GCLOUD_PROJECT
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const Logger = require('./GameTools/Logger.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const ExchangesManager = require('./GameTools/ExchangesManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');

const app = express();
const SECRET = ExchangesManager.SECRET;
const bucketUrls = {
    [GameSetting.GCloudProjects.Dev]: `https://storage.googleapis.com/majampachinko_exchanges_develop`,
    [GameSetting.GCloudProjects.Test]: `https://storage.googleapis.com/majampachinko_exchanges_test`,
    [GameSetting.GCloudProjects.Release]: `https://storage.googleapis.com/majampachinko_exchanges_release`
}
const bucketUrl = bucketUrls[GCloudProject];

exports.Exchanges_GetLink = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    // 玩家ID
    let uid = context.auth.uid;
    let doc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, uid);
    let setting = await FirestoreManager.GetDocData(GameSetting.ExchangesDataCols.Setting, "Version");
    if (doc == null || (setting.CheckingPoint == "GetLink" && doc.VIP < setting.VIPLimit)) {
        return {
            code: 2,
            link: "",
            token: ""
        };
    }
    // 加密
    console.log(`https://asia-east1-majampachinko-${GCloudProject}.cloudfunctions.net/exchanges/user/getToken`);
    console.log({ id: uid });
    let tokenData = await ExchangesManager.CallAPI('post', `https://asia-east1-${GCloudProject}.cloudfunctions.net/exchanges/user/getToken`, { id: uid });
    // 獲取網址
    let BackendURLData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Setting, "BackendURL");
    return {
        code: 1,
        link: `${BackendURLData.TradeSystemURL}?token=${tokenData.token}`,
        token: tokenData.token
    };

});


exports.Exchanges = functions.region('asia-east1').https.onRequest(app);
app.use(cors({ origin: true }));
// 獲取Token
app.post('/user/getToken', async (req, res) => {
    let data = { id: req.body.id || "#" };
    let token = jwt.sign(data, SECRET)
    res.json({
        code: 1,
        token: token
    });
    return;
});

// 驗證使用者Token
app.use('/user/verify', async (req, res) => {
    // 驗證token 得到uid
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        res.status(400).send('Invalid token');
        return;
    }
    let uid = jwtData.id;
    // 獲取呼叫者資訊
    let exist = await FirestoreManager.CheckDocExist(GameSetting.PlayerDataCols.Player, uid);
    if (!exist) {
        res.json({ code: 0 });
        return;
    }
    res.json({ code: 1 });
    return;
});

// 取得使用者基本資料
app.use('/user/detail', async (req, res) => {
    // 驗證token 得到uid
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        res.status(400).send('Invalid token');
        return;
    }
    let uid = jwtData.id
    // 獲取玩家資訊
    let doc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, uid);
    if (doc == null) {
        res.json({ code: 0 });
        return;
    }
    let imageUrl = GameDataManager.GetData(GameSetting.GameJsonNames.Icon, String(doc.CurIconID)).Ref
    res.json({
        code: 1,
        data: {
            id: doc.UID,
            name: doc.Name,
            imageUrl: `${bucketUrl}/Icon/${imageUrl}.png`
        }
    });
    return;
});

// 取得使用者所有卡片
app.use('/user/cards', async (req, res) => {
    // 驗證token
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        res.status(400).send('Invalid token');
        return;
    }
    var uid = jwtData.id
    // 獲取呼叫者資訊
    let exist = await FirestoreManager.CheckDocExist(GameSetting.PlayerDataCols.Player, uid);
    if (!exist) {
        res.json({ code: 0 });
        return;
    }
    // 獲取呼叫者道具
    let item = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, uid);
    let stuff = GameDataManager.GetJson(GameSetting.GameJsonNames.Stuff)
    let RoleFragmentList = [];
    let output = [];
    // 獲取是碎片的道具List
    for (let s of stuff) {
        if (s.StuffType == "RoleFragment") {
            RoleFragmentList.push(s.ID);
        }
    }
    // 獲取呼叫者已經合成的卡片
    let roles = await FirestoreManager.GetDocs_Where(GameSetting.PlayerDataCols.Role, "OwnerUID", uid);
    let isOwnRole = [];
    if (roles != null) {
        for (let role of roles) {
            isOwnRole.push(String(role.data()["RoleID"]));
        }
    }
    // 獲取交易者持有卡牌
    let item2 = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, req.body.id);
    let cantTrade = [];
    if (item2 != null && "Stuff" in item2 && item2.Stuff instanceof Object) {
        for (let [k, v] of Object.entries(item2.Stuff)) {
            if (v > 0) {
                cantTrade.push(k);
            }
        }
    }
    // 篩選出呼叫者持有卡片
    if ("Stuff" in item && item.Stuff instanceof Object) {
        for (let [k, v] of Object.entries(item.Stuff)) {
            if (isOwnRole.includes(k.slice(0, -1))) {
                continue;
            } else if (RoleFragmentList.includes(k) && v > 0) {
                output.push({
                    cardId: k,
                    count: v,
                    name: GameDataManager.GetStr(`Role_${k.slice(0, -1)}`, "Name_TW"),
                    rank: parseInt(GameDataManager.GetData(GameSetting.GameJsonNames.Stuff, k).Rank),
                    frameImageUrl: `${bucketUrl}/Frame/frame${GameDataManager.GetData(GameSetting.GameJsonNames.Stuff, k).Rank}.png`,
                    roleImageUrl: `${bucketUrl}/Role/${k.slice(0, -1)}/Role${k.slice(-1)}.png`,
                    bgImageUrl: `${bucketUrl}/Role/${k.slice(0, -1)}/RoleBG.png`,
                    roleFragmentImageUrl: `${bucketUrl}/RoleFragment/${k}.png`,
                    tradeStatus: !(cantTrade.includes(k))
                })
            }
        }
    }
    // 暫存Object
    dict = {}
    for (let c of output) {
        let roleId = c.cardId.slice(0, -1);
        if (!dict.hasOwnProperty(roleId)) {
            dict[roleId] = {
                name: c.name,
                bgImageUrl: c.bgImageUrl,
                frameImageUrl: c.frameImageUrl,
                frames: [{
                    cardId: c.cardId,
                    count: 1,
                    rank: c.rank,
                    roleFragmentImageUrl: c.roleFragmentImageUrl,
                    tradeStatus: c.tradeStatus,
                    roleImageUrl: c.roleImageUrl,
                }],
            }
        } else {
            dict[roleId]["frames"].push({
                cardId: c.cardId,
                count: 1,
                rank: c.rank,
                roleFragmentImageUrl: c.roleFragmentImageUrl,
                tradeStatus: c.tradeStatus,
                roleImageUrl: c.roleImageUrl,
            })
        }
    }
    output = Object.values(dict);
    res.json({
        code: 1,
        data: {
            cards: output,
            common: {
                fragmentBlack1ImageUrl: `${bucketUrl}/Mask/fragmentBlack1.png`,
                fragmentBlack2ImageUrl: `${bucketUrl}/Mask/fragmentBlack2.png`,
                fragmentBlack3ImageUrl: `${bucketUrl}/Mask/fragmentBlack3.png`,
                lockImageUrl: `${bucketUrl}/Mask/lock.png`,
                maskSmallImageUrl: `${bucketUrl}/Mask/maskSmall.png`,
                maskBigImageUrl: `${bucketUrl}/Mask/maskBig.png`,
            }
        }
    });
    return;
});

// 取得交易者基本資料
app.post('/otherUser/detail', async (req, res) => {
    // 驗證token
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        res.status(400).send('Invalid token');
        return;
    }
    var uid = jwtData.id
    // 獲取呼叫者資訊
    var exist = await FirestoreManager.CheckDocExist(GameSetting.PlayerDataCols.Player, uid);
    if (!exist) {
        res.json({ code: 0 });
        return;
    }
    // 獲取要查詢的對象
    var uid = req.body.id;
    // 獲取玩家資訊
    let doc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, uid);
    if (doc == null) {
        res.json({ code: 0 });
        return;
    }
    let imageUrl = GameDataManager.GetData(GameSetting.GameJsonNames.Icon, String(doc.CurIconID)).Ref
    res.json({
        code: 1,
        data: {
            id: doc.UID,
            name: doc.Name,
            imageUrl: `${bucketUrl}/Icon/${imageUrl}.png`
        }
    });
    return;
});

// 取得交易Token
app.post('/trade/getToken', async (req, res) => {
    console.log('觸發獲取交易token')
    console.table(req.body)
    // 驗證token
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        res.status(400).send('Invalid token');
        return;
    }
    var uid = jwtData.id
    // 獲取呼叫者資訊
    var doc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, uid);
    if (doc == null) {
        res.json({ code: 0 });
        return;
    }
    let setting = await FirestoreManager.GetDocData(GameSetting.ExchangesDataCols.Setting, "Version");
    if (setting.CheckingPoint == "TradeToken" && doc.VIP < setting.VIPLimit) {
        res.json({
            code: 2,
            data: {
                msg: `玩家${uid} VIP等級不足，無法進行交易`
            }
        });
    }
    // 獲取要查詢的對象
    var uid = req.body.id;
    // 獲取玩家資訊
    var doc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, uid);
    if (doc == null) {
        res.json({ code: 0 });
        return;
    }
    if (setting.CheckingPoint == "TradeToken" && doc.VIP < setting.VIPLimit) {
        res.json({
            code: 2,
            data: {
                msg: `玩家${uid} VIP等級不足，無法進行交易`
            }
        });
    }
    let data = { PlayerList: [jwtData.id, req.body.id] };
    console.log("密碼是" + SECRET)
    let token = jwt.sign(data, SECRET)
    res.json({
        code: 1,
        data: { tradeToken: token }
    });
    return;
});

// 交易卡片
app.post('/trade/add', async (req, res) => {
    // 驗證token
    console.log('觸發交易')
    console.table(req.body)
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        console.log("jwtData == Invalid token(呼叫者token驗證失敗)");
        res.status(400).send('Invalid token');
        return;
    }
    // 驗證交易token
    let uid = jwtData.id
    let checkResult = ExchangesManager.CheckTradeToken(req, uid);
    if (checkResult == 'Invalid token') {
        console.log("checkResult == Invalid token(交易token驗證失敗)");
        res.status(400).send('Invalid token');
        return;
    } else if (checkResult == false) {
        console.log("checkResult == false(交易者裡沒有此玩家)");
        res.json({ code: 0 });
        return;
    }
    // 將此紀錄存到資料庫
    let log = ExchangesManager.GetTradeLog(req);
    let doc = await FirestoreManager.GetDocData(GameSetting.ExchangesLogCols.TradeLog, log);
    if (doc == null) {
        console.log('玩家1觸發交易')
        await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.ExchangesLogCols.TradeLog, log, {
            PlayerUID1: uid,
            Data1: {
                tradeToken: req.body.tradeToken,
                fromCardId: req.body.fromCardId,
                toCardId: req.body.toCardId,
            },
            Progress: ["開始", "玩家1確定"]
        });
        var firstCall = true;
    } else if (admin.firestore.Timestamp.now() - doc.CreateTime > 15) {
        console.log('玩家2觸發交易，但太晚交易了，交易失敗')
        res.json({ code: 0 });
        return;
    } else {
        console.log('玩家2觸發交易')
        await FirestoreManager.UpdateDoc(GameSetting.ExchangesLogCols.TradeLog, log, {
            PlayerUID2: uid,
            Data2: {
                tradeToken: req.body.tradeToken,
                fromCardId: req.body.fromCardId,
                toCardId: req.body.toCardId,
            },
            Progress: admin.firestore.FieldValue.arrayUnion("玩家2確定")
        });
        // 執行交易
        await ExchangesManager.Trade(log);
        var firstCall = false;
    }
    // 回傳紀錄log ID (加密後的)
    let data = { LogUID: log };
    let token = jwt.sign(data, SECRET)
    res.json({
        code: 1,
        data: {
            ticket: token,
            firstCall: firstCall
        }
    });
    return;
});

// 確認交易
app.post('/trade/confirm', async (req, res) => {
    // 驗證token
    let jwtData = ExchangesManager.CheckToken(req);
    if (jwtData == 'Invalid token') {
        res.status(400).send('Invalid token');
        return;
    }
    //解析TicketToken
    let logID = ExchangesManager.CheckTicketToken(req);
    // console.log('log解析出來是', logID)
    let log = await FirestoreManager.GetDocData(GameSetting.ExchangesLogCols.TradeLog, logID);
    if (log == null || ("Result" in log && log.Result == "Fail") || (admin.firestore.Timestamp.now() - log.CreateTime > 15 && !("Result" in log))) {
        res.json({
            code: 1,
            data: { status: "01" }
        });
    } else if ("Result" in log && log.Result == "Success") {
        res.json({
            code: 1,
            data: { status: "00" }
        });
    } else {
        res.json({
            code: 1,
            data: { status: "02" }
        });
    }
    return;
});