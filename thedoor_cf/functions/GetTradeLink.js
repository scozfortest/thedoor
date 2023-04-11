//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
const axios = require("axios");
//自訂方法
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const CollectionName = require('./GameTools/GameSetting.js');

//獲取加密後的交易系統網址
exports.GetTradeLink = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    // 玩家ID
    let uid = context.auth.uid;
    // 密碼
    let BackendURLData = await FirestoreManager.GetDocData(CollectionName.GameDataCols.Setting, "BackendURL");
    let key = BackendURLData.TradeSystemKey;
    // 加密
    let token = await callAPI('get', `${BackendURLData.TradeSystemURL}/GetJWTToken`, { PlayerID: uid, key: key });
    return {
        Result: "Success",
        Link: `${BackendURLData.TradeSystemURL}/Login?token=${token}`,
    };

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