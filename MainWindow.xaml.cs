using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace RandomImage
{
    public partial class MainWindow : Window
    {
        private class StringMessages
        {
            public const string NEED_SEARCH = "I didn't find anything yet. :3";
            public const string DID_NOT_FOUND_IMAGE = "Sorry, but I found 0 images, try another directory. :3";
            public const string PATH_NOT_SELECTED = "Looks like you have not selected a path! :3";
            public const string LOG_HISTORY_DELETED = "History deleted.";
            public const string LOG_CLEARING_USING_HISTORY = "Clearing using history";
            public const string LOG_DID_NOT_CLEAR_HISTORY = "Whoooops! I didn't clear history.";
            public const string LOG_DISPLAYING_IMAGE = "Displaying image";
            public const string MODIFICATION_DATE = "Modification Date: {0}";
            public const string UNABLE_TO_GET_MODIFICATION_DATE = "Unable to get Modification Date";
            public const string LOG_PATH_DOES_NOT_EXIST = "Directory does not exist";
            public const string SELECTED_DIRECTORY_DOES_NOT_EXIST = "Selected directory doesn't exist anymore, choose another one. :3";
        }

        private const int MaximumLastPlacesCount = 10;

        public MainWindow()
        {
            InitializeComponent();

            App.Randomizer.SearchStarted += ShowProgress;
            App.Randomizer.SearchStarted += LockUI;
            App.Randomizer.SearchFinished += StopShowingProgress;
            App.Randomizer.SearchFinished += UnlockUI;
            App.Randomizer.SearchFinished += UpdateImagesCountLabel;
            App.Randomizer.ImagesCountChanged += UpdateImagesCountLabel;
            App.Randomizer.SearchFinished += ShowImageAndInfo;
            App.Randomizer.SearchAborted += HideImageAndInfo;
            App.Randomizer.SearchAborted += UpdateImagesCountLabel;
            App.Randomizer.SearchAborted += StopShowingProgress;
            App.Randomizer.SearchAborted += UnlockUI;

            Next_btn.Focus();
            HideImageAndInfo();
            StopShowingProgress();
            AdjustWindowSize();
            ApplySettings();

            if (!IsSearchPathSet) return;
            if (!IsSelectedDirectoryExist)
            {
                System.Windows.MessageBox.Show(StringMessages.SELECTED_DIRECTORY_DOES_NOT_EXIST);
                return;
            }
            App.Randomizer.UpdateCollection();
        }

        private void AdjustWindowSize()
        {
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            if (resolution.Height <= Height ||
                resolution.Width <= Width)
            {
                Width = MinWidth;
                Height = MinHeight;
            }
        }

        private void ApplySettings()
        {
            SearchInSubdirs_chbox.IsChecked = App.SettingsManager.Settings.SearchInSubdirectories;
            App.Randomizer.IncludeSubdirs = App.SettingsManager.Settings.SearchInSubdirectories;
            AutomaticalyCopyToClipboard_chbox.IsChecked = App.SettingsManager.Settings.AutomaticalyCopyImageToClipboard;
            CheckForAlreadyUsed_chbox.IsChecked = App.SettingsManager.Settings.CheckForAlreadyUsedImages;

            if (App.SettingsManager.Settings.LastPlaces.Any())
            {
                App.Randomizer.SearchDirectoryPath = App.SettingsManager.Settings.LastPlaces.ElementAt(0);
                SelectedPath_lbl.Content = App.SettingsManager.Settings.LastPlaces.ElementAt(0);
            }

            if (App.SettingsManager.Settings.LastPlaces.Count > MaximumLastPlacesCount)
                App.SettingsManager.Settings.LastPlaces.RemoveRange(
                    MaximumLastPlacesCount,
                    App.SettingsManager.Settings.LastPlaces.Count - MaximumLastPlacesCount);

            foreach (string path in App.SettingsManager.Settings.LastPlaces)
                Search_cbox.Items.Add(path);
        }

        private bool IsSearchPathSet
        {
            get
            {
                return !string.IsNullOrEmpty(App.SettingsManager.Settings.CurrentPlace);
            }
        }

        private bool IsSelectedDirectoryExist
        {
            get
            {
                return System.IO.Directory.Exists(App.Randomizer.SearchDirectoryPath);
            }
        }

        private void SuggestToSelectAPath()
        {
            System.Windows.MessageBox.Show(StringMessages.PATH_NOT_SELECTED);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = App.Randomizer.SearchDirectoryPath ?? "";
                DialogResult result = dialog.ShowDialog();
                if (result.ToString().ToLower().Equals("ok"))
                {
                    ChangeCurrentPath(dialog.SelectedPath);
                }
            }
        }

        private void Next_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSearchPathSet)
            {
                SuggestToSelectAPath();
                return;
            }

            if (!App.Randomizer.IsUpdated)
            {
                if (!IsSelectedDirectoryExist)
                {
                    System.Windows.MessageBox.Show(StringMessages.SELECTED_DIRECTORY_DOES_NOT_EXIST);
                    return;
                }
                App.Randomizer.UpdateCollection();
                return;
            }

            string path = App.Randomizer.NextImage();
            if (!string.IsNullOrEmpty(path))
                DisplayImage(path);
            else
                System.Windows.MessageBox.Show(StringMessages.DID_NOT_FOUND_IMAGE);
        }

        private void Prev_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSearchPathSet)
            {
                SuggestToSelectAPath();
                return;
            }

            if (!App.Randomizer.IsUpdated)
            {
                if (!IsSelectedDirectoryExist)
                {
                    System.Windows.MessageBox.Show(StringMessages.SELECTED_DIRECTORY_DOES_NOT_EXIST);
                    return;
                }
                App.Randomizer.UpdateCollection();
                return;
            }

            string path = App.Randomizer.PrevImage();
            if (!string.IsNullOrEmpty(path))
                DisplayImage(path);
            else
                System.Windows.MessageBox.Show(StringMessages.DID_NOT_FOUND_IMAGE);
        }

        private void Add_btn_Click(object sender, RoutedEventArgs e)
        {
            var str = App.Randomizer.Count == 0 ?
                StringMessages.DID_NOT_FOUND_IMAGE : App.Randomizer.CurrentImage;

            PostToLogBox(str);
            PostToClipboard(str);

            if (App.Randomizer.Count == 0)
                return;
            MarkAsUsed(str);
            DisplayUsedImageLabel(str);
        }

        private void ClearUsedHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.SettingsManager.Settings.ClearUsedImages();
                PostToLogBox(StringMessages.LOG_HISTORY_DELETED);
                DisplayUsedImageLabel(App.Randomizer.CurrentImage);
            }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log(StringMessages.LOG_CLEARING_USING_HISTORY, ex.Message);
                PostToLogBox(StringMessages.LOG_DID_NOT_CLEAR_HISTORY);
            }
        }

        private void AutomaticalyCopyToClipboard_chbox_Click(object sender, RoutedEventArgs e)
        {
            App.SettingsManager.Settings.AutomaticalyCopyImageToClipboard =
                (sender as System.Windows.Controls.CheckBox).IsChecked.Value;
        }

        private void CheckForAlreadyUsed_chbox_Click(object sender, RoutedEventArgs e)
        {
            bool checkForUsing = (sender as System.Windows.Controls.CheckBox).IsChecked.Value;
            App.SettingsManager.Settings.CheckForAlreadyUsedImages = checkForUsing;

            if (checkForUsing)
                DisplayUsedImageLabel(App.Randomizer.CurrentImage);
            else
                IsImageAlreadyUsed_lbl.Visibility = Visibility.Hidden;
        }

        private void Search_cbox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count != 0)
                return;
            ChangeCurrentPath(e.AddedItems[0].ToString());
            HideImageAndInfo();

            if (!IsSelectedDirectoryExist)
            {
                System.Windows.MessageBox.Show(StringMessages.SELECTED_DIRECTORY_DOES_NOT_EXIST);
                return;
            }
            App.Randomizer.UpdateCollection();
        }

        private void SearchInSubdirs_chbox_Click(object sender, RoutedEventArgs e)
        {
            var val = (sender as System.Windows.Controls.CheckBox).IsChecked.Value;
            App.Randomizer.IncludeSubdirs = val;
            App.SettingsManager.Settings.SearchInSubdirectories = val;
            HideImageAndInfo();

            if (!IsSelectedDirectoryExist)
            {
                System.Windows.MessageBox.Show(StringMessages.SELECTED_DIRECTORY_DOES_NOT_EXIST);
                return;
            }
            App.Randomizer.UpdateCollection();
        }

        private void Stop_btn_Click(object sender, RoutedEventArgs e)
        {
            App.Randomizer.AbortSeaerch();
        }

        private void DisplayImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            DisplayImageFileInfo(imagePath);
            DisplayUsedImageLabel(imagePath);

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath);
                image.EndInit();
                image.Freeze();
                ImageBehavior.SetAnimatedSource(Preview_img, image);
                if (App.SettingsManager.Settings.AutomaticalyCopyImageToClipboard)
                {
                    PostToClipboard(imagePath);
                    PostToLogBox(imagePath);
                    MarkAsUsed(imagePath);
                    DisplayUsedImageLabel(App.Randomizer.CurrentImage);
                }
                Preview_img.Visibility = Visibility.Visible;
            }
            catch (Exception e)
            {
                PostToLogBox(e.Message + " : " + imagePath);
                CrashLogger.Instance.Log(StringMessages.LOG_DISPLAYING_IMAGE, e.Message);
            }
        }

        private void HideImage()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(HideImage));
                return;
            }

            Preview_img.Visibility = Visibility.Hidden;
        }

        private void UpdateImagesCountLabel()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(UpdateImagesCountLabel));
                return;
            }

            FoundImagesCount_lbl.Content = App.Randomizer.Count;
        }

        private void DisplayModificationDate(string filePath)
        {
            string date = Aux.GetModificationDate(filePath).ToShortDateString();
            ImageModificationDate_lbl.Content =
                !string.IsNullOrEmpty(date)
                    ? string.Format(StringMessages.MODIFICATION_DATE, date)
                    : StringMessages.UNABLE_TO_GET_MODIFICATION_DATE;
            ImageModificationDate_lbl.Visibility = Visibility.Visible;
        }

        private void DisplaySize(string filePath)
        {
            long size = Aux.GetSize(filePath);
            if (size == 0)
                return;

            long kb = size / 1024;
            float mb = kb / 1024;
            string result = "0";

            if (mb >= 1.0)
                result = string.Concat(mb.ToString(), " MB");
            else if (kb != 0)
                result = string.Concat(kb.ToString(), " KB");
            else
                result = string.Concat(size.ToString(), " Bytes");

            ImageSize_lbl.Content = result;
            ImageSize_lbl.Visibility = Visibility.Visible;
        }

        private void HideSize()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(HideSize));
                return;
            }
            ImageSize_lbl.Visibility = Visibility.Hidden;
        }

        private void DisplayImageFileInfo(string filePath)
        {
            DisplayModificationDate(filePath);
            DisplaySize(filePath);
        }

        private void HideImageFileInfo()
        {
            HideModificationDate();
            HideSize();
        }

        private void HideModificationDate()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(HideModificationDate));
                return;
            }

            ImageModificationDate_lbl.Visibility = Visibility.Hidden;
        }

        private void DisplayUsedImageLabel(string imagePath)
        {
            if (App.SettingsManager.Settings.CheckForAlreadyUsedImages && !string.IsNullOrEmpty(imagePath))
                IsImageAlreadyUsed_lbl.Visibility = App.SettingsManager.Settings.UsedImages.Contains(Aux.GetHashCode(imagePath)) ?
                    Visibility.Visible : Visibility.Hidden;
        }

        private void HideUsedImageLabel()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(HideUsedImageLabel));
                return;
            }

            IsImageAlreadyUsed_lbl.Visibility = Visibility.Hidden;
        }

        private void ShowProgress()
        {
            Progress_pbar.Visibility = Visibility.Visible;
            Stop_btn.Visibility = Visibility.Visible;
        }

        private void StopShowingProgress()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(StopShowingProgress));
                return;
            }

            Progress_pbar.Visibility = Visibility.Hidden;
            Stop_btn.Visibility = Visibility.Hidden;
        }

        private void LockUI()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(LockUI));
                return;
            }

            ButtonsPanel.IsEnabled = false;
            SettingsPanel.IsEnabled = false;
        }

        private void UnlockUI()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(UnlockUI));
                return;
            }

            ButtonsPanel.IsEnabled = true;
            SettingsPanel.IsEnabled = true;
        }

        private void ChangeCurrentPath(string path)
        {
            App.Randomizer.SearchDirectoryPath = path;
            App.SettingsManager.Settings.AddPlace(path);
            SelectedPath_lbl.Content = path;
            Search_cbox.Items.Remove(path);
            Search_cbox.Items.Insert(0, path);
        }

        private void MarkAsUsed(string imagePath)
        {
            if (App.SettingsManager.Settings.CheckForAlreadyUsedImages)
                App.SettingsManager.Settings.UsedImages.Add(Aux.GetHashCode(imagePath));
        }

        private void PostToClipboard(string path)
        {
            // old way to post to clipboard. has started to work very slow even with STA
            // replaced with helper
            ////for (int i = 0; i < 10; i++)
            //{
            //    try
            //    {
            //        //System.Windows.Clipboard.SetDataObject(path, true);
            //        //System.Windows.Clipboard.SetText(path);
            //    catch {}
            //    System.Threading.Thread.Sleep(10);
            //}

            // found better solution, replaced with ntive
            //new SetClipboardHelper(System.Windows.DataFormats.Text, path).Go();

            // Welp, here it is...
            int i = 0;
            while (NativeSetClipboardHelper.CopyTextToClipboard(path) == false && i < 20)
            {
                ++i;
                System.Threading.Thread.Sleep(10);
            }
        }

        private void PostToLogBox(string text)
        {
            if (Information_tb.Text.Length != 0)
                Information_tb.Text += Environment.NewLine;
            Information_tb.Text += text;
            TextScroll_scvw.ScrollToEnd();
        }

        private void HideImageAndInfo()
        {
            HideImage();
            HideImageFileInfo();
            HideUsedImageLabel();
        }

        private void ShowImageAndInfo()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(ShowImageAndInfo));
                return;
            }

            if (!IsSearchPathSet || App.Randomizer.Count == 0)
                return;

            var file = App.Randomizer.CurrentImage;
            DisplayImage(file);
            DisplayImageFileInfo(file);
            DisplayUsedImageLabel(file);
        }
    }
}