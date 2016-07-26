using System;
using System.IO;

namespace RandomImage
{
    internal class CrashLogger : IDisposable
    {
        private const string _logFileName = "pi_c_random_crash_log.txt";
        private string _logFileDirectory;
        private StreamWriter _logsWriter;
        private bool _isInitialized = false;
        private const string boundry = "-----------------------";

        static private Lazy<CrashLogger> _instance = new Lazy<CrashLogger>(() => new CrashLogger());
        public static CrashLogger Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private string LogsFullPath
        {
            get
            {
                return _logFileDirectory + "\\" + _logFileName;
            }
        }

        private CrashLogger()
        {
            _logFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Init();
        }

        private void Init()
        {
            try
            {
                if (!File.Exists(LogsFullPath))
                {
                    var file = File.Create(LogsFullPath);
                    file.Close();
                    file.Dispose();
                }

                _logsWriter = new StreamWriter(LogsFullPath, true);
                _logsWriter.WriteLine(string.Concat("Today is ", DateTime.Now.ToShortDateString().ToString(), ", glad to see you again."));
                _logsWriter.Flush();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Sorry, but I can't create crash log file because of" + Environment.NewLine + ex.Message);
            }
        }

        public void Log(string msg, string exception_data)
        {
            if (!_isInitialized)
                return;

            try
            {
                _logsWriter.WriteLine(DateTime.Now.ToString());

                if (!msg.IsNullOrEmpty())
                    _logsWriter.WriteLine(msg);
                if (!exception_data.IsNullOrEmpty())
                    _logsWriter.WriteLine(exception_data);

                _logsWriter.WriteLine(boundry);
                _logsWriter.Flush();
            }
            catch { }
        }

        #region IDisposable

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logsWriter.Dispose();
                }

                _logsWriter = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}