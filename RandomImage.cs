using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace RandomImage
{
    public class RandomImageFinder
    {
        private string _searchDirectoryPath;
        private bool _haventSearchedImagesYet;
        private ImagesCollection _imagesCollection;
        private bool _includeSubdirs;

        public event Action SearchAborted;
        public event Action ImagesCountChanged;
        public event Action SearchStarted;
        public event Action SearchFinished;
        private event Action SearchFinishedInternal;

        private Task _searchTask;
        private CancellationTokenSource _searchCancellationTokenSource;

        private readonly string[] _imageFileMasks = { "*.gif", "*.jpeg", "*.jpg", "*.png" };
        private readonly string[] _excludeSearchDirectories = { "C:\\Windows", "C:\\Program Files", "C:\\Program Files (x86)", "C:\\ProgramData", "C:\\$Recycle.Bin", "C:\\Boot" };

        // PROPERTIES

        public bool IsIncludeSubdirs
        {
            get { return _includeSubdirs; }
            set
            {
                _includeSubdirs = value;
            }
        }

        public bool IsUpdated
        {
            get
            {
                return _searchTask != null && !_haventSearchedImagesYet;
            }
        }

        public string CurrentSearchDirectoryPath
        {
            get { return _searchDirectoryPath; }
            set
            {
                _searchDirectoryPath = value;
                _haventSearchedImagesYet = true;
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

        public int ImagesCount
        {
            get { return _imagesCollection.Count; }
        }

        // CTOR
        public RandomImageFinder()
        {
            _haventSearchedImagesYet = true;
            _searchDirectoryPath = Environment.CurrentDirectory;
            _imagesCollection = new ImagesCollection();

            SearchFinishedInternal += CompleteSearch;
        }

        // PUBLLIC METHODS

        public string NextImage()
        {
            if (_haventSearchedImagesYet)
                SearchImages();

            return _imagesCollection.IsEmpty ? string.Empty : _imagesCollection.Next();
        }

        public string PrevImage()
        {
            if (_haventSearchedImagesYet)
                SearchImages();

            return _imagesCollection.IsEmpty ? string.Empty : _imagesCollection.Prev();
        }

        public void SearchAndUpdateCollection()
        {
            SearchImages();
        }

        public void StopSeaerch()
        {
            StopSearchInternal();
            _imagesCollection.Clear();
            _haventSearchedImagesYet = true;
            RiseSearchStoped();
            RiseImagesCountChanged();
        }

        public void Reorganize()
        {
            _imagesCollection.Reorganize();
        }

        // PRIVATE/INTERNAL METHODS

        private void Search(
            string directory, ImagesCollection foundFiles, IReadOnlyCollection<string> fileMasks,
            SearchImagesOptions searchOptions, CancellationToken ct)
        {
            if (_excludeSearchDirectories.Any(exDir => exDir.ToLowerInvariant().Equals(directory.ToLowerInvariant())))
                return;

            var images = new List<string>();
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

            if (!searchOptions.IncludeSubdirs || ct.IsCancellationRequested)
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
                Search(subDir, foundFiles, fileMasks, searchOptions, ct);
        }

        private void SearchImages()
        {
            if (!System.IO.Directory.Exists(_searchDirectoryPath))
            {
                throw new DirectoryDoesNotExistAnymore(_searchDirectoryPath);
            }

            _imagesCollection.Clear();
            _haventSearchedImagesYet = true;
            RiseImagesCountChanged();
            var searchOptions = new SearchImagesOptions()
            {
                IncludeSubdirs = _includeSubdirs
            };

            RiseSearchStarted();
            StopSearchInternal();

            _searchCancellationTokenSource = new CancellationTokenSource();
            object callbackParams = new object[]
            {
                _searchDirectoryPath, _imagesCollection, _imageFileMasks, searchOptions, _searchCancellationTokenSource.Token
            };
            _searchTask = Task.Factory.StartNew(SearchDelegate, callbackParams);
        }

        private void StopSearchInternal()
        {
            if (_searchTask == null)
                return;
            if (_searchTask.Status == TaskStatus.Running)
            {
                _searchCancellationTokenSource.Cancel();
            }
            _searchTask = null;
            _searchCancellationTokenSource = null;
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
                var pms = parameters as object[];
                if (pms.GetUpperBound(0) < 3)
                    throw new ArgumentException("parameters have not all the parameters");

                var dir = pms[0].ToString();
                var ic = pms[1] as ImagesCollection;
                var fileMasks = pms[2] as IReadOnlyCollection<string>;
                var so = pms[3] as SearchImagesOptions;
                CancellationToken ct = (CancellationToken) pms[4];
                Search(dir, ic, fileMasks, so, ct);
                RiseSearchFinishedInternal();
        }

        private void RiseSearchFinished()
        {
            SearchFinished?.Invoke();
            RiseImagesCountChanged();
        }

        private void RiseSearchFinishedInternal()
        {
            SearchFinishedInternal?.Invoke();
        }

        private void RiseImagesCountChanged()
        {
            ImagesCountChanged?.Invoke();
        }

        private void RiseSearchStarted()
        {
            SearchStarted?.Invoke();
        }

        private void RiseSearchStoped()
        {
            SearchAborted?.Invoke();
        }
    }
}