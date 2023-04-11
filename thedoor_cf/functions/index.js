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
exports.ChangeName = PlayerSign.ChangeName;
exports.ChangeIntro = PlayerSign.ChangeIntro;
exports.SetGuide = PlayerSign.SetGuide;
exports.TriggerEvent = PlayerSign.TriggerEvent;
exports.CreateInvitationCodes = PlayerSign.CreateInvitationCodes;



//Client取Server時間
const GetServerTime = require('./GetServerTime');
exports.GetServerTime = GetServerTime.GetServerTime;



//獲取交易系統Token
const GetTradeLink = require('./GetTradeLink');
exports.GetTradeLink = GetTradeLink.GetTradeLink;



//GM工具
const GMTools = require('./GMTools');
exports.AddDatas = GMTools.AddDatas;
exports.GiveItem = GMTools.GiveItem;
exports.GiveCurrencyTest = GMTools.GiveCurrencyTest;
exports.Test = GMTools.Test;




//麻將相關
const MaJamPachinko = require('./MaJamPachinko');
exports.GetRandomAIPlayerIDs = MaJamPachinko.GetRandomAIPlayerIDs;
exports.GetMaJamRoomData = MaJamPachinko.GetMaJamRoomData;
exports.CreateMaJamMatchingRoom = MaJamPachinko.CreateMaJamMatchingRoom;
exports.RemoveMaJamMatchingRoom = MaJamPachinko.RemoveMaJamMatchingRoom;
exports.JoinMaJamMatchingRoom = MaJamPachinko.JoinMaJamMatchingRoom;
exports.MaJamMatchingStartGame = MaJamPachinko.MaJamMatchingStartGame;
exports.MaJamMatchingRemovePlayer = MaJamPachinko.MaJamMatchingRemovePlayer;
exports.UpdateMaJamMatchingRoomTimestamp = MaJamPachinko.UpdateMaJamMatchingRoomTimestamp;
exports.MaJamMatchingInvite = MaJamPachinko.MaJamMatchingInvite;
exports.RemoveMaJamMatchingInvite = MaJamPachinko.RemoveMaJamMatchingInvite;
exports.MaJamEndRound = MaJamPachinko.MaJamEndRound;
exports.MaJamEndGame = MaJamPachinko.MaJamEndGame;
exports.RecordMaJamHistory = MaJamPachinko.RecordMaJamHistory;
exports.OnRoomRemove = MaJamPachinko.OnRoomRemove;


//社群相關
const Social = require('./Social');
exports.Social_SendFriendRequest = Social.Social_SendFriendRequest;
exports.Social_AcceptFriendRequest = Social.Social_AcceptFriendRequest;
exports.Social_RemoveFriendRequest = Social.Social_RemoveFriendRequest;
exports.Social_RemoveFriend = Social.Social_RemoveFriend;


//商城
const Shop = require('./Shop');
exports.Shop_Buy = Shop.Shop_Buy;//購買商城品項
//儲值
const Purchase = require('./Purchase');
exports.Purchase = Purchase.Purchase;//付費儲值
exports.SetBougthShopUID = Purchase.SetBougthShopUID;//設定玩家最後購買的PurchaseUID


//信件
const Mail = require('./Mail');
exports.Mail_Claim = Mail.Mail_Claim;//領取信件
exports.Mail_ClaimAll = Mail.Mail_ClaimAll;//領取全部信件

//跑馬燈
const NewsTicker = require('./NewsTicker');
exports.NewsTicker_AddMsg = NewsTicker.NewsTicker_AddMsg;//新增訊息
//exports.NewsTicker_TmpSendAllMail = NewsTicker.NewsTicker_TmpSendAllMail;//暫時用 送全服玩家信

//腳色
const Role = require('./Role');
exports.Role_Combine = Role.Role_Combine;//合成腳色碎片
exports.Role_GetRandomCalls = Role.Role_GetRandomCalls;//取得隨機腳色來電
exports.Role_GetPlotReward = Role.Role_GetPlotReward;//完成情境對話後領獎
exports.Role_RemoveCall = Role.Role_RemoveCall;//刪除來電
exports.Role_SetCalled = Role.Role_SetCalled;//將該來電設定為已經顯示給玩家過了



//蒐藏
const Collection = require('./Collection');
exports.Collection_SetFavoriteEmojiID = Collection.Collection_SetFavoriteEmojiID;//更改Emoji最愛清單
exports.Collection_SetUseVoiceIDs = Collection.Collection_SetUseVoiceIDs;//設定出牌語音清單
exports.Collection_SetUseItem = Collection.Collection_SetUseItem;//設定目前使用中的道具


//排程
const Crontab = require('./Crontab');
exports.Crontab_PlayerOnlineStateUpdate = Crontab.Crontab_PlayerOnlineStateUpdate;//一段時間自動把超時未更新的上線玩家設回下
exports.Crontab_MaJamMatchingRoomTimeout = Crontab.Crontab_MaJamMatchingRoomTimeout;//一段時間自動移除超時未更新的好友配對房間
//exports.Crontab_RemoveTimeLimitMail = Crontab.Crontab_RemoveTimeLimitMail;//移除未領取的限時信件
exports.Crontab_UpdateLeaderboard = Crontab.Crontab_UpdateLeaderboard;//每小時更新排行榜資料
exports.Crontab_CheckBackStageSchedule = Crontab.Crontab_CheckBackStageSchedule;//每2分鐘檢查工作排程

//每日相關
const DailyReward = require('./DailyReward');
exports.DailyReward_GetReward = DailyReward.DailyReward_GetReward;//簽到簿
exports.HitTheDog_GetReward = DailyReward.HitTheDog_GetReward;//擊娃娃
exports.DailyReward_GetWatchADReward = DailyReward.DailyReward_GetWatchADReward;//廣告獎勵
exports.StreetFighter_Start = DailyReward.StreetFighter_Start;//巷戰古惑仔請求開始
exports.StreetFighter_GetReward = DailyReward.StreetFighter_GetReward;//巷戰古惑仔領取獎勵

//交易
const Trade = require('./Trade');
exports.Trade_SendPT = Trade.Trade_SendPT;


//生涯
const Achievement = require('./Achievement');
exports.Achievement_GetLVReward = Achievement.Achievement_GetLVReward;

//任務&成就
const Quest = require('./Quest');
exports.Quest_FinishProgress = Quest.Quest_FinishProgress;
exports.Quest_Claim = Quest.Quest_Claim;

//VIP
const VIP = require('./VIP');
exports.VIP_MonthlyResetVIPEXP = VIP.VIP_MonthlyResetVIPEXP;
exports.VIP_AddEXP = VIP.VIP_AddEXP;

//交易系統API
const Exchanges = require('./Exchanges');
exports.exchanges = Exchanges.Exchanges;//這裡故意命名小寫開頭
exports.Exchanges_GetLink = Exchanges.Exchanges_GetLink;

//刮刮卡
const ScratchCard = require('./ScratchCard');
exports.ScratchCard_Buy = ScratchCard.ScratchCard_Buy;

//領取邀請碼
const GetInvitationReward = require('./GetInvitationReward');
exports.GetInvitationReward = GetInvitationReward.GetInvitationReward;




