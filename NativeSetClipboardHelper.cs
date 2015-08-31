using System;
using System.Runtime.InteropServices;

namespace RandomImage
{
    // Workaround for clipboard copiying
    // http://stackoverflow.com/questions/68666/clipbrd-e-cant-open-error-when-setting-the-clipboard-from-net
    // Thanks, Mar!

    public static class NativeSetClipboardHelper
    {
        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool SetClipboardData(uint uFormat, IntPtr data);

        private const uint CF_UNICODETEXT = 13;

        public static bool CopyTextToClipboard(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return false;
            }

            var global = Marshal.StringToHGlobalUni(text);

            SetClipboardData(CF_UNICODETEXT, global);
            CloseClipboard();

            //-------------------------------------------
            // Not sure, but it looks like we do not need 
            // to free HGLOBAL because Clipboard is now 
            // responsible for the copied data. (?)
            //
            // Otherwise the second call will crash
            // the app with a Win32 exception 
            // inside OpenClipboard() function
            //-------------------------------------------
            // Marshal.FreeHGlobal(global);

            return true;
        }
    }
}
