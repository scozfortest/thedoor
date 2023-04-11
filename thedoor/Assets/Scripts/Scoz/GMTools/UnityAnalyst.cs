using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Scoz.Func;


public class UnityAnalyst : MonoBehaviour
{
    public static bool CnaSendAnalysis = false;
    public static bool ShowLog = false;
    public static void SendEnterGame()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendEnterGame");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("EnterGame");
    }
    public static void SendStartGame()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendStartGame");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("StartGame");
    }
    public static void SendStartChallenge()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendStartChallenge");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("SendStartChallenge");
    }
    public static void SendLeaveRoom()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendLeaveRoom");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("LeaveRoom");
    }
    public static void SendDisconnected()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-Disconnected");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Disconnected");
    }
    public static void SendWatchTutorial()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendWatchTutorial");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("WatchTutorial");
    }
    public static void SendChangeLanguage(Language _language)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendChangeLanguage");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("ChangeLanguage", new Dictionary<string, object>
        {
            { "language", _language.ToString() }
        });
    }
    public static void SendPlayerLogin(string _name)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPlayerLogin");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("PlayerLogin", new Dictionary<string, object>
        {
            { "name", _name }
        });
    }
    public static void SendClickFullScreen()
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendClickFullScreen");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("ClickFullScreen");
    }
    public static void SendMatchResult(int _level, int _kill, int _star, int _survivalTime)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendMatchResult");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("MatchResult", new Dictionary<string, object>
        {
            { "level", _level },
            { "kill", _kill },
            { "star", _star },
            { "survivalTime(s)", _survivalTime }
        });
    }
    public static void SendPurchaseRevive(string _buyer, int _kreds)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPurchaseRevive");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Purchase-Revive", new Dictionary<string, object>
        {
            { "buyer", _buyer },
            { "cost", _kreds }
        });
    }
    public static void SendPurchasePremium(string _buyer, int _kreds)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPurchasePremium");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Purchase-Premium", new Dictionary<string, object>
        {
            { "buyer", _buyer },
            { "cost", _kreds }
        });
    }
    public static void SendPurchaseStarItem(string _buyer, int _itemID, int _stars)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPurchaseStarItem");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Purchase-StarItem", new Dictionary<string, object>
        {
            { "buyer", _buyer },
            { "itemID", _itemID },
            { "cost", _stars }
        });
    }
    public static void SendPurchaseKredItem(string _buyer, int _itemID, int _kreds)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPurchaseKredItem");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Purchase-KredItem", new Dictionary<string, object>
        {
            { "buyer", _buyer },
            { "itemID", _itemID },
            { "cost", _kreds }
        });
    }
    public static void SendPurchaseStarClamor(string _buyer, int _clamorID, int _stars)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPurchaseStarClamor");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Purchase-StarClamor", new Dictionary<string, object>
        {
            { "buyer", _buyer },
            { "clamorID", _clamorID },
            { "cost", _stars }
        });
    }
    public static void SendPurchaseKredClamor(string _buyer, int _clamorID, int _kreds)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendPurchaseKredClamor");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Purchase-KredClamor", new Dictionary<string, object>
        {
            { "buyer", _buyer },
            { "clamorID", _clamorID },
            { "cost", _kreds }
        });
    }
    public static void SendRegion(string _region, int _ping)
    {
        if (ShowLog)
            DebugLogger.Log("Analysis-SendRegion");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("Region", new Dictionary<string, object>
        {
            { "region", _region },
            { "ping(ms)", _ping },
        });
    }
    /*做不到，因為網頁可以按X關閉網頁，所以不可能知道玩家玩多久
    public static void SendPlayInfo(double _playTime, int _playTimes, int _kill, int _die, int _gainStar, int _saveClamorTimes)
    {
        DebugLogger.Log("Analysis-SendPlayInfo");
        if (!CnaSendAnalysis)
            return;
        Analytics.CustomEvent("PlayInfo", new Dictionary<string, object>
        {
            { "playTime(minutes)", _playTime },
            { "playTimes", _playTimes },
            { "kill", _kill },
            { "die", _die },
            { "gainStar", _gainStar },
            { "saveClamorTimes", _saveClamorTimes }
        });
    }
    */
}
