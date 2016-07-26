using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace RandomImage
{
    public class SettingsManager
    {
        private bool _validAppData = false;
        private string _appData = "";
        private const string _programDirectoryName = "PicRanDom";
        private const string _settingsFileName = "settings.xml";

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
                return ProgramDirectory + "\\" + _settingsFileName;
            }
        }

        private bool IsAppDataFound
        {
            get
            {
                return _validAppData;
            }
        }

        public Settings Settings { get; set; }

        public SettingsManager()
        {
            _validAppData = true;
            _appData = Environment.GetEnvironmentVariable("AppData");

            if (_appData.IsNullOrEmpty())
            {
                MessageBox.Show("Sorry, but I can't load or save settings file.");
                // but we still can continue work
                _validAppData = false;
            }

            Settings = new Settings();
        }

        public void SaveSettings()
        {
            if (!IsAppDataFound)
            {
                CrashLogger.Instance.Log("Saving settings", string.Format("AppDataFound: false"));
                return;
            }

            try
            {
                CreateSettingsDirectoryAndFileIfDoesntExist();
                SerializeSettings();

            }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log("SaveSettings", ex.Message);
                return;
            }
        }

        public void LoadSettings()
        {
            if (!IsAppDataFound || !File.Exists(SettingsFileFullPath))
            {
                try
                {
                    SettingsCleanUp();
                }
                catch (Exception ex)
                {
                    CrashLogger.Instance.Log("SettingsCleanUp", ex.Message);
                    return;
                }

                SaveSettings();
                return;
            }

            try
            {
                DeserializeSettings();
            }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log("DeserializeSettings", ex.Message);
                HandleDeseralizationException(SettingsFileFullPath, ex);
            }
        }

        private void SettingsCleanUp()
        {
            File.Delete(SettingsFileFullPath);
            CreateSettingsDirectoryAndFileIfDoesntExist();
        }

        private void CreateSettingsDirectoryAndFileIfDoesntExist()
        {
            if (!System.IO.Directory.Exists(ProgramDirectory))
                System.IO.Directory.CreateDirectory(ProgramDirectory);

            if (!File.Exists(SettingsFileFullPath))
                File.Create(SettingsFileFullPath).Close();
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

        private void HandleDeseralizationException(string settingsPath, Exception ex)
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
            CrashLogger.Instance.Log("HandleDeseralizationException", ex.Message);
        }
    }
}