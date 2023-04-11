//Google Play API
const jwt = require('jsonwebtoken');
const keyData = require('../Key/majampachinko-release-a8f6fc1db261.json');         // Path to your JSON key file
const request = require('request-promise');

/** 
 * Exchanges the private key file for a temporary access token,
 * which is valid for 1 hour and can be reused for multiple requests
 */
function getAccessToken(keyData) {
    // Create a JSON Web Token for the Service Account linked to Play Store
    const token = jwt.sign(
        { scope: 'https://www.googleapis.com/auth/androidpublisher' },
        keyData.private_key,
        {
            algorithm: 'RS256',
            expiresIn: '1h',
            issuer: keyData.client_email,
            subject: keyData.client_email,
            audience: 'https://www.googleapis.com/oauth2/v4/token'
        }
    );

    // Make a request to Google APIs OAuth backend to exchange it for an access token
    // Returns a promise
    return request.post({
        uri: 'https://www.googleapis.com/oauth2/v4/token',
        form: {
            'grant_type': 'urn:ietf:params:oauth:grant-type:jwt-bearer',
            'assertion': token
        },
        transform: body => JSON.parse(body).access_token
    });
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

var methods = {
    //取得訂閱狀態
    GetSubscription: async function (packageName, subscriptionId, purchaseToken) {
        // The API url to call 取得訂閱狀態
        var result = null;
        const url = `https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${packageName}/purchases/subscriptions/${subscriptionId}/tokens/${purchaseToken}`;
        await getAccessToken(keyData).then(async (token) => {
            await makeApiRequest(url, token).then(async (data) => {
                //console.log("GetSubscription : " + JSON.stringify(data));
                result = data;
            }).catch(err => {
                console.log("makeApiRequest Err :" + err);
            });
        }).catch(err => {
            console.log("getAccessToken Err :" + err);
        });
        return result;
    },
    //取得內購商品狀態
    GetProduct: async function (packageName, productId, purchaseToken) {
        //console.log("packageName : " + packageName);
        //console.log("productId : " + productId);
        //console.log("purchaseToken : " + purchaseToken);

        // The API url to call 取得product狀態
        var result = null;
        const url = `https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${packageName}/purchases/products/${productId}/tokens/${purchaseToken}`;
        //console.log("url : " + url);
        await getAccessToken(keyData).then(async (token) => {
            //console.log("token: " + token);
            await makeApiRequest(url, token).then(async (data) => {
                //console.log("GetProduct: " + JSON.stringify(data));
                result = data;
            }).catch(err => {
                console.log("makeApiRequest Err :" + err);
            });
        }).catch(err => {
            console.log("getAccessToken Err :" + err);
        });
        return result;
    },
}
module.exports = methods;