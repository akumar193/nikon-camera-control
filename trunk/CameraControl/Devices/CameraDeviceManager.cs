﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using CameraControl.Devices.Others;
using PortableDeviceLib;
using WIA;

namespace CameraControl.Devices
{
  public class CameraDeviceManager : BaseFieldClass
  {
    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;
    private DeviceDescriptorEnumerator _deviceEnumerator;

    
    public Dictionary<string,Type> DeviceClass { get; set; }

    private  ICameraDevice _selectedCameraDevice;
    public  ICameraDevice SelectedCameraDevice
    {
      get { return _selectedCameraDevice; }
      set
      {
        _selectedCameraDevice = value;
        NotifyPropertyChanged("SelectedCameraDevice");
      }
    }

    public AsyncObservableCollection<ICameraDevice> ConnectedDevices { get; set; }


    public CameraDeviceManager()
    {
      DeviceClass = new Dictionary<string, Type>
                      {
                        {"D200", typeof (NikonD40)},
                        {"D3S", typeof (NikonD90)},
                        {"D3X", typeof (NikonD3X)},
                        {"D300", typeof (NikonD300)},
                        {"D300S", typeof (NikonD300)},
                        {"D3200", typeof (NikonD3200)},
                        {"D4", typeof (NikonD5100)},
                        {"D40", typeof (NikonD40)},
                        {"D5100", typeof (NikonD5100)},
                        {"D5000", typeof (NikonD90)},
                        {"D60", typeof (NikonD40)},
                        {"D70", typeof (NikonD40)},
                        {"D70s", typeof (NikonD40)},
                        {"D700", typeof (NikonD3X)},
                        {"D7000", typeof (NikonD5100)},
                        {"D80", typeof (NikonD80)},
                        {"D800", typeof (NikonD5100)},
                        {"D800E", typeof (NikonD5100)},
                        {"D90", typeof (NikonD90)},
                      };
      SelectedCameraDevice = new NotConnectedCameraDevice();
      ConnectedDevices = new AsyncObservableCollection<ICameraDevice>();
      _deviceEnumerator = new DeviceDescriptorEnumerator();
    }

    public ICameraDevice GetWiaIDevice(WIAManager manager, IDeviceInfo devInfo)
    {
      if (_deviceEnumerator.GetByWiaId(devInfo.DeviceID) != null)
        return _deviceEnumerator.GetByWiaId(devInfo.DeviceID).CameraDevice;
      _deviceEnumerator.RemoveDisconnected();
      DeviceDescriptor descriptor = new DeviceDescriptor { WiaDeviceInfo = devInfo, WiaId = devInfo.DeviceID };

      ICameraDevice cameraDevice = new WiaCameraDevice();
      cameraDevice.Init(descriptor);
      descriptor.CameraDevice = cameraDevice;
      _deviceEnumerator.Add(descriptor);
      ConnectedDevices.Add(cameraDevice);
      SelectedCameraDevice = cameraDevice;
      cameraDevice.PhotoCaptured += cameraDevice_PhotoCaptured;
      ServiceProvider.DeviceManager.SelectedCameraDevice.ReadDeviceProperties(0);
      ServiceProvider.Settings.SystemMessage = "New Camera is connected ! Driver :" + cameraDevice.DeviceName;
      ServiceProvider.Log.Debug("===========Camera is connected==============");
      ServiceProvider.Log.Debug("Driver :" + cameraDevice.GetType().Name);
      ServiceProvider.Log.Debug("Name :" + cameraDevice.DeviceName);
      ServiceProvider.Log.Debug("Manufacturer :" + cameraDevice.Manufacturer);

      return SelectedCameraDevice; 
    }

    [Obsolete]
    public ICameraDevice GetIDevice(WIAManager manager, IDeviceInfo devInfo)
    {
      // already the camera is connected
      if (_deviceEnumerator.GetByWiaId(devInfo.DeviceID) != null)
        return _deviceEnumerator.GetByWiaId(devInfo.DeviceID).CameraDevice;

      DeviceDescriptor descriptor = new DeviceDescriptor {WiaDeviceInfo = devInfo, WiaId = devInfo.DeviceID};

      ICameraDevice cameraDevice = new WiaCameraDevice();
      cameraDevice.Init(descriptor);

      if (!ServiceProvider.Settings.DisableNativeDrivers)
      {
        if (PortableDeviceCollection.Instance == null)
        {
          PortableDeviceCollection.CreateInstance(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
          PortableDeviceCollection.Instance.AutoConnectToPortableDevice = false;
        }

        foreach (var deviceId in PortableDeviceCollection.Instance.DeviceIds)
        {
          if (PhotoUtils.GetSerial(deviceId) == cameraDevice.SerialNumber &&
              DeviceClass.ContainsKey(cameraDevice.DeviceName.ToUpper())) 
          {
            descriptor.WpdId = deviceId;
            cameraDevice = (ICameraDevice) Activator.CreateInstance(DeviceClass[cameraDevice.DeviceName]);
            cameraDevice.SerialNumber = PhotoUtils.GetSerial(deviceId);
            cameraDevice.Init(descriptor);
            break;
          }
        }
      }

      descriptor.CameraDevice = cameraDevice;
      _deviceEnumerator.Add(descriptor);
      ConnectedDevices.Add(cameraDevice);
      SelectedCameraDevice = cameraDevice;
      cameraDevice.PhotoCaptured += cameraDevice_PhotoCaptured;
      ServiceProvider.DeviceManager.SelectedCameraDevice.ReadDeviceProperties(0);
      ServiceProvider.Settings.SystemMessage = "New Camera is connected ! Driver :" + cameraDevice.DeviceName;
      ServiceProvider.Log.Debug("===========Camera is connected==============");
      ServiceProvider.Log.Debug("Driver :" + cameraDevice.GetType().Name);
      ServiceProvider.Log.Debug("Name :" + cameraDevice.DeviceName);
      ServiceProvider.Log.Debug("Manufacturer :" + cameraDevice.Manufacturer);

      return SelectedCameraDevice;
    }

    void cameraDevice_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
    {
      if (PhotoCaptured != null)
        PhotoCaptured(sender, eventArgs);
    }

    public void ConnectDevices()
    {
      if (PortableDeviceCollection.Instance == null)
      {
        PortableDeviceCollection.CreateInstance(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
        PortableDeviceCollection.Instance.AutoConnectToPortableDevice = false;
      }
      _deviceEnumerator.RemoveDisconnected();

      foreach (PortableDevice portableDevice in PortableDeviceCollection.Instance.Devices)
      {
        if (!portableDevice.DeviceId.StartsWith("\\\\?\\usb"))
          continue;
        portableDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
        if(_deviceEnumerator.GetByWpdId(portableDevice.DeviceId)==null && DeviceClass.ContainsKey(portableDevice.Model))
        {
          ICameraDevice cameraDevice;
          DeviceDescriptor descriptor = new DeviceDescriptor {WpdId = portableDevice.DeviceId};
          cameraDevice = (ICameraDevice)Activator.CreateInstance(DeviceClass[portableDevice.Model]);
          cameraDevice.SerialNumber = PhotoUtils.GetSerial(portableDevice.DeviceId);
          cameraDevice.Init(descriptor);
          descriptor.CameraDevice = cameraDevice;
          _deviceEnumerator.Add(descriptor);
          ConnectedDevices.Add(cameraDevice);
          SelectedCameraDevice = cameraDevice;
          cameraDevice.PhotoCaptured += cameraDevice_PhotoCaptured;
          ServiceProvider.DeviceManager.SelectedCameraDevice.ReadDeviceProperties(0);
          ServiceProvider.Settings.SystemMessage = "New Camera is connected ! Driver :" + cameraDevice.DeviceName;
          ServiceProvider.Log.Debug("===========Camera is connected==============");
          ServiceProvider.Log.Debug("Driver :" + cameraDevice.GetType().Name);
          ServiceProvider.Log.Debug("Name :" + cameraDevice.DeviceName);
          ServiceProvider.Log.Debug("Manufacturer :" + cameraDevice.Manufacturer);
        }
      }
    }

    public void DisconnectCamera(string wiaId)
    {
      DeviceDescriptor descriptor = _deviceEnumerator.GetByWiaId(wiaId);
      if (descriptor != null)
      {
        descriptor.CameraDevice.PhotoCaptured -= cameraDevice_PhotoCaptured;
        ConnectedDevices.Remove(descriptor.CameraDevice);
        ServiceProvider.Settings.SystemMessage = "Camera disconnected :" + descriptor.CameraDevice.DeviceName;
        ServiceProvider.Log.Debug("===========Camera disconnected==============");
        ServiceProvider.Log.Debug("Name :" + descriptor.CameraDevice.DeviceName);
        if(SelectedCameraDevice==descriptor.CameraDevice)
        {
          if (ConnectedDevices.Count > 0)
            SelectedCameraDevice = ConnectedDevices[0];
          else
          {
            SelectedCameraDevice = new NotConnectedCameraDevice();
          }
        }
        descriptor.CameraDevice.Close();
        _deviceEnumerator.Remove(descriptor);
      }
    }

    public event PhotoCapturedEventHandler PhotoCaptured;
  }
}
