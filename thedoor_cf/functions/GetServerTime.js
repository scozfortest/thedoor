//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const MyTime = require('./Scoz/MyTime.js');
const GameSetting = require('./GameTools/GameSetting.js');

//Client取Server時間
exports.GetServerTime = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }
    let nowTime = admin.firestore.Timestamp.now();
    let scozTimeStr = MyTime.ConvertToScozTimeStr(MyTime.AddHours(nowTime.toDate(), 8));

    return {
        Result: GameSetting.ResultTypes.Success,
        Data: scozTimeStr,
    };

});