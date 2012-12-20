﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Actions;
using CameraControl.Actions.Enfuse;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Translation;
using CameraControl.windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for StartUpWindow.xaml
  /// </summary>
  public partial class StartUpWindow : Window
  {
    private IMainWindowPlugin _basemainwindow;
    public StartUpWindow()
    {
      InitializeComponent();
      string procName = Process.GetCurrentProcess().ProcessName;
      // get the list of all processes by that name

      Process[] processes = Process.GetProcessesByName(procName);

      if (processes.Length > 1)
      {
        MessageBox.Show("Application already running");
        Close();
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      InitApplication();
      //Thread thread = new Thread(InitApplication);
      //thread.SetApartmentState(ApartmentState.STA);
      //thread.Start();
    }

    private void InitApplication()
    {
      ServiceProvider.Configure();
      ServiceProvider.ActionManager.Actions = new AsyncObservableCollection<IMenuAction>
                                                {
                                                  new CmdFocusStackingCombineZP(),
                                                  new CmdEnfuse(),
                                                  new CmdToJpg(),
                                                  new CmdExpJpg()
                                                };
      ServiceProvider.Settings = new Settings();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      if (ServiceProvider.Settings.DisableNativeDrivers && MessageBox.Show(TranslationStrings.MsgDisabledDrivers, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        ServiceProvider.Settings.DisableNativeDrivers = false;
      ServiceProvider.Settings.LoadSessionData();
      TranslationManager.LoadLanguage(ServiceProvider.Settings.SelectedLanguage);

      ServiceProvider.WindowsManager = new WindowsManager();
      ServiceProvider.WindowsManager.Add(new FullScreenWnd());
      ServiceProvider.WindowsManager.Add(new LiveViewManager());
      ServiceProvider.WindowsManager.Add(new MultipleCameraWnd());
      ServiceProvider.WindowsManager.Add(new CameraPropertyWnd());
      ServiceProvider.WindowsManager.Add(new BrowseWnd());
      ServiceProvider.WindowsManager.Add(new TagSelectorWnd());
      ServiceProvider.WindowsManager.Add(new DownloadPhotosWnd());
      ServiceProvider.WindowsManager.Event += WindowsManager_Event;
      ServiceProvider.Trigger.Start();
      ServiceProvider.PluginManager.LoadPlugins(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins"));
      _basemainwindow = new MainWindow();
      ServiceProvider.PluginManager.MainWindowPlugins.Add(_basemainwindow);
      // event handlers
      ServiceProvider.Settings.SessionSelected += Settings_SessionSelected;
      ServiceProvider.DeviceManager.CameraConnected += DeviceManager_CameraConnected;
      ServiceProvider.DeviceManager.CameraSelected += DeviceManager_CameraSelected;
      //-------------------
      ServiceProvider.DeviceManager.DisableNativeDrivers = ServiceProvider.Settings.DisableNativeDrivers;
      ServiceProvider.DeviceManager.ConnectToCamera();
      Thread.Sleep(500);
      StartApplication();
      Dispatcher.Invoke(new Action(Hide));
    }

    private void StartApplication()
    {
      if (ServiceProvider.Settings.SelectedMainForm != _basemainwindow.DisplayName)
      {
        SelectorWnd wnd = new SelectorWnd();
        wnd.ShowDialog();
      }
      IMainWindowPlugin mainWindowPlugin = _basemainwindow;
      foreach (IMainWindowPlugin windowPlugin in ServiceProvider.PluginManager.MainWindowPlugins)
      {
        if (windowPlugin.DisplayName == ServiceProvider.Settings.SelectedMainForm)
          mainWindowPlugin = windowPlugin;
      }
      mainWindowPlugin.Show();
    }

    void WindowsManager_Event(string cmd, object o)
    {
      Log.Debug("Window command received :" + cmd);
      if (cmd == CmdConsts.All_Close)
      {
        ServiceProvider.DeviceManager.CloseAll();
        Thread.Sleep(1000);
        Application.Current.Shutdown();
      }
    }

    #region eventhandlers
    void Settings_SessionSelected(PhotoSession oldvalue, PhotoSession newvalue)
    {
      if (oldvalue != null)
        ServiceProvider.Settings.Save(oldvalue);
      ServiceProvider.QueueManager.Clear();
      if (ServiceProvider.DeviceManager.SelectedCameraDevice != null)
        ServiceProvider.DeviceManager.SelectedCameraDevice.AttachedPhotoSession = newvalue;
    }

    void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
    {
      if (newcameraDevice == null)
        return;
      CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(newcameraDevice);
      // load session data only if not session attached to the selected camera
      if (newcameraDevice.AttachedPhotoSession == null)
      {
        newcameraDevice.AttachedPhotoSession = ServiceProvider.Settings.GetSession(property.PhotoSessionName);
      }
      if (newcameraDevice.AttachedPhotoSession != null)
        ServiceProvider.Settings.DefaultSession = (PhotoSession)newcameraDevice.AttachedPhotoSession;

      if (newcameraDevice.GetCapability(CapabilityEnum.CaptureInRam))
        newcameraDevice.CaptureInSdRam = property.CaptureInSdRam;
    }

    void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
    {
      CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(cameraDevice);
      cameraDevice.DisplayName = property.DeviceName;
      cameraDevice.AttachedPhotoSession = ServiceProvider.Settings.GetSession(property.PhotoSessionName);
    }

    #endregion
  }
}
