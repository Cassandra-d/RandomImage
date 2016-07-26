using System;
using System.Windows;
using System.Windows.Threading;

namespace RandomImage
{
    public partial class App : Application
    {
        public static readonly RandomImageFinder Randomizer = new RandomImageFinder();
        public static readonly SettingsManager SettingsManager = new SettingsManager();

        //every 2 min it will save settings
        private static System.Threading.Timer SaveSettingsTimer =
            new System.Threading.Timer(SaveSettingsCallback, null, 120000, 120000);

        public App()
        {
            DispatcherUnhandledException += ApplicationScopeExceptionHandler;

            SettingsManager.LoadSettings();
            Randomizer.SearchDirectoryPath = SettingsManager.Settings.CurrentPlace;
        }
        ~App()
        {
            SettingsManager.SaveSettings();
            SaveSettingsTimer.Dispose();
        }

        private static void SaveSettingsCallback(object stateInfo)
        {
            SettingsManager.SaveSettings();
        }

        private void ApplicationScopeExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            CrashLogger.Instance.Log("App()", e.Exception.Message);
            MessageBox.Show("Critical error, I dunno what to do, HALT!");
            Environment.Exit(1);
        }
    }
}