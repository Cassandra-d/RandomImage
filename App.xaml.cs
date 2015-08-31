using System;
using System.Windows;

namespace RandomImage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly RandomImageFinder Randomizer = new RandomImageFinder();
        public static readonly SettingsManager SettingsManager = new SettingsManager();

        //every 2 min it will save settings
        private static System.Threading.Timer SaveSettingsTimer =
            new System.Threading.Timer(SaveSettingsCallback, null, 120000, 120000);

        public App()
        {
            try
            { 
                SettingsManager.LoadSettings();
                Randomizer.SearchDirectoryPath = SettingsManager.Settings.CurrentPlace;
            }
            catch(Exception ex)
            {
                CrashLogger.Instance.Log("App()", ex.Message);
                MessageBox.Show("Critical error, I dunno what to do, HALT!");
                Environment.Exit(1);
            }
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
    }
}