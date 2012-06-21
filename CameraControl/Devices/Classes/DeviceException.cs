﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace CameraControl.Devices.Classes
{
  [Serializable]
  public class DeviceException : Exception
  {
    public uint ErrorCode { get; set; }

    public DeviceException(string message)
      : base(message)
    {
    }

    public DeviceException(string format, params object[] args)
      : base(string.Format(format, args))
    {
    }

    public DeviceException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public DeviceException(string message, Exception innerException, uint code)
      : base(message, innerException)
    {
      ErrorCode = code;
    }


    public DeviceException(string format, Exception innerException, params object[] args)
      : base(string.Format(format, args), innerException)
    {
    }

    protected DeviceException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public DeviceException()
      : base()
    {
    }
  }
}
