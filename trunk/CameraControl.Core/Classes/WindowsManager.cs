using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CameraControl.Core.Interfaces;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class WindowsManager
    {
        public AsyncObservableCollection<WindowCommandItem> WindowCommands { get; set; }


        public delegate void EventEventHandler(string cmd, object o);
        public virtual event EventEventHandler Event;

        private List<IWindow> WindowsList;

        public WindowsManager()
        {
            WindowsList = new List<IWindow>();
            WindowCommands = new AsyncObservableCollection<WindowCommandItem>();
        }

        public void Add(IWindow window)
        {
            WindowsList.Add(window);
        }

        public void ExecuteCommand(string cmd)
        {
            ExecuteCommand(cmd, null);
        }

        public void ExecuteCommand(string cmd, object o)
        {
            foreach (IWindow window in WindowsList)
            {
                window.ExecuteCommand(cmd, o);
            }
            if (Event != null)
                Event(cmd, o);
        }

        public void ApplyTheme()
        {
            foreach (IWindow window in WindowsList)
            {
                Window win = window as Window;
                if (win != null)
                {
                    ServiceProvider.Settings.ApplyTheme(win);
                }
            }
            foreach (Window window in Application.Current.Windows)
            {
                ServiceProvider.Settings.ApplyTheme(window);
            }
        }

        public IWindow Get(Type t)
        {
            return WindowsList.FirstOrDefault(window => window.GetType() == t);
        }

        public void Remove(string type)
        {
            IWindow windowToRemove = null;
            foreach (IWindow window in WindowsList.Where(window => window.GetType().ToString() == type))
            {
                windowToRemove = window;
            }
            if (windowToRemove != null)
                WindowsList.Remove(windowToRemove);
        }

        /// <summary>
        /// Registers commands used by the application core.
        /// </summary>
        public void RegisterKnowCommands()
        {
            AddCommandsFromType(typeof(CmdConsts));
            AddCommandsFromType(typeof(WindowsCmdConsts));
            WindowCommands = new AsyncObservableCollection<WindowCommandItem>(WindowCommands.OrderBy(x => x.Name)); 
            foreach (WindowCommandItem item in WindowCommands)
            {
                switch (item.Name)
                {
                    case CmdConsts.Capture:
                        item.SetKey(Key.Space);
                        break;
                    case WindowsCmdConsts.Next_Image:
                        item.SetKey(Key.Right);
                        break;
                    case WindowsCmdConsts.Prev_Image:
                        item.SetKey(Key.Left);
                        break;
                    case WindowsCmdConsts.Like_Image:
                        item.SetKey(Key.P);
                        break;
                    case WindowsCmdConsts.Unlike_Image:
                        item.SetKey(Key.X);
                        break;
                }
            }
        }

        private void AddCommandsFromType(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static |
               BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            foreach (FieldInfo fieldInfo in fields)
            {
                WindowCommands.Add(new WindowCommandItem() { Name = fieldInfo.GetValue(type).ToString() });
            }
        }
    }
}
