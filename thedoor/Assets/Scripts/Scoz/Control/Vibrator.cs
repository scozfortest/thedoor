using UnityEngine;
using System.Collections;
using distriqt.plugins.vibration;
using TheDoor.Main;

namespace Scoz.Func {
    public static class Vibrator {

#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;
#endif

        public static void Vibrate() {
            if (isAndroid())
                vibrator.Call("vibrate");
            else
                Handheld.Vibrate();
        }

        static FeedbackGenerator NotificationGenerator;
        static FeedbackGenerator SelectGenerator;
        public static void Vibrate(long milliseconds) {
            if (!GamePlayer.Instance.Vibration) return;//關閉震動就return
            if (isAndroid())
                vibrator.Call("vibrate", milliseconds);
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                if (milliseconds < 100) {//震動小於100毫秒用SELECTION
                    if (SelectGenerator == null) {
                        SelectGenerator = Vibration.Instance.CreateFeedbackGenerator(FeedbackGeneratorType.SELECTION);
                    }
                    SelectGenerator.PerformFeedback();
                } else if (milliseconds < 400) {//震動小於400毫秒用NOTIFICATION
                    if (NotificationGenerator == null) {
                        NotificationGenerator = Vibration.Instance.CreateFeedbackGenerator(FeedbackGeneratorType.NOTIFICATION);
                    }
                    NotificationGenerator.PerformFeedback();
                } else {//震動大於400毫秒就用震動
                    Handheld.Vibrate();
                }
            }

        }

        public static void Vibrate(long[] pattern, int repeat) {
            if (isAndroid())
                vibrator.Call("vibrate", pattern, repeat);
            else
                Handheld.Vibrate();
        }

        public static bool HasVibrator() {
            return isAndroid();
        }

        public static void Cancel() {
            if (isAndroid())
                vibrator.Call("cancel");
        }

        private static bool isAndroid() {
#if UNITY_ANDROID && !UNITY_EDITOR
	return true;
#else
            return false;
#endif
        }
    }
}