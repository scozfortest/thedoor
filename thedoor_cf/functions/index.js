//基本設定
const admin = require('firebase-admin');
admin.initializeApp();

//工具
const Tools = require('./Tools');
exports.GetDataSize = Tools.GetDataSize;

//玩家登入/註冊
const PlayerSign = require('./PlayerSign');
exports.SignUp = PlayerSign.SignUp;
exports.SignIn = PlayerSign.SignIn;
exports.SetDevice = PlayerSign.SetDevice;
exports.UpdateOnlineTimestamp = PlayerSign.UpdateOnlineTimestamp;
exports.TriggerEvent = PlayerSign.TriggerEvent;

//冒險
const Adventure = require('./Adventure');
exports.CreateRole = Adventure.CreateRole;
exports.RemoveRole = Adventure.RemoveRole;
exports.CreateAdventure = Adventure.CreateAdventure;
exports.UpdateAdventure = Adventure.UpdateAdventure;



//Client取Server時間
const GetServerTime = require('./GetServerTime');
exports.GetServerTime = GetServerTime.GetServerTime;


//GM工具
const GMTools = require('./GMTools');
exports.AddDatas = GMTools.AddDatas;
exports.GiveItem = GMTools.GiveItem;
exports.GiveCurrencyTest = GMTools.GiveCurrencyTest;
exports.Test = GMTools.Test;



//商城
const Shop = require('./Shop');
exports.Shop_Buy = Shop.Shop_Buy;//購買商城品項
//儲值
const Purchase = require('./Purchase');
exports.Purchase = Purchase.Purchase;//付費儲值
exports.SetBougthShopUID = Purchase.SetBougthShopUID;//設定玩家最後購買的PurchaseUID




//排程
const Crontab = require('./Crontab');
exports.Crontab_PlayerOnlineStateUpdate = Crontab.Crontab_PlayerOnlineStateUpdate;//一段時間自動把超時未更新的上線玩家設回下



