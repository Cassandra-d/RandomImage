using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace RandomImage
{
    public class SettingsManager
    {
        private bool _validAppData = false;
        private string _appData = "";
        private readonly string _programDirectoryName = "PicRanDom";
        private readonly string _settingsFileName = "settings.xml";

        public Settings Settings { get; set; }

        public SettingsManager()
        {
            _validAppData = true;
            _appData = Environment.GetEnvironmentVariable("AppData");
            if (String.IsNullOrEmpty(_appData))
            {
                MessageBox.Show("Sorry, but I can't load or save settings file.");
                _validAppData = false;
            }

            Settings = new RandomImage.Settings();
        }

        public void SaveSettings()
        {
            if (!IsAppDataFound || IsSettingsDirectoryExist() == false)
            {
                CrashLogger.Instance.Log("Saving settings", 
                    string.Format("AppDataFound: {0}, SettingsDirectoryExist: {1}", IsAppDataFound, IsSettingsDirectoryExist()));
                return;
            }

            // we will serialize settings and used images separetely
            // I think that it's better to save at least something
            // so, don't unite it under one try block
            try
            {
                CreateSettingsFileIfDoesntExist();
                SerializeSettings();
            }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log("Saving settings", ex.Message);
            }
        }

        public void LoadSettings()
        {
            if (!IsAppDataFound || IsSettingsDirectoryExist() == false || !File.Exists(SettingsFileFullPath))
            {
                SettingsCleanUp();
                SaveSettings();
                CrashLogger.Instance.Log("Saving settings",
                    string.Format("AppDataFound: {0}, SettingsDirectoryExist: {1}, IsSettingsFileExist: {2}",
                    IsAppDataFound, IsSettingsDirectoryExist(), File.Exists(SettingsFileFullPath)));
                return;
            }

            try
            {
                DeserializeSettings();
            }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log("Loading settings", ex.Message);
                HandleDeseralizationException(SettingsFileFullPath, ex);
                return;
            }
        }

        private void SettingsCleanUp()
        {
            File.Delete(SettingsFileFullPath);
            CreateSettingsFileIfDoesntExist();
        }

        private string ProgramDirectory
        {
            get
            {
                return _appData + "\\" + _programDirectoryName;
            }
        }

        private string SettingsFileFullPath
        {
            get
            {
                return ProgramDirectory + "\\" + _settingsFileName; ;
            }
        }

        private bool IsAppDataFound
        {
            get
            {
                return _validAppData;
            }
        }

        private bool IsSettingsDirectoryExist()
        {
            bool retVal = true;
            try
            {
                if (!System.IO.Directory.Exists(ProgramDirectory))
                    System.IO.Directory.CreateDirectory(ProgramDirectory);
            }
            catch (Exception ex)
            {
                retVal = false;
                CrashLogger.Instance.Log("Checking settings directory", ex.Message);
            }
            return retVal;
        }

        private void CreateSettingsFileIfDoesntExist()
        {
            if (!File.Exists(SettingsFileFullPath))
            {
                File.Create(SettingsFileFullPath).Close();
            }
        }

        private void SerializeSettings()
        {
            using (StreamWriter sw = new StreamWriter(SettingsFileFullPath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(sw, Settings);
            }
        }

        private void DeserializeSettings()
        {
            using (FileStream fs = new FileStream(SettingsFileFullPath, FileMode.Open))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                Settings = (Settings)deserializer.Deserialize(fs);
            }
        }

        private void HandleDeseralizationException(String settingsPath, Exception ex)
        {
            if (ex is InvalidOperationException)
            {
                try
                {
                    File.Delete(settingsPath);
                    SettingsCleanUp();
                    SaveSettings();
                }
                catch { }
            }
            CrashLogger.Instance.Log("Settings deserealization", ex.Message);
        }
    }
}