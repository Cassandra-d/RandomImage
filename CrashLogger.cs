using System;
using System.IO;

namespace RandomImage
{
    internal class CrashLogger : IDisposable
    {
        private string _logFileName = "pi_c_random_crash_log.txt";
        private string _logFileDirectory;
        private StreamWriter _sw;
        private bool _isInitialized = false;
        private readonly string boundry = "-----------------------";

        static private Lazy<CrashLogger> _instance = new Lazy<CrashLogger>(() => new CrashLogger());

        static public CrashLogger Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private CrashLogger()
        {
            _logFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Init();
        }

        private string LogsFullPath
        {
            get
            {
                return _logFileDirectory + "\\" + _logFileName;
            }
        }

        private void Init()
        {
            try
            {
                if (!File.Exists(LogsFullPath))
                {
                    var file = File.Create(LogsFullPath);
                    file.Dispose();
                }

                _sw = new StreamWriter(LogsFullPath, true);
                _sw.WriteLine(String.Concat("Today is ", DateTime.Now.ToShortDateString().ToString(), ", glad to see you again."));
                _sw.Flush();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Sorry, but I can't create crash log file because of" + Environment.NewLine + ex.Message);
                return;
            }
        }

        public void Log(string msg, string exception_data)
        {
            if (!_isInitialized)
                return;

            _sw.WriteLine(DateTime.Now.ToString());
            if (!string.IsNullOrEmpty(msg))
                _sw.WriteLine(msg);
            if (!string.IsNullOrEmpty(exception_data))
                _sw.WriteLine(exception_data);
            _sw.WriteLine(boundry);
            _sw.Flush();
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sw.Dispose();
                }

                _sw = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}