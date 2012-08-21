﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices;

namespace CameraControl.Classes
{
  public class CameraPropertyEnumerator
  {
    public AsyncObservableCollection<CameraProperty> Items { get; set; }

    public CameraPropertyEnumerator()
    {
      Items = new AsyncObservableCollection<CameraProperty>();
    }

    public CameraProperty Get(ICameraDevice device)
    {
      foreach (CameraProperty cameraProperty in Items)
      {
        if (cameraProperty.SerialNumber == device.SerialNumber)
          return cameraProperty;
      }
      var c = new CameraProperty() {SerialNumber = device.SerialNumber, DeviceName = device.DisplayName};
      Items.Add(c);
      return c;
    }

  }
}
