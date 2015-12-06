using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;

namespace RandomImage
{
    public class RandomImageFinder
    {
        private string _searchDirectoryPath;
        private bool _haventSearchedImagesYet;
        private ImagesCollection _imagesCollection;
        private bool _includeSubdirs;
        private bool _threadAborted;

        public event Action SearchAborted;
        public event Action ImagesCountChanged;
        public event Action SearchStarted;
        public event Action SearchFinished;
        private event Action SearchFinishedInternal;

        private Thread _searchThread;

        private readonly string[] _imageFileMasks = { "*.gif", "*.jpeg", "*.jpg", "*.png" };
        private readonly string[] _excludeSearchDirectories = { "C:\\Windows", "C:\\Program Files", "C:\\Program Files (x86)", "C:\\ProgramData", "C:\\$Recycle.Bin", "C:\\Boot" };

        public RandomImageFinder()
        {
            _haventSearchedImagesYet = true;
            _searchDirectoryPath = Environment.CurrentDirectory;
            _imagesCollection = new ImagesCollection();
            _threadAborted = false;

            SearchFinishedInternal += CompleteSearch;
        }

        ~RandomImageFinder()
        {
            AbortSeaerch();
        }

        public string NextImage()
        {
            if (_haventSearchedImagesYet)
                SearchImages();

            return _imagesCollection.IsEmpty ? String.Empty : _imagesCollection.Next();
        }

        public string PrevImage()
        {
            if (_haventSearchedImagesYet)
                SearchImages();

            return _imagesCollection.IsEmpty ? String.Empty : _imagesCollection.Prev();
        }

        public void UpdateCollection()
        {
            SearchImages();
        }

        internal void AbortSeaerch()
        {
            AbortThreadInternal();
            _imagesCollection.Clear();
            _haventSearchedImagesYet = true;
            RiseSearchAborted();
            RiseImagesCountChanged();
        }

        private void SearchImages()
        {
            if (!System.IO.Directory.Exists(_searchDirectoryPath))
            {
                System.Windows.MessageBox.Show("Selected directory doesn't exist anymore, choose another one. :3");
                return;
            }

            _imagesCollection.Clear();
            _haventSearchedImagesYet = true;
            RiseImagesCountChanged();
            var searchOptions = new SearchImagesOptions()
            {
                IncludeSubdirs = _includeSubdirs
            };

            object callbackParams = new object[4]
            {
                _searchDirectoryPath, _imagesCollection, _imageFileMasks, searchOptions
            };
            RiseSearchStarted();
            AbortThreadInternal();
            _threadAborted = false;
            _searchThread = new Thread(SearchDelegate);
            _searchThread.Start(callbackParams);
        }

        private void AbortThreadInternal()
        {
            if (_searchThread != null)
            {
                if (_searchThread.IsAlive)
                {
                    _searchThread.Abort();
                    _threadAborted = true;
                }
                _searchThread = null;
            }
        }

        private void CompleteSearch()
        {
            _haventSearchedImagesYet = false;
            _imagesCollection.Reorganize();
            _imagesCollection.Shuffle();
            RiseSearchFinished();
        }

        private void SearchDelegate(object parameters)
        {
            try
            {
                var pms = (parameters as object[]);
                if (pms.GetUpperBound(0) != 3)
                    throw new ArgumentException("parameters have not all the parameters");

                var dir = pms[0].ToString();
                var ic = pms[1] as ImagesCollection;
                var fileMasks = pms[2] as IReadOnlyCollection<string>;
                var so = pms[3] as SearchImagesOptions;
                Search(dir, ic, fileMasks, so);
                RiseSearchFinishedInternal();
            }
            catch (ThreadAbortException) { }
        }

        private void Search(string directory, ImagesCollection foundFiles, IReadOnlyCollection<string> fileMasks, SearchImagesOptions searchOptions)
        {
            foreach (var excludeDirectory in _excludeSearchDirectories)
                if (directory.StartsWith(excludeDirectory))
                    return;

            List<string> images = new List<string>();
            try
            {
                foreach (var fileMask in fileMasks)
                {
                    images.AddRange(System.IO.Directory.EnumerateFiles(directory, fileMask, SearchOption.TopDirectoryOnly));
                }
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log("Files enumeration failed in directory " + directory, ex.Message);
            }

            foundFiles.AddRange(directory, images);
            if (!searchOptions.IncludeSubdirs)
                return;

            IEnumerable<string> subDirs = new List<string>();
            try
            {
                subDirs = System.IO.Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                CrashLogger.Instance.Log("Directory enumeration failed in directory " + directory, ex.Message);
            }

            foreach (var subDir in subDirs)
                Search(subDir, foundFiles, fileMasks, searchOptions);
        }

        public void Reorganize()
        {
            _imagesCollection.Reorganize();
        }

        public string SearchDirectoryPath
        {
            get { return _searchDirectoryPath; }
            set
            {
                if (_searchDirectoryPath.Equals(value))
                    return;

                _searchDirectoryPath = value;
                _haventSearchedImagesYet = true;
            }
        }

        public bool IncludeSubdirs
        {
            get { return _includeSubdirs; }
            set
            {
                if (_includeSubdirs == value)
                    return;

                _includeSubdirs = value;
            }
        }

        public string CurrentImage
        {
            get
            {
                return _imagesCollection.Count > 0 ?
                    _imagesCollection.Current() : string.Empty;
            }
        }

        public int Count
        {
            get { return _imagesCollection.Count; }
        }

        public bool IsUpdated
        {
            get
            {
                return !_threadAborted && !_haventSearchedImagesYet;
            }
        }

        private void RiseSearchFinished()
        {
            if (SearchFinished != null)
                SearchFinished();
            RiseImagesCountChanged();
        }

        private void RiseSearchFinishedInternal()
        {
            if (SearchFinishedInternal != null)
                SearchFinishedInternal();
        }

        private void RiseImagesCountChanged()
        {
            if (ImagesCountChanged != null)
                ImagesCountChanged();
        }

        private void RiseSearchStarted()
        {
            if (SearchStarted != null)
                SearchStarted();
        }

        private void RiseSearchAborted()
        {
            if (SearchAborted != null)
                SearchAborted();
        }
    }
}