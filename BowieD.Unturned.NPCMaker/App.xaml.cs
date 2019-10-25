﻿using BowieD.Unturned.NPCMaker.Configuration;
using BowieD.Unturned.NPCMaker.Forms;
using BowieD.Unturned.NPCMaker.Localization;
using BowieD.Unturned.NPCMaker.Logging;
using BowieD.Unturned.NPCMaker.Notification;
using BowieD.Unturned.NPCMaker.Updating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace BowieD.Unturned.NPCMaker
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IUpdateManager UpdateManager { get; private set; }
        public static INotificationManager NotificationManager { get; private set; }
        public static List<ILogger> Logger { get; private set; } = new List<ILogger>();
        public static Version Version
        {
            get
            {
                try
                {
                    if (_readVersion == null)
                        _readVersion = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
                    return _readVersion;
                }
                catch { return new Version("0.0.0.0"); }
            }
        }
        public App()
        {
            InitializeComponent();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        public new void Run()
        {
            InitLoggers();
            Logger.LogInfo($"BowieD.Unturned.NPCMaker {Version}. Copyright (C) 2019 Anton 'BowieD' Galakhov");
            Logger.LogInfo("This program comes with ABSOLUTELY NO WARRANTY; for details type `license w'.");
            Logger.LogInfo("This is free software, and you are welcome to redistribute it");
            Logger.LogInfo("under certain conditions; type `license c' for details.");
            Logger.LogInfo("[EXTRCT] - Extracting libraries...");
            #region COPY LIBS
            CopyResource(NPCMaker.Properties.Resources.DiscordRPC, AppConfig.Directory + "DiscordRPC.dll");
            CopyResource(NPCMaker.Properties.Resources.Newtonsoft_Json, AppConfig.Directory + "Newtonsoft.Json.dll");
            CopyResource(NPCMaker.Properties.Resources.ControlzEx, AppConfig.Directory + "ControlzEx.dll");
            CopyResource(NPCMaker.Properties.Resources.MahApps_Metro, AppConfig.Directory + "MahApps.Metro.dll");
            CopyResource(NPCMaker.Properties.Resources.Microsoft_Xaml_Behaviors, AppConfig.Directory + "Microsoft.Xaml.Behaviors.dll");
            CopyResource(NPCMaker.Properties.Resources.MahApps_Metro_IconPacks_Core, AppConfig.Directory + "MahApps.Metro.IconPacks.Core.dll");
            CopyResource(NPCMaker.Properties.Resources.MahApps_Metro_IconPacks_Material, AppConfig.Directory + "MahApps.Metro.IconPacks.Material.dll");
            CopyResource(NPCMaker.Properties.Resources.Xceed_Wpf_AvalonDock, AppConfig.Directory + "Xceed.Wpf.AvalonDock.dll");
            CopyResource(NPCMaker.Properties.Resources.Xceed_Wpf_AvalonDock_Themes_Aero, AppConfig.Directory + "Xceed.Wpf.AvalonDock.Themes.Aero.dll");
            CopyResource(NPCMaker.Properties.Resources.Xceed_Wpf_AvalonDock_Themes_Metro, AppConfig.Directory + "Xceed.Wpf.AvalonDock.Themes.Metro.dll");
            CopyResource(NPCMaker.Properties.Resources.Xceed_Wpf_AvalonDock_Themes_VS2010, AppConfig.Directory + "Xceed.Wpf.AvalonDock.Themes.VS2010.dll");
            CopyResource(NPCMaker.Properties.Resources.Xceed_Wpf_Toolkit, AppConfig.Directory + "Xceed.Wpf.Toolkit.dll");
            #endregion
            Logger.LogInfo("[EXTRCT] - Extraction complete!");
            AppConfig.Instance.Load();
            #region SCALE
            Resources["Scale"] = AppConfig.Instance.scale;
            #endregion
#if !FAST
            App.UpdateManager = new GitHubUpdateManager();
            var result = App.UpdateManager.CheckForUpdates().GetAwaiter().GetResult();
            if (result == UpdateAvailability.AVAILABLE)
            {
                if (AppConfig.Instance.autoUpdate)
                {
                    App.UpdateManager.StartUpdate();
                    return;
                }
                else
                {
                    LocalizationManager.LoadLanguage(AppConfig.Instance.language);
                    var dlg = MessageBox.Show(LocalizationManager.Current.Interface["Update_Available_Body"], LocalizationManager.Current.Interface["Update_Available_Title"], MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dlg == MessageBoxResult.Yes)
                    {
                        App.UpdateManager.StartUpdate();
                        return;
                    }
                }
            }
#else
            Logger.LogInfo("[APP] - DebugFast enabled! Skipping update check...");
#endif
            if (!LocalizationManager.IsLoaded)
                LocalizationManager.LoadLanguage(AppConfig.Instance.language);
#if DEBUG
            Logger.LogInfo("[APP] - Opening MainWindow...");
#else
            Logger.LogInfo("[APP] - Closing console and opening app...");
#endif
            MainWindow mw = new MainWindow();
            InitManagers();
#if DEBUG
#else
            ConsoleLogger.HideConsoleWindow();
#endif
            mw.Show();
            base.Run();
        }
        public static void InitLoggers()
        {
            Logger.Add(new ConsoleLogger());
            Logger.Add(new FileLogger());
            Logger.Open();
        }
        public static void InitManagers()
        {
            UpdateManager = new GitHubUpdateManager();
            NotificationManager = new NotificationManager();
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppCrashReport acr = new AppCrashReport(e.Exception);
            acr.ShowDialog();
            e.Handled = acr.Handle;
            if (acr.Handle)
                App.Logger.LogWarning($"[ACR] - Ignoring exception {e.Exception.Message}.");
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains(".resources"))
                return null;
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(d => d.FullName == args.Name);
            if (assembly != null)
                return assembly;
            string fileName = args.Name.Split(',')[0] + ".dll";
            string asmFile = Path.Combine(AppConfig.Directory, fileName);
            try
            {
                return Assembly.LoadFrom(asmFile);
            }
            catch { return null; }
        }
        private void CopyResource(byte[] res, string file)
        {
            Logger.LogInfo($"[EXTRCT] - Extracting to {file}");
            try
            {
                using (Stream output = File.OpenWrite(file))
                {
                    output.Write(res, 0, res.Length);
                }
            }
            catch { }
        }
        private static Version _readVersion = null;
    }
}
