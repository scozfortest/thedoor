//基本設定

const axios = require('axios');
var methods = {
    //傳入ID陣列與權重陣列取得ID陣列索引
    SendPost: async function (url, jsonObj) {
        let res = await axios.post(url, jsonObj, { timeout: 3600000 });//逾時設定為一小時
        let data = res.data;
        return data;
    },
}
module.exports = methods;
