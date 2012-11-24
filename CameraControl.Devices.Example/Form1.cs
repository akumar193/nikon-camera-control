﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Example
{
  public partial class Form1 : Form
  {

    public CameraDeviceManager DeviceManager { get; set; }
    public string FolderForPhotos { get; set; }

    public Form1()
    {
      DeviceManager = new CameraDeviceManager();
      DeviceManager.CameraSelected += DeviceManager_CameraSelected;
      DeviceManager.CameraConnected += DeviceManager_CameraConnected;
      DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
      FolderForPhotos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Test");
      InitializeComponent();
    }


    private void RefreshDisplay()
    {
      cmb_cameras.BeginUpdate();
      cmb_cameras.Items.Clear();
      foreach (ICameraDevice cameraDevice in DeviceManager.ConnectedDevices)
      {
        cmb_cameras.Items.Add(cameraDevice);
      }
      cmb_cameras.DisplayMember = "DeviceName";
      cmb_cameras.SelectedItem = DeviceManager.SelectedCameraDevice;
      cmb_cameras.EndUpdate();
    }

    private void PhotoCaptured(object o)
    {
      PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
      if (eventArgs == null)
        return;
      try
      {
        string fileName = Path.Combine(FolderForPhotos, eventArgs.FileName);
        // if file exist try to generate a new filename to prevent file lost. 
        // This useful when camera is set to record in ram the the all file names are same.
        if (File.Exists(fileName))
          fileName =
            StaticHelper.GetUniqueFilename(
              Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
              Path.GetExtension(fileName));
        // check the folder of filename, if not found create it
        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        }
        eventArgs.CameraDevice.TransferFile(eventArgs.EventArgs, fileName);
        // the IsBusy isn't used in code but can be useful to check if camera is in a capture process or not 
        eventArgs.CameraDevice.IsBusy = false;
        img_photo.ImageLocation = fileName;
      }
      catch (Exception exception)
      {
        eventArgs.CameraDevice.IsBusy = false;
        MessageBox.Show("Error download photo from camera :\n"+exception.Message);
      }
    }


    void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
    {
      // to prevent UI freeze start the transfer process in a new thread
      Thread thread = new Thread(PhotoCaptured);
      thread.Start(eventArgs);
    }

    void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
    {
      RefreshDisplay();
    }

    void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
    {
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      DeviceManager.ConnectToCamera();
      RefreshDisplay();
    }

    private void btn_capture_Click(object sender, EventArgs e)
    {
      DeviceManager.SelectedCameraDevice.CapturePhoto();
    }

    private void cmb_cameras_SelectedIndexChanged(object sender, EventArgs e)
    {
      DeviceManager.SelectedCameraDevice = (ICameraDevice)cmb_cameras.SelectedItem;
    }



  }
}
