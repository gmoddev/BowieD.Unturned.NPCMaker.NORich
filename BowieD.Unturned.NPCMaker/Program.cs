﻿using BowieD.Unturned.NPCMaker.Common.Utility;
using BowieD.Unturned.NPCMaker.Configuration;
using BowieD.Unturned.NPCMaker.NPC;
using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace BowieD.Unturned.NPCMaker
{
    public sealed class Program
    {
        internal const string CRASH_SAVE_FILENAME = "crashSave.npcproj";

        [STAThread]
        private static void Main()
        {
            try
            {
                SetupExceptionHandling();

                try
                {
                    Directory.SetCurrentDirectory(AppConfig.ExeDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("could not set current directory");
                    Console.WriteLine(ex);
                }

                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception e)
            {
                TryToSaveProject();
                DisplayException(e);
                SaveToCrashException(e);
                ForceExit();
            }
        }

        internal static void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            TryToSaveProject();
            DisplayException(e.Exception);
            SaveToCrashException(e.Exception);
            ForceExit();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TryToSaveProject();
            DisplayException((Exception)e.ExceptionObject);
            SaveToCrashException((Exception)e.ExceptionObject);
            ForceExit();
        }

        private static void DisplayException(Exception e)
        {
            const string caption = "NPC Maker Crashed";

            try
            {
                switch (e)
                {
                    case SecurityException _:
                    case UnauthorizedAccessException _:
                        MessageBox.Show($"Security exception.\nTry running the app with admin privileges.\n{e}", caption);
                        break;
                    default:
                        MessageBox.Show(e.ToString(), caption);
                        break;
                }
            }
            catch { }
        }

        private static void SaveToCrashException(Exception e)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(AppConfig.ExeDirectory, "crash.txt"), true))
                {
                    writer.WriteLine(DebugUtility.GetDebugInformation());
                    writer.WriteLine();
                    writer.WriteLine(e);
                }
            }
            catch { }
        }

        private static void TryToSaveProject()
        {
            try
            {
                ProjectData.CurrentProject.file = Path.Combine(AppConfig.ExeDirectory, CRASH_SAVE_FILENAME);

                ProjectData.CurrentProject.DoSave();
            }
            catch { }
        }

        private static void ForceExit()
        {
            Environment.Exit(1);
        }
    }
}
