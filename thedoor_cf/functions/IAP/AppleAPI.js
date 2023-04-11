//Google Play API
//const fs = require('fs');
const jwt = require('jsonwebtoken');
const privateKey = "-----BEGIN PRIVATE KEY-----\nMIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgqi2d6Q+RaYuNEst+\nW5JGZYG8O0ZDOKDFRLC8/Boyb52gCgYIKoZIzj0DAQehRANCAASldPXKMpMPdXK7\nsJQYLVZwRYXHR4eLqhKfzyqWkhgPA4xMhj86nPGUc86OWeyvKCRdEalvrCW+ae36\n/ckw4+L9\n-----END PRIVATE KEY-----";//"fs.readFileSync("functions/Key/SubscriptionKey_78AWAY84DL.p8")
const request = require('request-promise');
const apiKeyId = "78AWAY84DL"
const issuerId = "e6d62271-5635-4d76-8eaf-960c9d6ed7a8"

const verifyReceiptProductionURL = "https://buy.itunes.apple.com/verifyReceipt"
const verifyReceiptSandBoxURL = "https://sandbox.itunes.apple.com/verifyReceipt"

/** 
 * Exchanges the private key file for a temporary access token,
 * which is valid for 1 hour and can be reused for multiple requests
 */
function getAccessToken() {
    let now = Math.round((new Date()).getTime() / 1000); // Notice the /1000 
    let nowPlus20 = now + 1199 // 1200 === 20 minutes

    let payload = {
        "iss": issuerId,
        "iat": now,
        "exp": nowPlus20,
        "aud": "appstoreconnect-v1",
        "bid": "com.among.majampachinkorelease"
    }

    let signOptions = {
        "algorithm": "ES256", // you must use this algorythm, not jsonwebtoken's default
        header : {
            "alg": "ES256",
            "kid": apiKeyId,
            "typ": "JWT"
        }
    };
    // Create a JSON Web Token for the Service Account linked to Play Store
    const token = jwt.sign(payload, privateKey, signOptions);
    //console.log("token : " + token);
    return token;
    // Make a request to Google APIs OAuth backend to exchange it for an access token
    // Returns a promise
    /*return request.get({
        uri: 'https://api.appstoreconnect.apple.com/v1/apps',
        header: {
            'Accept':'application/a-gzip, application/json',
            'Authorization' : 'Bearer ' + token

        },
        transform: body => JSON.parse(body).access_token
    });*/
}

/**
 * Makes a GET request to given URL with the access token
 */
function makeApiRequest(url, accessToken) {
    return request.get({
        url: url,
        auth: {
            bearer: accessToken
        },
        transform: body => JSON.parse(body)
    });
}

async function verifyReceipt(url, payload) {
    return await request.post({
        url: url,
        body: payload,
        //transform: body => payload
    });
}


var methods = {
    //取得訂閱狀態
    GetSubscription: async function(packageName, originalTransactionId) {
        const isTestEnviorment = (process.env.GCLOUD_PROJECT == "majampachinko-test1");
        // The API url to call 取得product狀態
        let appleURL = "api.storekit.itunes.apple.com"
        if (isTestEnviorment)
            appleURL = "api.storekit-sandbox.itunes.apple.com"
        // The API url to call 取得訂閱狀態
        const url = `https://${appleURL}/inApps/v1/subscriptions/${originalTransactionId}`;
        const token = getAccessToken()
        await makeApiRequest(url, token).then(async (data) => {
                return data;
            }).catch(err => {
                console.log("makeApiRequest Err :" + err);
                return null;
            });
        /*await getAccessToken().then(async (token) => {
            await makeApiRequest(url, token).then(async (data) => {
                return data;
            }).catch(err => {
                console.log("makeApiRequest Err :" + err);
                return null;
            });
        }).catch(err => {
            console.log("getAccessToken Err :" + err);
            return null;
        });*/
    },
    //取得內購商品狀態
    GetProduct: async function(packageName, originalTransactionId) {
        const isTestEnviorment = (process.env.GCLOUD_PROJECT == "majampachinko-test1");
        // The API url to call 取得product狀態
        let appleURL = "api.storekit.itunes.apple.com"
        if (isTestEnviorment)
            appleURL = "api.storekit-sandbox.itunes.apple.com"
        const url = `https://${appleURL}/inApps/v1/history/${originalTransactionId}`;
        const token = getAccessToken()
        await makeApiRequest(url, token).then(async (data) => {
            console.log(data);
                return data;
            }).catch(err => {
                console.log("makeApiRequest Err :" + err);
                return null;
            });
        /*await getAccessToken().then(async (token) => {
            await makeApiRequest(url, token).then(async (data) => {
                return data;
            }).catch(err => {
                console.log("makeApiRequest Err :" + err);
                return null;
            });
        }).catch(err => {
            console.log("getAccessToken Err :" + err);
            return null;
        });*/
    },
    ValidateReciept: async function(payLoad) {
        let dic = {};
        dic['receipt-data'] = payLoad;
        dic['password'] = "d9edd09b99c4420783f936d092f46dd4";        
        let jsonData = JSON.stringify(dic);
        let productionData = await verifyReceipt(verifyReceiptProductionURL, jsonData)
        if (productionData == null)
            return null;
        const result = JSON.parse(productionData)
        //console.log("result : " + typeof(result.status) + " " + result.status);
        if (result["status"] == 21007) {
            let sandboxData = await verifyReceipt(verifyReceiptSandBoxURL, jsonData)
            const sandboxResult = JSON.parse(sandboxData)
            //console.log("sandboxResult : " + typeof(sandboxResult.status) + " " + sandboxResult.status);
            return sandboxResult;
        }
        else {
            return result;
        }
    },
}
module.exports = methods;