﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using CameraControl.Classes;
using CameraControl.windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using WIA;
using WPF.Themes;
using EditSession = CameraControl.windows.EditSession;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    public PropertyWnd PropertyWnd { get; set; }

    public WIAManager WiaManager { get; set; }

    public MainWindow()
    {
      ServiceProvider.Configure();
      ServiceProvider.Settings = new Settings();
      ServiceProvider.ThumbWorker = new ThumbWorker();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      ServiceProvider.Settings.PropertyChanged += Settings_PropertyChanged;
      ServiceProvider.Settings.LoadSessionData();
      WiaManager = new WIAManager();
      ServiceProvider.Settings.Manager = WiaManager;
      WiaManager.PhotoTaked += WiaManager_PhotoTaked;
      InitializeComponent();
      DataContext = ServiceProvider.Settings;
      if (ServiceProvider.Settings.DefaultSession.Files.Count > 0)
        ImageLIst.SelectedIndex = 0;
      if ((DateTime.Now - ServiceProvider.Settings.LastUpdateCheckDate).TotalDays > 7)
      {
        ServiceProvider.Settings.LastUpdateCheckDate = DateTime.Now;
        ServiceProvider.Settings.Save();
        CheckForUpdate();
      }
    }

    private void CheckForUpdate()
    {
      try
      {
        string tempfile = System.IO.Path.GetTempFileName();
        using (WebClient client = new WebClient())
        {
          client.DownloadFile("http://nikon-camera-control.googlecode.com/svn/trunk/versioninfo.xml", tempfile);
        }
       
        XmlDocument document=new XmlDocument();
        document.Load(tempfile);
        string ver=document.SelectSingleNode("application/version").InnerText;
        Version v_ver=new Version(ver);
        Version app_ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        if(v_ver>System.Reflection.Assembly.GetExecutingAssembly().GetName().Version)
        {
          if(MessageBox.Show("New version of application released\nDo you want to download?","Update",MessageBoxButtons.YesNo)==System.Windows.Forms.DialogResult.Yes)
          {
            System.Diagnostics.Process.Start("http://code.google.com/p/nikon-camera-control/downloads/list");
            Close();
          }
        }
        File.Delete(tempfile);
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Error download update information",exception);
      }
    }

    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "DefaultSession")
      {
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += delegate
                                     {
                                       try
                                       {
                                         //ServiceProvider.ThumbWorker.Start();
                                         foreach (FileItem fileItem in ServiceProvider.Settings.DefaultSession.Files)
                                         {
                                           if (fileItem.Thumbnail == null)
                                             fileItem.GetExtendedThumb();
                                         }
                                       }
                                       catch (Exception)
                                       {

                                       }
                                     };
        backgroundWorker.RunWorkerAsync();
      }
    }

    private void WiaManager_PhotoTaked(Item item)
    {
      try
      {
        ServiceProvider.Settings.SystemMessage = "Photo transfer begin.";
        string s = item.ItemID;
        ImageFile imageFile = (ImageFile) item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
        string fileName = ServiceProvider.Settings.DefaultSession.GetNextFileName(imageFile.FileExtension);
        //file exist : : 0x80070050
        imageFile.SaveFile(fileName);

        ImageLIst.SelectedValue = ServiceProvider.Settings.DefaultSession.AddFile(fileName);
        ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
        ServiceProvider.Settings.SystemMessage = "Photo transfer done.";
      }
      catch (Exception ex)
      {
        ServiceProvider.Settings.SystemMessage = "Transfer error !\nMessage :" + ex.Message;
        ServiceProvider.Log.Error("Transfer error !",ex);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      WiaManager.ConnectToCamera();
      //SessionPanel.DataContext = ServiceProvider.Settings;
      ImagePanel.DataContext = ServiceProvider.Settings;
    }

    //private void button1_Click(object sender, RoutedEventArgs e)
    //{
    //  if (WiaManager.ConnectToCamera())
    //  {
    //    //listBox1.Items.Clear();
    //    foreach (Property property in WiaManager.Device.Properties)
    //    {
    //      textBox1.Text += property.Name + "|" + property.get_Value().ToString() + "|" + property.IsReadOnly.ToString() +
    //                       property.IsVector.ToString() + "|" + property.SubType.ToString() + "|";
    //      try
    //      {
    //        textBox1.Text += property.SubTypeMax.ToString();
    //        //  + property.SubTypeMin.ToString() + "|" +
    //        //property.SubTypeStep.ToString() + "|" + property.SubTypeValues.ToString() +
    //        //property.Type.ToString() + "|";
    //      }
    //      catch (Exception)
    //      {


    //      }
    //      textBox1.Text += "\n";
    //    }
    //  }
    //}

    private void button3_Click(object sender, RoutedEventArgs e)
    {

      if (!ServiceProvider.Settings.DefaultSession.TimeLapse.IsDisabled)
      {
        if (
          MessageBox.Show("A time lapse photo session runnig !\n Do you want to stop it  ?",
                          "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
        {
          ServiceProvider.Settings.DefaultSession.TimeLapse.Stop();
          return;
        }
      }

      try
      {
        ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
      }
        catch(COMException comException)
        {
          if(comException.ErrorCode==-2147467259)
          {
            ServiceProvider.Settings.SystemMessage = "Unable to take photo. Unable to focus !";
          }
          else
          {
            ServiceProvider.Log.Error("Take photo", comException);
          }
        }
      catch (Exception exception)
      {
        MessageBox.Show("No picture was taken !\n" + exception.Message);
        ServiceProvider.Log.Error("Take photo", exception);
      }

    }

    private void btn_edit_Sesion_Click(object sender, RoutedEventArgs e)
    {
      if (File.Exists(ServiceProvider.Settings.DefaultSession.ConfigFile))
      {
        File.Delete(ServiceProvider.Settings.DefaultSession.ConfigFile);
      }
      EditSession editSession = new EditSession(ServiceProvider.Settings.DefaultSession);
      editSession.ShowDialog();
      ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
    }

    private void btn_add_Sesion_Click(object sender, RoutedEventArgs e)
    {
      EditSession editSession = new EditSession(new PhotoSession());
      if (editSession.ShowDialog() == true)
      {
        ServiceProvider.Settings.Add(editSession.Session);
        ServiceProvider.Settings.DefaultSession = editSession.Session;
      }
    }

    private void ImageLIst_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

      if (e.AddedItems.Count > 0)
      {
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += worker_DoWork;
        FileItem item = e.AddedItems[0] as FileItem;
        if (item != null)
        {
          //image1.Source = item.Thumbnail;
          ServiceProvider.Settings.SelectedBitmap.SetFileItem(item);
          worker.RunWorkerAsync(item);
        }
      }
    }

    private void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      Thread.Sleep(200);
      FileItem fileItem = e.Argument as FileItem;
      if (ServiceProvider.Settings.SelectedBitmap.FileItem.FileName != fileItem.FileName)
        return;
      ServiceProvider.Settings.ImageLoading = Visibility.Visible;
      ServiceProvider.Settings.SelectedBitmap.GetBitmap();
      ServiceProvider.Settings.ImageLoading = Visibility.Hidden;
      //logo.BeginInit();
      //logo.UriSource = new Uri(fileItem.FileName);
      //logo.EndInit();
      //logo.Freeze();

      //Call the UI thread using the Dispatcher to update the Image control
      //Dispatcher.BeginInvoke(new ThreadStart(delegate
      //                                         {
      //                                           image1.Source = logo;
      //                                         }));

    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      if (button1.IsChecked == true)
      {
        if (PropertyWnd == null)
        {
          PropertyWnd = new PropertyWnd();
        }
        PropertyWnd.IsVisibleChanged -= PropertyWnd_IsVisibleChanged;
        PropertyWnd.Show();
        PropertyWnd.IsVisibleChanged += PropertyWnd_IsVisibleChanged;
      }
      else
      {
        if (PropertyWnd != null && PropertyWnd.Visibility == Visibility.Visible)
        {
          PropertyWnd.IsVisibleChanged -= PropertyWnd_IsVisibleChanged;
          PropertyWnd.Hide();
        }
      }
    }

    private void PropertyWnd_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      button1.IsChecked = !button1.IsChecked;
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = false;
      SettingsWnd wnd = new SettingsWnd();
      wnd.ShowDialog();
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      if (PropertyWnd != null)
      {
        PropertyWnd.Hide();
        PropertyWnd.Close();
      }
      WiaManager.DisconnectCamera();
    }

    private void but_timelapse_Click(object sender, RoutedEventArgs e)
    {
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = false;
      TimeLapseWnd wnd = new TimeLapseWnd();
      wnd.ShowDialog();
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = true;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if (!ServiceProvider.Settings.DefaultSession.TimeLapse.IsDisabled)
      {
        if (
          MessageBox.Show("A time lapse photo session runnig !\n Do you want to stop it and exit from application ?",
                          "Closing", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
        {
          e.Cancel = true;
        }
        else
        {
          ServiceProvider.Settings.DefaultSession.TimeLapse.Stop();
        }
      }
    }

    private void but_fullscreen_Click(object sender, RoutedEventArgs e)
    {
      ShowFullScreen();
    }

    private void ShowFullScreen()
    {
      FullScreenWnd wnd = new FullScreenWnd();
      wnd.KeyDown += wnd_KeyDown;
      wnd.KeyUp += wnd_KeyUp;
      wnd.Show();
      wnd.Activate();
      wnd.Topmost = true;  
      wnd.Topmost = false; 
      wnd.Focus();        
    }

    void wnd_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      ImageLIst.RaiseEvent(e);
    }

    void wnd_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Right && ImageLIst.SelectedIndex < ImageLIst.Items.Count - 1)
      {
        ImageLIst.SelectedIndex++;
      }
      if (e.Key == Key.Left && ImageLIst.SelectedIndex >0)
      {
        ImageLIst.SelectedIndex--;
      }
    }

    private void image1_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
        ShowFullScreen();
    }

    private void btn_liveview_Click(object sender, RoutedEventArgs e)
    {
      if (WiaManager.CameraDevice == null)
        return;
      LiveViewWnd wnd = new LiveViewWnd(WiaManager.CameraDevice);
      wnd.ShowDialog();
    }

    private void btn_about_Click(object sender, RoutedEventArgs e)
    {
      AboutWnd wnd=new AboutWnd();
      wnd.ShowDialog();
    }

    private void mnu_delete_file_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (ServiceProvider.Settings.SelectedBitmap == null || ServiceProvider.Settings.SelectedBitmap.FileItem == null)
          return;
        if (MessageBox.Show("Do you really want to delete this file ?", "Delete file", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
        {
          int indx = ImageLIst.SelectedIndex;
          Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(ServiceProvider.Settings.SelectedBitmap.FileItem.FileName,
                                                             UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
          if (indx > -1 || indx < ImageLIst.Items.Count)
            ImageLIst.SelectedItem = ImageLIst.Items[indx];
          if (indx >= ImageLIst.Items.Count)
            ImageLIst.SelectedIndex = ImageLIst.Items.Count - 1;
        }
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Error to delete file", exception);
      }
    }

    private void mnu_show_explorer_Click(object sender, RoutedEventArgs e)
    {
      if (ServiceProvider.Settings.SelectedBitmap == null || ServiceProvider.Settings.SelectedBitmap.FileItem == null)
        return;
      try
      {
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "explorer";
        processStartInfo.UseShellExecute = true;
        processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
        processStartInfo.Arguments =
            string.Format("/e,/select,\"{0}\"", ServiceProvider.Settings.SelectedBitmap.FileItem.FileName);
        Process.Start(processStartInfo);
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Error to show file in explorer", exception);
      }
    }

    private void mnu_open_Click(object sender, RoutedEventArgs e)
    {
      if (ServiceProvider.Settings.SelectedBitmap == null || ServiceProvider.Settings.SelectedBitmap.FileItem == null)
        return;
      Process.Start(ServiceProvider.Settings.SelectedBitmap.FileItem.FileName);
    }
  }
}
