namespace RandomImage
{
    // Workaround for clipboard copiying
    // http://stackoverflow.com/questions/899350/how-to-copy-the-contents-of-a-string-to-the-clipboard-in-c
    // Thanks, Paul!

    class SetClipboardHelper : StaHelper
    {
        readonly string _format;
        readonly object _data;

        public SetClipboardHelper(string format, object data)
        {
            _format = format;
            _data = data;
            DontRetryWorkOnFailed = false;
        }

        protected override void Work()
        {
            var obj = new System.Windows.DataObject(
                _format,
                _data
            );

            System.Windows.Clipboard.SetDataObject(obj, true);
        }
    }
}
