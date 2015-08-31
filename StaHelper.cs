using System;
using System.Threading;

namespace RandomImage
{
    // Workaround for clipboard copiying
    // http://stackoverflow.com/questions/899350/how-to-copy-the-contents-of-a-string-to-the-clipboard-in-c
    // Thanks, Paul!

    abstract class StaHelper
    {
        readonly ManualResetEvent _complete = new ManualResetEvent(false);
        private static Thread _thread;

        public void Go()
        {
            if (_thread != null)
            {
                if (_thread.IsAlive)
                    _thread.Abort();
                _thread = null;
            }
           
            _thread = new Thread(new ThreadStart(DoWork))
            {
                IsBackground = true,
                Name = "Clipboard",
            };

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        // Thread entry method
        private void DoWork()
        {
            try
            {
                _complete.Reset();
                Work();
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                    return;

                if (DontRetryWorkOnFailed)
                    throw new Exception("Don't retry", ex);
                else
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        try
                        {
                            Thread.Sleep(300);
                            Work();
                            break;
                        }
                        catch (Exception e)
                        {
                            if (e is ThreadAbortException)
                                return;
                            // ex from first exception
                            //LogAndShowMessage(ex);
                        }
                    }
                }
            }
            finally
            {
                _complete.Set();
            }
        }

        public bool DontRetryWorkOnFailed { get; set; }

        // Implemented in base class to do actual work.
        protected abstract void Work();
    }
}
