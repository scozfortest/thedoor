
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Extensions;
using Scoz.Func;

namespace TheDoor.Main {
    public partial class FirebaseManager {


        public static void UploadFile(byte[] _bytes, string _cloudPath) {
            //DebugLogger.Log(_cloudPath);
            StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference;
            // Create a reference to the file you want to upload
            StorageReference cloudRef = storageRef.Child(_cloudPath);

            cloudRef.PutBytesAsync(_bytes)
                .ContinueWithOnMainThread((Task<StorageMetadata> task) => {
                    if (task.IsFaulted || task.IsCanceled) {
                        PopupUI.ShowClickCancel(StringData.GetUIString("ErrorNetworkError"), null);
                        DebugLogger.Log(task.Exception.ToString());
                        // Uh-oh, an error occurred!
                    } else {
                        DebugLogger.Log("UploadFile Success");
                        // Metadata contains file metadata such as size, content-type, and download URL.
                        //StorageMetadata metadata = task.Result;
                        //string md5Hash = metadata.Md5Hash;
                        //DebugLogger.Log("Finished uploading:" + _cloudPath);
                        //DebugLogger.Log("md5 hash = " + md5Hash);
                    }
                });
        }
        public static void DownloadFile(string _path, Action<byte[]> _cb, bool _showErrorl = true) {
            StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference;
            StorageReference cloudRef = storageRef.Child(_path);
            if (string.IsNullOrEmpty(_path)) {
                _cb?.Invoke(null);
                return;
            }
            const long maxAllowedSize = 5 * 1024 * 1024;
            try {
                cloudRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task => {
                    if (task.IsFaulted || task.IsCanceled) {
                        if (_showErrorl)
                            DebugLogger.LogWarningFormat("取不到Firebase資料:{0}", _path);
                        _cb?.Invoke(null);
                    } else {
                        byte[] fileContents = task.Result;
                        _cb?.Invoke(fileContents);
                    }
                });
            } catch (Exception _e) {
                _cb?.Invoke(null);
                DebugLogger.LogError(_e);
            }


        }
    }
}
