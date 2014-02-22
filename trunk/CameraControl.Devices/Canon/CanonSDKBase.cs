using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CameraControl.Devices.Classes;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Internal.SDK;
using PortableDeviceLib;
using PortableDeviceLib.Model;

namespace CameraControl.Devices.Canon
{
    public class CanonSDKBase : BaseMTPCamera
    {
        private EosLiveImageEventArgs _liveViewImageData = null;

        public EosCamera Camera = null;

        protected Dictionary<uint, string> _shutterTable = new Dictionary<uint, string>
                                                         {
                                                           {0x0C, "Bulb"},
                                                           {0x10, "30"},
                                                           {0x13, "25"},
                                                           {0x14, "20"},
                                                           {0x15, "20 (1/3)"},
                                                           {0x18, "15"},
                                                           {0x1B, "13"},
                                                           {0x1C, "10"},
                                                           {0x1D, "20  (1/3)"},
                                                           {0x20, "8"},
                                                           {0x23, "6 (1/3)"},
                                                           {0x24, "6"},
                                                           {0x25, "5"},
                                                           {0x28, "4"},
                                                           {0x2B, "3.2"},
                                                           {0x2C, "3"},
                                                           {0x2D, "2.5"},
                                                           {0x30, "2"},
                                                           {0x33, "1.6"},
                                                           {0x34, "1.5"},
                                                           {0x35, "1.3"},
                                                           {0x38, "1"},
                                                           {0x3B, "0.8"},
                                                           {0x3C, "0.7"},
                                                           {0x3D, "0.6"},
                                                           {0x40, "0.5"},
                                                           {0x43, "0.4"},
                                                           {0x44, "0.3"},
                                                           {0x45, "0.3 (1/3)"},
                                                           {0x48, "1/4"},
                                                           {0x4B, "1/5"},
                                                           {0x4C, "1/6"},
                                                           {0x4D, "1/56 (1/3)"},
                                                           {0x50, "1/8"},
                                                           {0x53, "1/10 (1/3)"},
                                                           {0x54, "1/10"},
                                                           {0x55, "1/13"},
                                                           {0x58 ,"1/15"},
                                                           {0x5B ,"1/20 (1/3)"},
                                                           {0x5C ,"1/20"},
                                                           {0x5D ,"1/25"},
                                                           {0x60 ,"1/30"},
                                                           {0x63 ,"1/40"},
                                                           {0x64 ,"1/45"},
                                                           {0x65 ,"1/50"},
                                                           {0x68 ,"1/60"},
                                                           {0x6B ,"1/80"},
                                                           {0x6C ,"1/90"},
                                                           {0x6D ,"1/100"},
                                                           {0x70 ,"1/125"},
                                                           {0x73 ,"1/160"},
                                                           {0x74 ,"1/180"},
                                                           {0x75 ,"1/200"},
                                                           {0x78 ,"1/250"},
                                                           {0x7B ,"1/320"},
                                                           {0x7C ,"1/350"},
                                                           {0x7D ,"1/400"},
                                                           {0x80 ,"1/500"},
                                                           {0x83 ,"1/640"},
                                                           {0x84 ,"1/750"},
                                                           {0x85 ,"1/800"},
                                                           {0x88 ,"1/1000"},
                                                           {0x8B ,"1/1250"},
                                                           {0x8C ,"1/1500"},
                                                           {0x8D ,"1/1600"},
                                                           {0x90 ,"1/2000"},
                                                           {0x93 ,"1/2500"},
                                                           {0x94 ,"1/3000"},
                                                           {0x95 ,"1/3200"},
                                                           {0x98 ,"1/4000"},
                                                           {0x9B ,"1/5000"},
                                                           {0x9C ,"1/6000"},
                                                           {0x9D ,"1/6400"},
                                                           {0xA0 ,"1/8000"},
                                                         };

        protected Dictionary<int, string> _apertureTable = new Dictionary<int, string>
                                                               {
                                                                   {0x08, "1.0"},
                                                                   {0x0B, "1.1"},
                                                                   {0x0C, "1.2"},
                                                                   {0x0D, "1.0 (1/3)"},
                                                                   {0x10, "1.4"},
                                                                   {0x13, "1.6"},
                                                                   {0x14, "1.8"},
                                                                   {0x15, "1.8 (1/3)"},
                                                                   {0x18, "2.0"},
                                                                   {0x1B, "2.2"},
                                                                   {0x1C, "2.5"},
                                                                   {0x1D, "2.5 (1/3)"},
                                                                   {0x20, "2.8"},
                                                                   {0x23, "3.2"},
                                                                   {0x24, "3.5"},
                                                                   {0x25, "3.5 (1/3)"},
                                                                   {0x28, "4.0"},
                                                                   {0x2B, "4.5"},
                                                                   {0x2C, "4.5"},
                                                                   {0x2D, "5.0"},
                                                                   {0x30, "5.6"},
                                                                   {0x33, "6.3"},
                                                                   {0x34, "6.7"},
                                                                   {0x35, "7.1"},
                                                                   {0x38, "8.0"},
                                                                   {0x3B, "9.0"},
                                                                   {0x3C, "9.5"},
                                                                   {0x3D, "10.0"},
                                                                   {0x40, "11.0"},
                                                                   {0x43, "13.0 (1/3)"},
                                                                   {0x44, "13.0"},
                                                                   {0x45, "14.0"},
                                                                   {0x48, "16.0"},
                                                                   {0x4B, "18.0"},
                                                                   {0x4C, "19.0"},
                                                                   {0x4D, "20.0"},
                                                                   {0x50, "22.0"},
                                                                   {0x53, "25.0"},
                                                                   {0x54, "27.0"},
                                                                   {0x55, "29.0"},
                                                                   {0x58, "32.0"},
                                                                   {0x5B, "36.0"},
                                                                   {0x5C, "38.0"},
                                                                   {0x5D, "40.0"},
                                                                   {0x60, "45.0"},
                                                                   {0x63, "51.0"},
                                                                   {0x64, "54.0"},
                                                                   {0x65, "57.0"},
                                                                   {0x68, "64.0"},
                                                                   {0x6B, "72.0"},
                                                                   {0x6C, "76.0"},
                                                                   {0x6D, "80.0"},
                                                                   {0x70, "91.0"},
                                                               };

        protected Dictionary<uint, string> _exposureModeTable = new Dictionary<uint, string>()
                            {
                              {0, "P"},
                              {1, "Tv"},
                              {2, "Av"},
                              {3, "M"},
                              {4, "Bulb"},
                              {5, "A-DEP"},
                              {6, "DEP"},
                              {7, "Camera settings registered"},
                              {8, "Lock"},
                              {9, "Auto"},
                              {10, "Night scene Portrait"},
                              {11, "Sport"},
                              {12, "Portrait"},
                              {13, "Landscape"},
                              {14, "Close-Up"},
                              {15, "Flash Off"},
                              {19, "Creative Auto"},
                              {21, "Photo in Movie"},
                            };

        protected Dictionary<uint, string> _isoTable = new Dictionary<uint, string>()
                                                  {
                                                    {0x00000028, "6"},
                                                    {0x00000030, "12"},
                                                    {0x00000038, "25"},
                                                    {0x00000040, "50"},
                                                    {0x00000048, "100"},
                                                    {0x0000004B, "125"},
                                                    {0x0000004D, "160"},
                                                    {0x00000050, "200"},
                                                    {0x00000053, "250"},
                                                    {0x00000055, "320"},
                                                    {0x00000058, "400"},
                                                    {0x0000005B, "500"},
                                                    {0x0000005D, "640"},
                                                    {0x00000060, "800"},
                                                    {0x00000063, "1000"},
                                                    {0x00000065, "1250"},
                                                    {0x00000068, "1600"},
                                                    {0x00000070, "3200"},
                                                    {0x00000078, "6400"},
                                                    {0x00000080, "12800"},
                                                    {0x00000088, "25600"},
                                                    {0x00000090, "51200"},
                                                    {0x00000098, "102400"},
                                                  };

        protected Dictionary<uint, string> _ec = new Dictionary<uint, string>()
                                                     {
                                                         {0x18,"+3.0"},
                                                         {0x15,"+2 2/3"},
                                                         {0x14,"+2.5"},
                                                         {0x13,"+2 1/3"},
                                                         {0x10,"+2.0"},
                                                         {0x0D,"+1 2/3"},
                                                         {0x0C,"+1.5"},
                                                         {0x0B,"+1 1/3"},
                                                         {0x08,"+1"},
                                                         {0x05,"+2/3"},
                                                         {0x04,"+0.5"},
                                                         {0x03,"+1/3"},
                                                         {0x00,"0.0"},
                                                         {0xFD,"-1/3"},
                                                         {0xFC,"-0.5"},
                                                         {0xFB,"-2/3"},
                                                         {0xF8,"-1"},
                                                         {0xF5,"-1 1/3"},
                                                         {0xF4,"-1.5"},
                                                         {0xF3,"-1 2/3"},
                                                         {0xF0,"-2"},
                                                         {0xED,"-2 1/3"},
                                                         {0xEC,"-2.5"},
                                                         {0xEB,"-3 2/3"},
                                                         {0xE8,"-3"},
                                                     };
        protected Dictionary<uint, string> _wbTable = new Dictionary<uint, string>()
                  {
                    {0, "Auto"},
                    {1, "Daylight"},
                    {2, "Cloudy"},
                    {3, "Tungsten"},
                    {4, "Fluorescent"},
                    {5, "Flash"},
                    {6, "Manual"},
                    {8, "Shade"},
                    {9, "Color temperature"},
                    {10, "Custom white balance: PC-1"},
                    {11, "Custom white balance: PC-2"},
                    {12, "Custom white balance: PC-3"},
                    {15, "Manual 2"},
                    {16, "Manual 3"},
                    {18, "Manual 4"},
                    {19, "Manual 5"},
                    {20, "Custom white balance: PC-4"},
                    {21, "Custom white balance: PC-5"},
                  };

        protected Dictionary<uint, string> _meteringTable = new Dictionary<uint, string>()
                                                                {
                                                                    {1,"Spot metering"},
                                                                    {2,"Evaluative metering"},
                                                                    {3,"Partial metering"},
                                                                    {4,"Spot metering"},
                                                                    {5,"Center-weighted averaging metering"},
                                                                    {0xFFFFFFFF,"Not valid/no settings changes"},
                                                                };
        protected Dictionary<uint, string> _focusModeTable = new Dictionary<uint, string>()
                                                                {
                                                                    {0,"One-Shot AF"},
                                                                    {1,"AI Servo AF"},
                                                                    {2,"AI Focus AF"},
                                                                    {3,"Manual Focus"},
                                                                    {0xFFFFFFFF,"Not valid/no settings changes"},
                                                                };
        public CanonSDKBase()
        {

        }

        public override bool CaptureInSdRam
        {
            get { return base.CaptureInSdRam; }
            set
            {
                base.CaptureInSdRam = value;
                try
                {
                    if (base.CaptureInSdRam)
                    {
                        Camera.SavePicturesToCamera();
                    }
                    else
                    {
                        Camera.SavePicturesToHost(Path.GetTempPath());
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Error set CaptureInSdram", exception);
                }
            }
        }

        private DateTime _dateTime;

        public override DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                NotifyPropertyChanged("DateTime");
            }
        }

        public bool Init(EosCamera camera)
        {
            try
            {
                IsBusy = true;
                Camera = camera;
                Camera.IsErrorTolerantMode = true;
                DeviceName = Camera.DeviceDescription;
                Manufacturer = "Canon Inc.";
                Camera.SetEventHandlers();
                Camera.Error += _camera_Error;
                Camera.Shutdown += _camera_Shutdown;
                Camera.LiveViewPaused += Camera_LiveViewPaused;
                Camera.LiveViewUpdate += Camera_LiveViewUpdate;
                Camera.PictureTaken += Camera_PictureTaken;
                Capabilities.Add(CapabilityEnum.Bulb);
                Capabilities.Add(CapabilityEnum.LiveView);
                Capabilities.Add(CapabilityEnum.CaptureInRam);
                IsConnected = true;
                LoadProperties();
                return true; 
            }
            catch (Exception exception)
            {
                Log.Error("Error initialize EOS camera object ", exception);
                return false;
            }
        }

        public void LoadProperties()
        {
            InitMode();
            InitShutterSpeed();
            InitFNumber();
            InitIso();
            InitEc();
            InitWb();
            InitMetering();
            InitFocus();
            InitOther();
            InitCompression();
            Battery = (int)Camera.BatteryLevel;
            IsBusy = false;
            Camera.PropertyChanged += Camera_PropertyChanged;
            CaptureInSdRam = true;
            SerialNumber = Camera.SerialNumber;
        }

        private void InitOther()
        {
            LiveViewImageZoomRatio = new PropertyValue<int> { Name = "LiveViewImageZoomRatio" };
            LiveViewImageZoomRatio.AddValues("All", 1);
            LiveViewImageZoomRatio.AddValues("5x", 5);
            LiveViewImageZoomRatio.AddValues("10x", 10);
            LiveViewImageZoomRatio.SetValue("All");
            LiveViewImageZoomRatio.ValueChanged += LiveViewImageZoomRatio_ValueChanged;
        }

        private void InitCompression()
        {
            CompressionSetting = new PropertyValue<int>();
            CompressionSetting.AddValues("Jpeg", (int)EosImageFormat.Jpeg);
            CompressionSetting.AddValues("Crw", (int) EosImageFormat.Crw);
            CompressionSetting.AddValues("Cr2", (int)EosImageFormat.Cr2);
            CompressionSetting.SetValue((int)Camera.ImageQuality.PrimaryImageFormat);
            CompressionSetting.ValueChanged += new PropertyValue<int>.ValueChangedEventHandler(CompressionSetting_ValueChanged);
        }

        void CompressionSetting_ValueChanged(object sender, string key, int val)
        {
            
        }


        void LiveViewImageZoomRatio_ValueChanged(object sender, string key, int val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_Evf_Zoom, val);
            }
            catch (Exception exception)
            {
                Log.Error("Error set property sP", exception);
            }
        }


        void Camera_PropertyChanged(object sender, EosPropertyEventArgs e)
        {
            try
            {
                //Log.Debug("Property changed " + e.PropertyId.ToString("X"));
                switch (e.PropertyId)
                {
                    case Edsdk.PropID_ExposureCompensation:
                        ExposureCompensation.SetValue((int)Camera.GetProperty(Edsdk.PropID_ExposureCompensation), false);
                        break;
                    case Edsdk.PropID_AEMode:
                        Mode.SetValue((uint)Camera.GetProperty(Edsdk.PropID_AEMode), false);
                        ReInitFNumber(true);
                        ReInitShutterSpeed();
                        break;
                    case Edsdk.PropID_WhiteBalance:
                        WhiteBalance.SetValue(Camera.GetProperty(Edsdk.PropID_WhiteBalance), false);
                        break;
                    case Edsdk.PropID_Tv:
                        ShutterSpeed.SetValue(Camera.GetProperty(Edsdk.PropID_Tv), false);
                        break;
                    case Edsdk.PropID_Av:
                        FNumber.SetValue((int) Camera.GetProperty(Edsdk.PropID_Av), false);
                        break;
                    case Edsdk.PropID_MeteringMode:
                        ExposureMeteringMode.SetValue((int)Camera.GetProperty(Edsdk.PropID_MeteringMode), false);
                        break;
                    case Edsdk.PropID_AFMode:
                        FocusMode.SetValue((int)Camera.GetProperty(Edsdk.PropID_AFMode), false);
                        break;
                    case Edsdk.PropID_ImageQuality:
                        CompressionSetting.SetValue((int)Camera.ImageQuality.PrimaryImageFormat);
                        break;
                    case Edsdk.PropID_BatteryLevel:
                        Battery = (int) Camera.BatteryLevel;
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error set property " + e.PropertyId.ToString("X"), exception);
            }
        }

        void Camera_PictureTaken(object sender, EosImageEventArgs e)
        {
            try
            {
                Log.Debug("Picture taken event received type" + e.GetType().ToString());
                PhotoCapturedEventArgs args = new PhotoCapturedEventArgs
                                                  {
                                                      WiaImageItem = null,
                                                      //EventArgs =
                                                      //  new PortableDeviceEventArgs(new PortableDeviceEventType()
                                                      //  {
                                                      //      ObjectHandle =
                                                      //        (uint)longeventParam
                                                      //  }),
                                                      CameraDevice = this,
                                                      FileName = "IMG0000.jpg",
                                                      Handle = e
                                                  };

                EosFileImageEventArgs file = e as EosFileImageEventArgs;
                if (file != null)
                {
                    args.FileName = Path.GetFileName(file.ImageFilePath);
                }
                EosMemoryImageEventArgs memory = e as EosMemoryImageEventArgs;
                if (memory != null)
                {
                    if (!string.IsNullOrEmpty(memory.FileName))
                        args.FileName = Path.GetFileName(memory.FileName);
                }
                OnPhotoCapture(this, args);
            }
            catch (Exception exception)
            {
                Log.Error("EOS Picture taken event error", exception);
            }

        }

        void Camera_LiveViewUpdate(object sender, EosLiveImageEventArgs e)
        {
            LiveViewData viewData = new LiveViewData();
            if (Monitor.TryEnter(Locker, 1))
            {
                try
                {
                    _liveViewImageData = e;
                }
                catch (Exception exception)
                {
                    Log.Error("Error get live view image event", exception);
                }
                finally
                {
                    Monitor.Exit(Locker);
                }
            }

        }

        void Camera_LiveViewPaused(object sender, EventArgs e)
        {
            try
            {
                Camera.ResetShutterButton();
                Camera.TakePictureNoAf();
                Camera.ResetShutterButton();
                //Camera.ResumeLiveview();
            }
            catch (Exception exception)
            {
                IsBusy = false;
                Log.Debug("Live view pause error", exception);
            }
        }

        void _camera_Shutdown(object sender, EventArgs e)
        {
            IsConnected = false;
            OnCameraDisconnected(this, new DisconnectCameraEventArgs { StillImageDevice = null,EosCamera = Camera});
        }

        public override void Close()
        {
            HaveLiveView = false;
            Camera.Error -= _camera_Error;
            Camera.Shutdown -= _camera_Shutdown;
            Camera.LiveViewPaused -= Camera_LiveViewPaused;
            Camera.LiveViewUpdate -= Camera_LiveViewUpdate;
            Camera.PictureTaken -= Camera_PictureTaken;
            Camera.PropertyChanged -= Camera_PropertyChanged;
            Camera.Dispose();
            Camera = null;
        }


        void _camera_Error(object sender, EosExceptionEventArgs e)
        {
            try
            {
                Log.Error("Canon error", e.Exception);
            }
            catch (Exception exception)
            {
                Log.Error("Error get camera error", exception);
            }
        }

        public override bool Init(DeviceDescriptor deviceDescriptor)
        {
            //StillImageDevice = new StillImageDevice(deviceDescriptor.WpdId);
            //StillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
            //StillImageDevice.DeviceEvent += _stillImageDevice_DeviceEvent;
            Capabilities.Add(CapabilityEnum.Bulb);
            Capabilities.Add(CapabilityEnum.LiveView);
            
            IsConnected = true;
            return true;
        }


        private void InitShutterSpeed()
        {
            ShutterSpeed = new PropertyValue<long>();
            ShutterSpeed.Name = "ShutterSpeed";
            ShutterSpeed.ValueChanged += ShutterSpeed_ValueChanged;
            ReInitShutterSpeed();
        }

        private void ReInitShutterSpeed()
        {
            lock (Locker)
            {
                try
                {
                    ShutterSpeed.Clear();
                    var data = Camera.GetPropertyDescription(Edsdk.PropID_Tv);
                    foreach (KeyValuePair<uint, string> keyValuePair in _shutterTable)
                    {
                        if (data.NumElements > 0)
                        {
                            if (ArrayContainValue(data.PropDesc, keyValuePair.Key))
                                ShutterSpeed.AddValues(keyValuePair.Value, keyValuePair.Key);
                        }
                        else
                        {
                            ShutterSpeed.AddValues(keyValuePair.Value, keyValuePair.Key);
                        }
                    }
                    
                    long value = Camera.GetProperty(Edsdk.PropID_Tv);
                    if (value == 0)
                    {
                        ShutterSpeed.IsEnabled = false;
                    }
                    else
                    {
                        ShutterSpeed.IsEnabled = true;
                        ShutterSpeed.SetValue(Camera.GetProperty(Edsdk.PropID_Tv), false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug("EOS Shutter speed init", ex);
                }
            }
        }

        void ShutterSpeed_ValueChanged(object sender, string key, long val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_Tv, val);
            }
            catch (Exception exception)
            {
                Log.Error("Error set property sP", exception);
            }
        }
#region F number
        private void InitFNumber()
        {
            FNumber = new PropertyValue<int> { IsEnabled = true, Name = "FNumber" };
            FNumber.ValueChanged += FNumber_ValueChanged;
            ReInitFNumber(true);
        }

        void FNumber_ValueChanged(object sender, string key, int val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_Av, val);
            }
            catch (Exception exception)
            {
                Log.Error("Error set property AV", exception);
            }
        }

        private void ReInitFNumber(bool trigervaluchange)
        {
            try
            {
                var data = Camera.GetPropertyDescription(Edsdk.PropID_Av);
                long value = Camera.GetProperty(Edsdk.PropID_Av);
                bool shouldinit = FNumber.Values.Count == 0;
                
                if (data.NumElements > 0)
                    FNumber.Clear();

                if (shouldinit && data.NumElements == 0)
                {
                    foreach (KeyValuePair<int, string> keyValuePair in _apertureTable)
                    {
                        FNumber.AddValues("�/" + keyValuePair.Value, keyValuePair.Key);
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, string> keyValuePair in _apertureTable.Where(keyValuePair => data.NumElements > 0).Where(keyValuePair => ArrayContainValue(data.PropDesc, keyValuePair.Key)))
                    {
                        FNumber.AddValues("�/" + keyValuePair.Value, keyValuePair.Key);
                    }
                }

                if(value==0)
                {
                    FNumber.IsEnabled = false;
                }
                else
                {
                    FNumber.SetValue((int)value, false);
                    FNumber.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error set aperture ", ex);
            }
        }
#endregion

        private void InitIso()
        {
            IsoNumber = new PropertyValue<int>();
            IsoNumber.ValueChanged += IsoNumber_ValueChanged;
            ReInitIso();
        }

        void IsoNumber_ValueChanged(object sender, string key, int val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_ISOSpeed, val);
            }
            catch (Exception exception)
            {
                Log.Debug("Error set ISO to camera", exception);
            }
        }

        private void ReInitIso()
        {
            try
            {
                var data = Camera.GetPropertyDescription(Edsdk.PropID_ISOSpeed);
                long value = Camera.GetProperty(Edsdk.PropID_ISOSpeed);
                bool shouldinit = IsoNumber.Values.Count == 0;

                if (data.NumElements > 0)
                    IsoNumber.Clear();

                if (shouldinit && data.NumElements == 0)
                {
                    foreach (KeyValuePair<uint, string> keyValuePair in _isoTable)
                    {
                        IsoNumber.AddValues(keyValuePair.Value, (int) keyValuePair.Key);
                    }
                }
                else
                {
                    foreach (KeyValuePair<uint, string> keyValuePair in _isoTable.Where(keyValuePair => data.NumElements > 0).Where(keyValuePair => ArrayContainValue(data.PropDesc, keyValuePair.Key)))
                    {
                        IsoNumber.AddValues(keyValuePair.Value, (int) keyValuePair.Key);
                    }
                }

                if (value == 0)
                {
                    IsoNumber.IsEnabled = false;
                }
                else
                {
                    IsoNumber.SetValue((int)value, false);
                    IsoNumber.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error set iso ", ex);
            }   
        }

        private void InitMode()
        {
            Mode = new PropertyValue<uint>();
            try
            {
                foreach (KeyValuePair<uint, string> keyValuePair in _exposureModeTable)
                {
                    Mode.AddValues(keyValuePair.Value, keyValuePair.Key);
                }

                Mode.SetValue((uint)Camera.GetProperty(Edsdk.PropID_AEMode), false);
                Mode.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Log.Debug("Error set aperture ", ex);
            }

        }

        private void InitEc()
        {
            ExposureCompensation = new PropertyValue<int>();
            ExposureCompensation.ValueChanged += ExposureCompensation_ValueChanged;
            try
            {
                foreach (KeyValuePair<uint, string> keyValuePair in _ec)
                {
                    ExposureCompensation.AddValues(keyValuePair.Value, (int) keyValuePair.Key);
                }
                ExposureCompensation.IsEnabled = true;
                ExposureCompensation.SetValue((int)Camera.GetProperty(Edsdk.PropID_ExposureCompensation), false);
            }
            catch (Exception exception)
            {
                Log.Debug("Error get EC", exception);
            }
        }

        private void InitWb()
        {
            WhiteBalance = new PropertyValue<long>();
            WhiteBalance.ValueChanged += WhiteBalance_ValueChanged;
            try
            {
                foreach (KeyValuePair<uint, string> keyValuePair in _wbTable)
                {
                    WhiteBalance.AddValues(keyValuePair.Value, (int)keyValuePair.Key);
                }
                WhiteBalance.IsEnabled = true;
                WhiteBalance.SetValue((long)Camera.GetProperty(Edsdk.PropID_WhiteBalance), false);
            }
            catch (Exception exception)
            {
                Log.Debug("Error get EC", exception);
            }
        }

        void WhiteBalance_ValueChanged(object sender, string key, long val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_WhiteBalance, val);
            }
            catch (Exception exception)
            {
                Log.Debug("Error set WB to camera", exception);
            }            
        }

        private void InitMetering()
        {
            ExposureMeteringMode = new PropertyValue<int>();
            ExposureMeteringMode.ValueChanged += ExposureMeteringMode_ValueChanged;
            try
            {
                foreach (KeyValuePair<uint, string> keyValuePair in _meteringTable)
                {
                    ExposureMeteringMode.AddValues(keyValuePair.Value, (int)keyValuePair.Key);
                }
                ExposureMeteringMode.IsEnabled = true;
                ExposureMeteringMode.SetValue((int) Camera.GetProperty(Edsdk.PropID_MeteringMode), false);
            }
            catch (Exception exception)
            {
                Log.Debug("Error get metering", exception);
            }
        }

        private void InitFocus()
        {
            FocusMode = new PropertyValue<long>();
            FocusMode.ValueChanged += FocusMode_ValueChanged;
            try
            {
                foreach (KeyValuePair<uint, string> keyValuePair in _focusModeTable)
                {
                    FocusMode.AddValues(keyValuePair.Value, (int)keyValuePair.Key);
                }
                FocusMode.IsEnabled = true;
                FocusMode.SetValue((int)Camera.GetProperty(Edsdk.PropID_AFMode), false);
            }
            catch (Exception exception)
            {
                Log.Debug("Error get focus mode", exception);
            }
        }

        void FocusMode_ValueChanged(object sender, string key, long val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_AFMode, val);
            }
            catch (Exception exception)
            {
                Log.Debug("Error set focus mode to camera", exception);
            }
        }

        void ExposureMeteringMode_ValueChanged(object sender, string key, int val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_MeteringMode, val);
            }
            catch (Exception exception)
            {
                Log.Debug("Error set metering mode to camera", exception);
            }
        }

        void ExposureCompensation_ValueChanged(object sender, string key, int val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_ExposureCompensation, val);
            }
            catch (Exception exception)
            {
                Log.Debug("Error set EC to camera", exception);
            }
        }

        public override void CapturePhoto()
        {
            Log.Debug("EOS capture start");
            Monitor.Enter(Locker);
            try
            {
                IsBusy = true;
                if (Camera.IsInHostLiveViewMode)
                {
                    Camera.TakePictureInLiveview();
                }
                else
                {
                    Camera.TakePicture();                    
                }

            }
            catch (COMException comException)
            {
                IsBusy = false;
                ErrorCodes.GetException(comException);
            }
            catch
            {
                IsBusy = false;
                throw;
            }
            finally
            {
                Monitor.Exit(Locker);
            }
            Log.Debug("EOS capture end");
        }

        public override void CapturePhotoNoAf()
        {
            Log.Debug("EOS capture start");
            Monitor.Enter(Locker);
            try
            {
                IsBusy = true;
                if (Camera.IsInHostLiveViewMode)
                {
                    Camera.TakePictureInLiveview();
                }
                else
                {
                    Camera.TakePicture();
                }
            }
            catch (COMException comException)
            {
                IsBusy = false;
                ErrorCodes.GetException(comException);
            }
            catch
            {
                IsBusy = false;
                throw;
            }
            finally
            {
                Monitor.Exit(Locker);
            }
            Log.Debug("EOS capture end");

        }

        public override void StartBulbMode()
        {
            Camera.BulbStart();
        }

        public override void EndBulbMode()
        {
            Camera.BulbEnd();
        }

        public override void Focus(int x, int y)
        {
            Camera.SetPropertyIntegerArrayData(Edsdk.PropID_Evf_ZoomPosition, new uint[] {(uint) x, (uint) y});
        }
        
        public override LiveViewData GetLiveViewImage()
        {
            LiveViewData viewData = new LiveViewData();
            if (Monitor.TryEnter(Locker, 1))
            {
                try
                {
                    //DeviceReady();
                    viewData.HaveFocusData = true;
                    viewData.ImageDataPosition = 0;
                    viewData.ImageData = _liveViewImageData.ImageData;
                    viewData.ImageHeight = _liveViewImageData.ImageSize.Height;
                    viewData.ImageWidth = _liveViewImageData.ImageSize.Width;
                    viewData.LiveViewImageHeight = 100;
                    viewData.LiveViewImageWidth = 100;
                    viewData.FocusX = _liveViewImageData.ZommBounds.X + (_liveViewImageData.ZommBounds.Width/2);
                    viewData.FocusY = _liveViewImageData.ZommBounds.Y + (_liveViewImageData.ZommBounds.Height/2);
                    viewData.FocusFrameXSize = _liveViewImageData.ZommBounds.Width;
                    viewData.FocusFrameYSize = _liveViewImageData.ZommBounds.Height;
                }
                catch (Exception)
                {
                    //Log.Error("Error get live view image ", e);
                }
                finally
                {
                    Monitor.Exit(Locker);
                }
            }
            return viewData;
        }

        public override void StartLiveView()
        {
            Camera.ResetShutterButton();
            //if (!Camera.IsInLiveViewMode) 
            Camera.StartLiveView(EosLiveViewAutoFocus.LiveMode);
        }

        public override void StopLiveView()
        {
            Camera.ResetShutterButton();
            //if (Camera.IsInLiveViewMode)
                Camera.StopLiveView();
        }

        public override void AutoFocus()
        {
            Camera.ResetShutterButton();
            Camera.AutoFocus();
        }

        public override void Focus(int step)
        {
            Camera.ResetShutterButton();
            if(step<0)
            {
                step = -step;
                if (step < 50)
                    Camera.FocusInLiveView(Edsdk.EvfDriveLens_Near1);
                else if (step >= 50 && step< 200)
                    Camera.FocusInLiveView(Edsdk.EvfDriveLens_Near2);
                else
                    Camera.FocusInLiveView(Edsdk.EvfDriveLens_Near3);
            }
            else
            {
                if (step < 50)
                    Camera.FocusInLiveView(Edsdk.EvfDriveLens_Far1);
                else if (step >= 50 && step < 200)
                    Camera.FocusInLiveView(Edsdk.EvfDriveLens_Far2);
                else
                    Camera.FocusInLiveView(Edsdk.EvfDriveLens_Far3);
            }
        }

        public override void TransferFile(object o, string filename)
        {
            EosFileImageEventArgs file = o as EosFileImageEventArgs;
            if (file != null)
            {
                Log.Debug("File transfer started");
                try
                {
                    if(File.Exists(file.ImageFilePath))
                    {
                        File.Copy(file.ImageFilePath, filename, true);
                        File.Delete(file.ImageFilePath);
                    }
                    else
                    {
                        Log.Error("Base file not found " + file.ImageFilePath);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Transfer error ", exception);
                }
            }
            EosMemoryImageEventArgs memory = o as EosMemoryImageEventArgs;
            if(memory!=null)
            {
                Log.Debug("Memory file transfer started");
                try
                {
                    using (FileStream fileStream = File.Create(filename, (int)memory.ImageData.Length))
                    {
                        fileStream.Write(memory.ImageData, 0, memory.ImageData.Length);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Error transfer memory file", exception);
                }
            }
        }

        public override string GetProhibitionCondition(OperationEnum operationEnum)
        {
            return "";
        }

        private bool ArrayContainValue(IEnumerable<int> data, uint value)
        {
            return ArrayContainValue(data, (int) value);
        }

        private bool ArrayContainValue(IEnumerable<int> data, int value)
        {
            return data.Any(i => i == (int)value);
        }
    }
}
