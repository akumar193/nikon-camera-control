﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;

namespace CameraControl.windows
{
    public class LiveViewManager : IWindow
    {
        private static object _locker = new object();

        #region Implementation of IWindow

        private Dictionary<object, LiveViewWnd> _register;
        private static Dictionary<ICameraDevice, bool> _recordtoRam;
        private static Dictionary<ICameraDevice, bool> _hostMode;
        private static Dictionary<ICameraDevice, CameraPreset> _presets;

        public LiveViewManager()
        {
            _register = new Dictionary<object, LiveViewWnd>();
            _recordtoRam = new Dictionary<ICameraDevice, bool>();
            _hostMode = new Dictionary<ICameraDevice, bool>();
            _presets = new Dictionary<ICameraDevice, CameraPreset>();
        }

        public void ExecuteCommand(string cmd, object param)
        {
            if (param == null)
                param = ServiceProvider.DeviceManager.SelectedCameraDevice;
            lock (_locker)
            {

                switch (cmd)
                {
                    case WindowsCmdConsts.LiveViewWnd_Show:
                        {
                            if (!_register.ContainsKey(param))
                            {
                                Application.Current.Dispatcher.Invoke(new Action(delegate
                                                                                     {
                                                                                         LiveViewWnd wnd =
                                                                                             new LiveViewWnd();
                                                                                         ServiceProvider.Settings.
                                                                                             ApplyTheme
                                                                                             (wnd);
                                                                                         _register.Add(param, wnd);
                                                                                     }));
                            }
                            NikonBase nikonBase = param as NikonBase;
                            if (nikonBase != null && ServiceProvider.Settings.EasyLiveViewControl)
                            {
                                CameraPreset preset = new CameraPreset();
                                preset.Get(nikonBase);
                                if (!_presets.ContainsKey(nikonBase))
                                    _presets.Add(nikonBase, preset);
                                else
                                    _presets[nikonBase] = preset;
                                if (nikonBase.ShutterSpeed.Value == "Bulb")
                                {
                                    nikonBase.ShutterSpeed.Value =
                                        nikonBase.ShutterSpeed.Values[nikonBase.ShutterSpeed.Values.Count/2];
                                }
                                nikonBase.FocusMode.Value = nikonBase.FocusMode.Values[0];
                                nikonBase.FNumber.Value = nikonBase.FNumber.Values[0];
                            }
                            _register[param].ExecuteCommand(cmd, param);
                        }
                        break;
                    case WindowsCmdConsts.LiveViewWnd_Hide:
                        {
                            if (_register.ContainsKey(param))
                                _register[param].ExecuteCommand(cmd, param);
                            var nikonBase = param as NikonBase;
                            if (ServiceProvider.Settings.EasyLiveViewControl)
                            {
                                if (nikonBase != null && _presets.ContainsKey(nikonBase))
                                {
                                    nikonBase.ShutterSpeed.Value = _presets[nikonBase].GetValue("ShutterSpeed");
                                    nikonBase.FNumber.Value = _presets[nikonBase].GetValue("FNumber");
                                    nikonBase.FocusMode.Value = _presets[nikonBase].GetValue("FocusMode");
                                }
                            }
                        }
                        break;
                    case CmdConsts.All_Close:
                        foreach (var liveViewWnd in _register)
                        {
                            liveViewWnd.Value.ExecuteCommand(cmd, param);
                        }
                        break;
                    default:
                        foreach (var liveViewWnd in _register)
                        {
                            if (cmd.StartsWith("LiveView"))
                                liveViewWnd.Value.ExecuteCommand(cmd, param);
                        }
                        break;
                }
            }
        }

        public bool IsVisible { get; private set; }

        #endregion

        public static void StartLiveView(ICameraDevice device)
        {
            // some nikon cameras can set af to manual
            //force to capture in ram
            if (device is NikonBase)
            {
                if (!_recordtoRam.ContainsKey(device))
                    _recordtoRam.Add(device, device.CaptureInSdRam);
                else
                    _recordtoRam[device] = device.CaptureInSdRam;
                device.CaptureInSdRam = true;
                if (!_hostMode.ContainsKey(device))
                    _hostMode.Add(device, device.HostMode);
                else
                    _hostMode[device] = device.HostMode;
                device.HostMode = true;
            }
            device.StartLiveView();
        }

        public static void StopLiveView(ICameraDevice device)
        {
            device.StopLiveView();
            if (device is NikonBase)
            {
                if (_recordtoRam.ContainsKey(device))
                    device.CaptureInSdRam = _recordtoRam[device];
                if (_hostMode.ContainsKey(device))
                    device.HostMode = _hostMode[device];
            }
        }

        public static LiveViewData GetLiveViewImage(ICameraDevice device)
        {
            return device.GetLiveViewImage();
        }

    }
}
