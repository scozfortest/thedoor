using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

using System;

namespace TheDoor.Main {
    public class LocoNotification : MonoBehaviour {
        public static LocoNotification Instance { get; private set; }
#if UNITY_ANDROID
        const string channelID = "RoleCall";
        List<int> NotificationIDs = new List<int>();

#endif
        public virtual void Init() {
#if UNITY_ANDROID
            AndroidNotificationCenter.Initialize();
            RegisterChannel_Android(channelID, "角色來電通知");
#elif UNITY_IOS
#endif
            Instance = this;
        }





#if UNITY_ANDROID
        void RegisterChannel_Android(string _channelID, string _channelName) {
            var channel = new AndroidNotificationChannel() {
                Id = _channelID,
                Name = _channelName,
                Importance = Importance.Default,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        int SendNotification_Android(string _channel, string _title, string _text, Color _color, DateTime _time) {
            var notification = new AndroidNotification();
            notification.Title = _title;
            notification.Text = _text;
            notification.FireTime = _time;
            notification.LargeIcon = "logo";
            notification.Color = _color;
            return AndroidNotificationCenter.SendNotification(notification, _channel);
        }
        NotificationStatus GetNotificationState_Andoird(int _id) {
            var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(_id);
            return notificationStatus;
        }
        public void CancelAllNotifications_Android() {
            AndroidNotificationCenter.CancelAllNotifications();
        }
        public void AddRoleCallNotification_Android(int _roleID, DateTime _callTime) {
            //超過時間的來電就不設定推播了，通常上線中的玩家不會收到推播，因為推播的時間設定會晚CallTime60秒，所以會每次呼叫此Functiong時，推播都會被清掉
            if (GameManager.Instance.NowTime > _callTime) return;
            RoleData roleData = RoleData.GetData(_roleID);
            DateTime time = _callTime.AddHours(GameManager.Instance.LocoHourOffsetToServer).AddSeconds(60);//晚60秒，因為玩家在線收到來電要移除推播，晚點收到推播才來得及移除
            TimeSpan offsetFromNow = (time - DateTime.Now);
            time = DateTime.Now.AddSeconds(offsetFromNow.TotalSeconds);
            //WriteLog.LogError("腳色ID=" + roleData.ID + "的推播 時間: " + time);
            string title = "title";
            string content = "content";

            int id = SendNotification_Android(channelID, title, content, new Color32(255, 180, 0, 255), time);
            NotificationIDs.Add(id);
            //Debug.LogError("///////////Finished  " + GetNotificationState_Andoird(id));

            /*
            for (int i = 0; i < NotificationIDs.Count; i++) {
                Debug.LogWarning(NotificationIDs[i] + ":" + GetNotificationState_Andoird(NotificationIDs[i]));
            }
            */

            //遊戲中收到推播就取消該推播
            AndroidNotificationCenter.NotificationReceivedCallback receivedNotificationHandler =
                delegate (AndroidNotificationIntentData data) {
                    //var msg = "Notification received : " + data.Id + "\n";
                    //msg += "\n Notification received: ";
                    //msg += "\n .Title: " + data.Notification.Title;
                    //msg += "\n .Body: " + data.Notification.Text;
                    //msg += "\n .Channel: " + data.Channel;
                    //Debug.Log(msg);
                    //Debug.Log("取消推播  腳色ID=" + data.Id + "的推播 時間: " + data.Notification.FireTime);
                    AndroidNotificationCenter.CancelNotification(data.Id);
                };
            AndroidNotificationCenter.OnNotificationReceived += receivedNotificationHandler;

        }
#elif UNITY_IOS
        void SendNotification_IOS(string _category, string _title, string _text, DateTime _time) {
            var calendarTrigger = new iOSNotificationCalendarTrigger() {
                Year = _time.Year,
                Month = _time.Month,
                Day = _time.Day,
                Hour = _time.Hour,
                Minute = _time.Minute,
                Second = _time.Second,
                Repeats = false
            };


            var notification = new iOSNotification() {
                Title = _title,
                Body = _text,
                Subtitle = "",
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Badge | PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = _category,
                ThreadIdentifier = _category + "Thread",
                Trigger = calendarTrigger,
            };
            iOSNotificationCenter.ScheduleNotification(notification);
        }

        public void ResetRoleCallNotification_IOS(int[] _roleIDs, DateTime[] _callTimes) {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            if (_roleIDs == null || _roleIDs.Length == 0) return;
            string categoryID = "RoleCall";
            for (int i = 0; i < _callTimes.Length; i++) {
                //超過時間的來電就不設定推播了，通常上線中的玩家不會收到推播，因為推播的時間設定會晚CallTime60秒，所以會每次呼叫此Functiong時，推播都會被清掉
                if (GameManager.Instance.NowTime > _callTimes[i]) continue;
                RoleData roleData = RoleData.GetData(_roleIDs[i]);
                DateTime time = _callTimes[i].AddHours(GameManager.Instance.LocoHourOffsetToServer).AddSeconds(60);//晚60秒，因為玩家在線收到來電要移除推播，晚點收到推播才來得及移除
                //WriteLog.Log("腳色ID=" + roleData.ID + "的推播 時間: " + time);
                string title = roleData.NotificationTitle;
                string content = roleData.NotificationContent;
                SendNotification_IOS(categoryID, title, content, time);
            }
        }

#endif




    }
}