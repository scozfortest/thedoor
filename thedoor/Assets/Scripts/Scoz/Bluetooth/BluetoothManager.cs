#if UNITY_EDITOR || UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;

namespace Scoz.Func {
    public class BluetoothManager {

        public static BluetoothHelper BTHelper;
        static Action<BluetoothHelper> OnConnectedAC;
        static Action<BluetoothHelper> OnConnectionFailedAC;
        static Action<BluetoothHelper> OnDataReceivedAC;
        static Action<LinkedList<BluetoothDevice>> OnScanEndedAC;
        /// <summary>
        /// 若有傳入OnScanEndedAC，則搜尋完裝置後會回傳裝置一覽
        /// </summary>
        public static LinkedList<BluetoothDevice> Devices;
        public static bool IsConnected {
            get {
                if (BTHelper == null || !BTHelper.isConnected())
                    return false;
                return true;
            }
        }
        public static string DeviceName {
            get {
                if (BTHelper == null || !BTHelper.isConnected())
                    return "尚未連上藍芽裝置";
                return BTHelper.getDeviceName();
            }
        }
        public static string State {
            get {
                if (BTHelper == null || !BTHelper.isConnected())
                    return "尚未連上藍芽裝置";
                return string.Format("連線裝置中: {0}", BTHelper.getDeviceName());
            }
        }

        /// <summary>
        /// 初始化時傳入裝置名與Callback函式
        /// </summary>
        public static void Init(string _deviceName, int _dataLength, Action<BluetoothHelper> _onConnected, Action<BluetoothHelper> _onConnectionFailed,
            Action<BluetoothHelper> _onDataReceived) {
            DebugLogger.LogFormat("初始化BluetoothManager 目標裝置: {0}", _deviceName);
            //初始化BluetoothHelper
            BluetoothHelper.BLE = false;
            BTHelper = BluetoothHelper.GetInstance();
            BTHelper.OnConnected += OnConnected;
            BTHelper.OnConnectionFailed += OnConnectionFailed;
            BTHelper.OnDataReceived += OnDataReceived;
            BTHelper.OnScanEnded += OnScanEnded;
            BTHelper.setFixedLengthBasedStream(_dataLength); //data is received byte by byte
            BTHelper.setDeviceName(_deviceName);
            //設定Callback
            OnConnectedAC = _onConnected;
            OnConnectionFailedAC = _onConnectionFailed;
            OnDataReceivedAC = _onDataReceived;
        }
        static void OnConnected(BluetoothHelper _helper) {
            _helper.StartListening();
            DebugLogger.LogFormat("已連接至藍芽裝置: {0} 並開始偵聽", _helper.getDeviceName());
            OnConnectedAC?.Invoke(_helper);
        }

        static void OnConnectionFailed(BluetoothHelper _helper) {
            DebugLogger.LogFormat("連接藍芽裝置: {0} 失敗", _helper.getDeviceName());
            OnConnectionFailedAC?.Invoke(_helper);
        }

        static void OnDataReceived(BluetoothHelper _helper) {
            OnDataReceivedAC?.Invoke(_helper);
        }
        static void OnScanEnded(BluetoothHelper _helper, LinkedList<BluetoothDevice> _devices) {
            Devices = _devices;
            OnScanEndedAC?.Invoke(_devices);
        }
        /// <summary>
        /// 嘗試搜尋附近的藍芽裝置
        /// </summary>
        public static void ScanNearbyDevices(Action<LinkedList<BluetoothDevice>> _cb) {
            OnScanEndedAC = null;
            if (BTHelper == null) {
                DebugLogger.LogFormat("尚未初始化藍芽裝置");
                return;
            }
            DebugLogger.LogFormat("嘗試搜尋附近的藍芽裝置");
            bool scanning = BTHelper.ScanNearbyDevices();
            if (scanning) {
                OnScanEndedAC = _cb;
                DebugLogger.LogFormat("開始搜尋");
            } else
                DebugLogger.LogFormat("搜尋失敗");
        }
        /// <summary>
        /// 連線到目標藍芽裝置
        /// </summary>
        public static void Connect() {
            Debug.Log("Connect");
            if (BTHelper == null) {
                DebugLogger.LogFormat("尚未初始化藍芽裝置");
                return;
            }
            DebugLogger.LogFormat("嘗試連接至藍芽裝置: {0}", BTHelper.getDeviceName());
            BTHelper.Connect();
        }
        /// <summary>
        /// 中斷目標藍芽裝置
        /// </summary>
        public static void Disconnect() {
            if (BTHelper == null) {
                DebugLogger.LogFormat("尚未初始化藍芽裝置");
                return;
            }
            DebugLogger.LogFormat("中斷連接藍芽裝置: {0}", BTHelper.getDeviceName());
            BTHelper.Disconnect();
        }
        /// <summary>
        /// 送字串資料到裝置
        /// </summary>
        public static void SendData(string _str) {
            if (BTHelper == null) {
                DebugLogger.LogFormat("尚未初始化藍芽裝置");
                return;
            }
            DebugLogger.LogFormat("送字串到裝置: " + _str);
            BTHelper.SendData(_str);
        }
        /// <summary>
        /// 送Bytes資料到裝置
        /// </summary>
        public static void SendData(byte[] _bytes) {
            if (BTHelper == null) {
                DebugLogger.LogFormat("尚未初始化藍芽裝置");
                return;
            }
            if (_bytes == null || _bytes.Length == 0) return;
            BTHelper.SendData(_bytes);
            string log = TextManager.ByesToHexStr(_bytes);
            DebugLogger.LogFormat("送Bytes到裝置: " + log);

        }
    }
}
#endif