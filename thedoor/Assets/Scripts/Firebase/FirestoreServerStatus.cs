using Firebase.Extensions;
using Firebase.Firestore;
using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheDoor.Main
{
    public partial class FirebaseManager
    {
        public static void GetServerStatus(Action<Dictionary<string, object>> _cb)
        {
            DocumentReference docRef = Store.Collection(ColNames.GetValueOrDefault(ColEnum.GameSetting)).Document(GameDataDocEnum.Version.ToString());
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    PopupUI.ShowClickCancel(StringData.GetUIString("ErrorNetworkError"), null);
                    DebugLogger.LogError("Error:" + task.Exception.ToString());
                    _cb?.Invoke(null);
                    return;
                }
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> data = snapshot.ToDictionary();
                    _cb?.Invoke(snapshot.ToDictionary());
                }
                else
                {
                    _cb?.Invoke(null);
                }
            });
        }
    }
}