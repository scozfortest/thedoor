//基本設定
const admin = require('firebase-admin');
const axios = require("axios");
const jwt = require('jsonwebtoken');
//自訂方法
const GameSetting = require('./GameSetting.js');
const Logger = require('./Logger.js');
const FirestoreManager = require('../FirebaseTools/FirestoreManager.js');
const PlayerItemManager = require('./PlayerItemManager.js');
const GameDataManager = require('./GameDataManager.js');

var methods = {
    SECRET: 'Wte56gaMtrP7z',
    // 呼叫API
    CallAPI: async function (method, url, params) {
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
    },
    // 驗證呼叫者Token
    CheckToken: function (req) {
        let token = req.header('Authorization').replace('Bearer ', '');
        console.log(token)
        try {
            var jwtData = jwt.verify(token, methods.SECRET);
        } catch (error) {
            return 'Invalid token';
        }
        return jwtData;
    },
    // 驗證交易Token
    CheckTradeToken: function (req, uid) {
        let token = req.body.tradeToken;
        console.log(token)
        try {
            var jwtData = jwt.verify(token, methods.SECRET);
        } catch (error) {
            return 'Invalid token';
        }
        console.log('此次交易者們為', jwtData.PlayerList)
        return jwtData.PlayerList.includes(uid);
    },
    // 獲取交易Log
    GetTradeLog: function (req) {
        let token = req.body.tradeToken;
        return token.substr(-30);
    },
    //執行交易
    Trade: async function (log) {
        await FirestoreManager.UpdateDoc(GameSetting.ExchangesLogCols.TradeLog, log, { Progress: admin.firestore.FieldValue.arrayUnion("交易") });
        let doc = await FirestoreManager.GetDocData(GameSetting.ExchangesLogCols.TradeLog, log);
        if (doc.Data1.tradeToken == doc.Data2.tradeToken && doc.Data1.fromCardId == doc.Data2.fromCardId && doc.Data1.toCardId == doc.Data2.toCardId) {
            let PlayerList = jwt.verify(doc.Data1.tradeToken, methods.SECRET).PlayerList;
            console.log(`交易者為${PlayerList}`)
            console.log(`玩家1要給出的物件為${doc.Data1.fromCardId}`)
            console.log(`玩家2要給出的物件為${doc.Data1.toCardId}`)
            // 檢查玩家1是否擁有物件1
            let check1_1 = await CheckIsOwnStuff(PlayerList[0], doc.Data1.fromCardId);
            // 檢查玩家1是否擁有物件2
            let check1_2 = await CheckIsOwnStuff(PlayerList[0], doc.Data1.toCardId);
            // 檢查玩家2是否擁有物件2
            let check2_1 = await CheckIsOwnStuff(PlayerList[1], doc.Data1.toCardId);
            // 檢查玩家1是否擁有物件2
            let check2_2 = await CheckIsOwnStuff(PlayerList[1], doc.Data1.fromCardId);
            if (check1_1 && !check1_2 && check2_1 && !check2_2) {
                let item1 = `Stuff.${doc.Data1.fromCardId}`;
                let item2 = `Stuff.${doc.Data1.toCardId}`;
                // 把物件2給玩家1 並從玩家1那移除物件1
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Item, PlayerList[0], { [item2]: admin.firestore.FieldValue.increment(1), [item1]: admin.firestore.FieldValue.increment(-1) })
                // 把物件1給玩家2 並從玩家2那移除物件2
                await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Item, PlayerList[1], { [item1]: admin.firestore.FieldValue.increment(1), [item2]: admin.firestore.FieldValue.increment(-1) })
                // 紀錄交易成功LOG
                await FirestoreManager.UpdateDoc(GameSetting.ExchangesLogCols.TradeLog, log, { Progress: admin.firestore.FieldValue.arrayUnion("成功"), Result: "Success", Msg: "交易成功" });
            } else {
                await FirestoreManager.UpdateDoc(GameSetting.ExchangesLogCols.TradeLog, log, { Progress: admin.firestore.FieldValue.arrayUnion("失敗"), Result: "Fail", Msg: "道具已擁有" });
            }
            return;
        }
        await FirestoreManager.UpdateDoc(GameSetting.ExchangesLogCols.TradeLog, log, { Progress: admin.firestore.FieldValue.arrayUnion("失敗"), Result: "Fail", Msg: "雙方資料不匹配" });
        return;
    },
    // 解析交易log
    CheckTicketToken: function (req) {
        let token = req.body.ticket;
        console.log(token)
        try {
            var jwtData = jwt.verify(token, methods.SECRET);
        } catch (error) {
            return 'Invalid token';
        }
        return jwtData.LogUID;
    }
}
module.exports = methods;

async function CheckIsOwnStuff(PlayerUID, ItemValue) {
    let doc = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Item, PlayerUID);
    if ("Stuff" in doc && String(ItemValue) in doc.Stuff && doc["Stuff"][String(ItemValue)] > 0) {
        return true;
    }
    return false;
}