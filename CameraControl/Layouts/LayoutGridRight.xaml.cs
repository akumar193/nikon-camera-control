﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraControl.Core;
using Xceed.Wpf.Toolkit.Core.Input;

namespace CameraControl.Layouts
{
    /// <summary>
    /// Interaction logic for LayoutGridRight.xaml
    /// </summary>
    public partial class LayoutGridRight : LayoutBase
    {
        public LayoutGridRight()
        {
            InitializeComponent();
            zoombox.RelativeZoomModifiers.Clear();
            zoombox.RelativeZoomModifiers.Add(KeyModifier.None);
            zoombox.DragModifiers.Clear();
            zoombox.DragModifiers.Add(KeyModifier.None);
            ImageLIst = ImageLIstBox;
            InitServices();
            ServiceProvider.Settings.PropertyChanged += Settings_PropertyChanged;
            ServiceProvider.Settings.DefaultSession.PropertyChanged += Settings_PropertyChanged;
            folderBrowserComboBox1.SelectedPath = ServiceProvider.Settings.DefaultSession.Folder;
        }

        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DefaultSession" || e.PropertyName == "Folder")
            {
                folderBrowserComboBox1.SelectedPath = ServiceProvider.Settings.DefaultSession.Folder;
            }
        }

        private void folderBrowserComboBox1_ValueChanged(object sender, EventArgs e)
        {
            ServiceProvider.Settings.DefaultSession.Folder = folderBrowserComboBox1.SelectedPath;
            ServiceProvider.QueueManager.Clear();
            ServiceProvider.Settings.DefaultSession.Files.Clear();
            ServiceProvider.Settings.LoadData(ServiceProvider.Settings.DefaultSession);
        }

        public override void OnImageLoaded()
        {
            Dispatcher.Invoke(new Action(() => zoombox.FitToBounds()));
        }

        private void zoombox_ViewStackIndexChanged(object sender, Xceed.Wpf.Toolkit.Core.IndexChangedEventArgs e)
        {
            LoadFullRes();
        }


    }
}
